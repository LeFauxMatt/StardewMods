namespace StardewMods.SpritePatcher.Framework.Services;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Models.Events;

/// <summary>Managing the code which is compiled by this mod from content pack data.</summary>
internal sealed class CodeManager : BaseService
{
    private static readonly CSharpCompilationOptions CompileOptions = new(
        OutputKind.DynamicallyLinkedLibrary,
        generalDiagnosticOption: ReportDiagnostic.Error,
        metadataImportOptions: MetadataImportOptions.All);

    private static readonly IComparer<int> Comparer = new DescendingComparer();

    private readonly IDictionary<string, SortedDictionary<int, IList<IPatchModel>>> patches =
        new Dictionary<string, SortedDictionary<int, IList<IPatchModel>>>(StringComparer.OrdinalIgnoreCase);

    private readonly string assetPath;
    private readonly IEventManager eventManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly IManifest manifest;
    private readonly IMonitor monitor;
    private readonly IModRegistry modRegistry;
    private readonly string path;
    private readonly List<MetadataReference> references = [];
    private readonly string template;

    /// <summary>Initializes a new instance of the <see cref="CodeManager" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public CodeManager(
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        IModHelper modHelper,
        IMonitor monitor,
        IModRegistry modRegistry)
        : base(log, manifest)
    {
        this.assetPath = this.ModId + "/Patches";
        this.eventManager = eventManager;
        this.gameContentHelper = gameContentHelper;
        this.manifest = manifest;
        this.monitor = monitor;
        this.modRegistry = modRegistry;
        this.template = File.ReadAllText(Path.Join(modHelper.DirectoryPath, "assets/ConditionalTextureTemplate.cs"));
        this.path = Path.Combine(modHelper.DirectoryPath, "_generated");
        if (!Directory.Exists(this.path))
        {
            Directory.CreateDirectory(this.path);
        }

        this.CompileReferences();
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Tries to get the conditional textures for the given target.</summary>
    /// <param name="target">The target for which the data is requested.</param>
    /// <param name="data">When this method returns, contains the data for the target if it is found; otherwise, null.</param>
    /// <returns>true if the data for the target is found; otherwise, false.</returns>
    public bool TryGet(string target, [NotNullWhen(true)] out IEnumerable<IPatchModel>? data)
    {
        if (!this.patches.TryGetValue(target, out var prioritizedPatches))
        {
            data = null;
            return false;
        }

        data = prioritizedPatches.SelectMany(patchModels => patchModels.Value);
        return true;
    }

    private bool TryCompile(string id, string code, [NotNullWhen(true)] out Assembly? assembly)
    {
        this.Log.Trace("Compiling code for {0}", id);
        var filename = $"v{this.manifest.Version}_{id}_{code.Length}_{Game1.hash.GetDeterministicHashCode(code)}";
        var fullPath = Path.Combine(this.path, $"{filename}.dll");
        if (File.Exists(fullPath))
        {
            this.Log.Trace("Code already compiled for {0}", id);
            assembly = Assembly.LoadFrom(fullPath);
            return true;
        }

        var output = this.template;
        output = output.Replace("#REPLACE_namespace", id);
        output = output.Replace("#REPLACE_code", code);
        output = output.Replace('`', '"');

        var compiledCode = CSharpCompilation.Create(
            filename,
            new[] { CSharpSyntaxTree.ParseText(output) },
            this.references,
            CodeManager.CompileOptions);

        using var memoryStream = new MemoryStream();
        var result = compiledCode.Emit(memoryStream);

        if (!result.Success)
        {
            this.Log.Error("Failed to compile code for {0}", id);
            assembly = null;
            return false;
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes(fullPath, memoryStream.ToArray());
        memoryStream.Seek(0, SeekOrigin.Begin);

        assembly = Assembly.LoadFrom(fullPath);
        return true;
    }

    private void CompileReferences()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (assembly.FullName?.Contains("HarmonyLib") == true)
                {
                    continue;
                }

                var reference = MetadataReference.CreateFromFile(assembly.Location);
                this.references.Add(reference);
            }
            catch (Exception e)
            {
                this.Log.Trace("Failed to load assembly: {0}\nError: {1}", assembly, e.Message);
            }
        }
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.assetPath))
        {
            e.LoadFrom(static () => new Dictionary<string, ContentModel>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(this.assetPath)))
        {
            this.eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) =>
        this.eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);

    private void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        this.eventManager.Unsubscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        this.patches.Clear();
        var contentModels = this.gameContentHelper.Load<Dictionary<string, ContentModel>>(this.assetPath);

        foreach (var (key, contentModel) in contentModels)
        {
            var parts = PathUtilities.GetSegments(key);
            if (parts.Length != 2)
            {
                this.Log.Warn("Failed to load patch: {0}.\nInvalid id.", key);
                continue;
            }

            var modId = parts[0];
            var modInfo = this.modRegistry.Get(modId);
            if (modInfo == null)
            {
                this.Log.Warn("Failed to load patch: {0}.\nMod not found.", modInfo);
                continue;
            }

            if (!this.TryCompile(modId, contentModel.Code, out var assembly))
            {
                this.Log.Warn("Failed to load patch: {0}.\nFailed to compile code.", key);
                continue;
            }

            try
            {
                var type = assembly.GetType($"{modId}.Runner");
                var ctor = type!.GetConstructor(
                [
                    typeof(IMonitor),
                    typeof(string),
                    typeof(IContentPack),
                    typeof(string),
                    typeof(Rectangle?),
                    typeof(List<DrawMethod>),
                    typeof(PatchMode),
                ]);

                var contentPack = (IContentPack)modInfo.GetType().GetProperty("ContentPack")!.GetValue(modInfo)!;
                var patchModel = (BasePatchModel)ctor!.Invoke(
                [
                    this.monitor,
                    modId,
                    contentPack,
                    contentModel.Target,
                    contentModel.Area,
                    contentModel.DrawMethods,
                    contentModel.PatchMode,
                ]);

                if (!this.patches.TryGetValue(contentModel.Target, out var prioritizedPatches))
                {
                    prioritizedPatches = new SortedDictionary<int, IList<IPatchModel>>(CodeManager.Comparer);
                    this.patches[contentModel.Target] = prioritizedPatches;
                }

                if (!prioritizedPatches.TryGetValue(contentModel.Priority, out var patchModels))
                {
                    patchModels = new List<IPatchModel>();
                    prioritizedPatches[contentModel.Priority] = patchModels;
                }

                patchModels.Add(patchModel);
            }
            catch (Exception ex)
            {
                this.Log.Warn("Failed to load patch: {0}.\nError: {1}", key, ex.Message);
            }
        }

        this.eventManager.Publish(new PatchesChangedEventArgs(this.patches.Keys.ToList()));
    }

    private sealed class DescendingComparer : IComparer<int>
    {
        /// <inheritdoc />
        public int Compare(int x, int y) => y.CompareTo(x);
    }
}