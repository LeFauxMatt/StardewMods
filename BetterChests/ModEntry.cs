namespace BetterChests
{
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony.Services;
    using Services;
    using StardewModdingAPI;

    /// <inheritdoc />
    public class ModEntry : Mod
    {
        private ServiceManager _serviceManager;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            // Init
            Log.Init(this.Monitor);
            this._serviceManager = new(this.Helper, this.ModManifest);

            // Services
            this._serviceManager.Create(new []
            {
                typeof(CarryChestService),
                typeof(ChestTabsService),
                typeof(DisplayedInventoryService),
                typeof(HarmonyService),
                typeof(ItemGrabMenuChangedService),
                typeof(ManagedChestService),
                typeof(ModConfigService),
                typeof(OpenHeldChestService),
                typeof(RenderedActiveMenuService),
                typeof(RenderingActiveMenuService),
                typeof(ResizeChestMenuService),
                typeof(ResizeChestService),
            });

            this._serviceManager.GetByType<CarryChestService>().Activate();
            this._serviceManager.GetByType<ChestTabsService>().Activate();
            this._serviceManager.GetByType<OpenHeldChestService>().Activate();
            this._serviceManager.GetByType<ResizeChestMenuService>().Activate();
            this._serviceManager.GetByType<ResizeChestService>().Activate();
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new BetterChestsApi(this._serviceManager);
        }
    }
}