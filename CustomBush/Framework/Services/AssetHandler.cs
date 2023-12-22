namespace StardewMods.CustomBush.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.CustomBush.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    private const string DataPath = "furyx639.CustomBush/Data";

    private readonly Lazy<Dictionary<string, BushModel>> data;
    private readonly IGameContentHelper gameContent;
    private readonly Logging logging;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="events">Dependency used for monitoring and logging.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    public AssetHandler(IModEvents events, IGameContentHelper gameContent, Logging logging)
    {
        this.gameContent = gameContent;
        this.logging = logging;
        this.data = new(this.GetData);
        events.Content.AssetRequested += AssetHandler.OnAssetRequested;
    }

    /// <summary>Gets CustomBush data.</summary>
    public Dictionary<string, BushModel> Data => this.data.Value;

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(AssetHandler.DataPath))
        {
            e.LoadFrom(
                () => new Dictionary<string, BushModel>(StringComparer.OrdinalIgnoreCase),
                AssetLoadPriority.Exclusive);
        }
    }

    private Dictionary<string, BushModel> GetData()
    {
        var gameData = this.gameContent.Load<Dictionary<string, BushModel>>(AssetHandler.DataPath);
        foreach (var key in gameData.Keys)
        {
            this.logging.Trace("Custom Bush loaded a data model for {0}.", key);
        }

        return gameData;
    }
}
