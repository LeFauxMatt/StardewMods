namespace StardewMods.SpritePatcher.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Models.Events;

/// <summary>Manages the data model for sprite patches.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly string assetPath;
    private readonly IEventManager eventManager;
    private readonly IGameContentHelper gameContentHelper;

    private Dictionary<string, PatchData> allPatches = [];
    private Dictionary<string, SortedList<int, PatchData>> loadedPatches = [];
    private bool checkForChanges = true;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public AssetHandler(IEventManager eventManager, IGameContentHelper gameContentHelper, ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.assetPath = this.ModId + "/Patches";
        this.eventManager = eventManager;
        this.gameContentHelper = gameContentHelper;
        eventManager.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventManager.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventManager.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Tries to get the data for the given target.</summary>
    /// <param name="target">The target for which the data is requested.</param>
    /// <param name="patches">When this method returns, contains the data for the target if it is found; otherwise, null.</param>
    /// <returns>true if the data for the target is found; otherwise, false.</returns>
    public bool TryGetData(string target, [NotNullWhen(true)] out SortedList<int, PatchData>? patches)
    {
        if (!this.checkForChanges)
        {
            return this.loadedPatches.TryGetValue(target, out patches);
        }

        this.checkForChanges = false;
        var newPatches = this.gameContentHelper.Load<Dictionary<string, PatchData>>(this.assetPath);
        if (this.allPatches.Count == newPatches.Count && newPatches.Keys.All(this.allPatches.ContainsKey))
        {
            return this.loadedPatches.TryGetValue(target, out patches);
        }

        var keys = newPatches
            .Keys.Except(this.allPatches.Keys)
            .Concat(this.allPatches.Keys.Except(newPatches.Keys))
            .ToList();

        var targets = newPatches
            .Concat(this.allPatches)
            .Where(pair => keys.Contains(pair.Key))
            .Select(pair => pair.Value.BaseTarget)
            .Distinct()
            .ToList();

        this.allPatches = newPatches;
        this.ReloadPatches();
        this.eventManager.Publish(new PatchesChangedEventArgs(targets));
        return this.loadedPatches.TryGetValue(target, out patches);
    }

    private void ReloadPatches()
    {
        var data = new Dictionary<string, SortedList<int, PatchData>>();
        foreach (var (_, patch) in this.allPatches)
        {
            if (!data.TryGetValue(patch.BaseTarget, out var patches))
            {
                patches = new SortedList<int, PatchData>(new DescendingComparer());
                data[patch.BaseTarget] = patches;
            }

            patches.Add(patch.Priority, patch);
        }

        if (this.loadedPatches.Count == data.Count && !data.Keys.All(this.loadedPatches.ContainsKey))
        {
            return;
        }

        this.loadedPatches = data;
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.assetPath))
        {
            e.LoadFrom(static () => new Dictionary<string, PatchData>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(this.assetPath)))
        {
            this.checkForChanges = true;
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) => this.checkForChanges = true;

    private sealed class DescendingComparer : IComparer<int>
    {
        /// <inheritdoc />
        public int Compare(int x, int y) => y.CompareTo(x);
    }
}