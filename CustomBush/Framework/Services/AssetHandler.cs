namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.CustomBush.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for monitoring and logging.</param>
    public AssetHandler(ILog log, IGameContentHelper gameContentHelper, IManifest manifest, IModEvents modEvents)
        : base(log, manifest)
    {
        this.DataPath = this.ModId + "/Data";
        this.gameContentHelper = gameContentHelper;
        modEvents.Content.AssetRequested += this.OnAssetRequested;
    }

    /// <summary>Gets the game path to Custom Bush data.</summary>
    public string DataPath { get; }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.DataPath))
        {
            e.LoadFrom(
                () => new Dictionary<string, BushModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }

    private Dictionary<string, BushModel> GetBushData()
    {
        var gameData = this.gameContentHelper.Load<Dictionary<string, BushModel>>(this.DataPath);
        foreach (var key in gameData.Keys)
        {
            this.Log.Trace("Custom Bush loaded a data model for {0}.", key);
        }

        return gameData;
    }
}