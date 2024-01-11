namespace StardewMods.FuryCore.Framework.Services;

using HarmonyLib;
using StardewMods.Common.Enums;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.FuryCore.IPatchManager" />
internal sealed class PatchManager : BaseService<PatchManager>, IPatchManager
{
    private readonly Lazy<Harmony> harmony;
    private readonly Dictionary<string, List<ISavedPatch>> savedPatches = new();

    /// <summary>Initializes a new instance of the <see cref="PatchManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public PatchManager(ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.harmony = new Lazy<Harmony>(() => new Harmony(this.ModId));

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches)
    {
        if (!this.savedPatches.TryGetValue(id, out var list))
        {
            list = new List<ISavedPatch>();
            this.savedPatches.Add(id, list);
        }

        foreach (var patch in patches)
        {
            list.Add(patch);
        }
    }

    /// <inheritdoc />
    public void Patch(string id)
    {
        if (!this.savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        foreach (var patch in patches)
        {
            try
            {
                this.Log.Trace(
                    "Patching {0}.{1} with {2}.{3} {4}.",
                    patch.Original.DeclaringType!.Name,
                    patch.Original.Name,
                    patch.Patch.DeclaringType!.Name,
                    patch.Patch.Name,
                    patch.Type.ToStringFast());

                switch (patch.Type)
                {
                    case PatchType.Prefix:
                        this.harmony.Value.Patch(patch.Original, new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Postfix:
                        this.harmony.Value.Patch(patch.Original, postfix: new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Transpiler:
                        this.harmony.Value.Patch(patch.Original, transpiler: new HarmonyMethod(patch.Patch));
                        continue;
                    case PatchType.Finalizer:
                        this.harmony.Value.Patch(patch.Original, finalizer: new HarmonyMethod(patch.Patch));
                        continue;
                }
            }
            catch (Exception e)
            {
                this.Log.Warn("Patch {0} failed with exception: {1}", patch.LogId ?? patch.Patch.Name, e.Message);
            }
        }
    }

    /// <inheritdoc />
    public void Unpatch(string id)
    {
        if (!this.savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        foreach (var patch in patches)
        {
            this.Log.Trace("Unpatching {0} with {1}.", patch.Original.Name, patch.Patch.Name);
            this.harmony.Value.Unpatch(patch.Original, patch.Patch);
        }
    }
}