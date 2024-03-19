namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.CustomBush.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public AssetHandler(IEventSubscriber eventSubscriber, ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.DataPath = this.ModId + "/Data";
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the game path to Custom Bush data.</summary>
    public string DataPath { get; }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.DataPath))
        {
            e.LoadFrom(
                static () => new Dictionary<string, BushModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }
}