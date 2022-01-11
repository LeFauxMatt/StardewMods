namespace MoreChests;

using Common.Helpers;
using Common.Integrations.MoreChests;
using Services;
using StardewModdingAPI;

public class ModEntry : Mod
{
    internal const string ModPrefix = "MoreChests";
    private IMoreChestsApi _api;

    internal ServiceLocator ServiceLocator { get; private set; }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Init(this.Monitor);
        this.ServiceLocator = new(this.Helper, this.ModManifest);
        this._api = new MoreChestsApi(this);

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