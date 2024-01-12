namespace StardewMods.SpritePatcher.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Models;

/// <summary>Manages the data model for sprite patches.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly string assetPath;
    private readonly IGameContentHelper gameContentHelper;

    private Dictionary<string, List<PatchData>>? patchData;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public AssetHandler(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.assetPath = this.ModId + "/Patches";
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventSubscriber.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Tries to get the data for the given target.</summary>
    /// <param name="target">The target for which the data is requested.</param>
    /// <param name="patches">When this method returns, contains the data for the target if it is found; otherwise, null.</param>
    /// <returns>true if the data for the target is found; otherwise, false.</returns>
    public bool TryGetData(string target, [NotNullWhen(true)] out List<PatchData>? patches)
    {
        this.patchData ??= this.LoadData();
        return this.patchData.TryGetValue(target, out patches);
    }

    private Dictionary<string, List<PatchData>> LoadData()
    {
        var data = new Dictionary<string, List<PatchData>>();
        foreach (var (_, patch) in this.gameContentHelper.Load<Dictionary<string, PatchData>>(this.assetPath))
        {
            if (!data.TryGetValue(patch.Target, out var patches))
            {
                patches = new List<PatchData>();
                data[patch.Target] = patches;
            }

            patches.Add(patch);
        }

        return data;
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.assetPath))
        {
            e.LoadFrom(() => new Dictionary<string, PatchData>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(this.assetPath)))
        {
            this.patchData = null;
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) => this.patchData = null;
}