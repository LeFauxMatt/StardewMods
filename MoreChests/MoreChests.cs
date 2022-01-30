namespace StardewMods.MoreChests;

using Common.Helpers;
using StardewModdingAPI;
using StardewMods.FuryCore.Services;
using StardewMods.MoreChests.Services;

/// <inheritdoc />
public class MoreChests : Mod
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;

        // Services
        this.Services.Add(
            new AssetHandler(this.Services));


        // Services
        this.ServiceLocator.Create(new []
        {
            typeof(AssetHandler),
            typeof(ContentPackLoader),
            typeof(CustomChestManager),
            typeof(InventoryHandler),
            typeof(ModConfigService),
        });
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return this._api;
    }
}