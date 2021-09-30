namespace XSPlus
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Enums;
    using Common.Helpers;
    using Common.Integrations.XSPlus;
    using CommonHarmony.Services;
    using Features;
    using Services;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Objects;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSPlus : Mod
    {
        /// <summary>Mod-specific prefix for modData.</summary>
        internal const string ModPrefix = "furyx639.ExpandedStorage";

        private static XSPlus Instance = default!;
        private readonly IXSPlusAPI _api = new XSPlusAPI();
        private ServiceManager _serviceManager = default!;
        private FeatureManager _featureManager = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="XSPlus"/> class.
        /// </summary>
        public XSPlus()
        {
            XSPlus.Instance = this;
        }

        /// <summary>Gets placed Chests that are accessible to the player.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        public static IEnumerable<Chest> AccessibleChests
        {
            get
            {
                IList<Chest> chests = XSPlus.Instance.AccessibleLocations.SelectMany(location => location.Objects.Values.OfType<Chest>()).ToList();
                return chests;
            }
        }

        private IEnumerable<GameLocation> AccessibleLocations
        {
            get
            {
                IEnumerable<GameLocation> locations = Context.IsMainPlayer
                    ? Game1.locations.Concat(
                        Game1.locations.OfType<BuildableGameLocation>().SelectMany(
                            location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)))
                    : this.Helper.Multiplayer.GetActiveLocations();
                return locations;
            }
        }

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            if (this.Helper.ModRegistry.IsLoaded("furyx639.BetterChests"))
            {
                this.Monitor.Log("BetterChests deprecates eXpanded Storage (Plus).\nRemove XSPlus from your mods folder!", LogLevel.Warn);
                return;
            }

            Log.Init(this.Monitor);
            Mixin.Init(this.ModManifest);
            Content.init(this.Helper.Content);
            Events.Init(this.Helper.Events);
            Input.Init(this.Helper.Input);
            Locale.Init(this.Helper.Translation);

            // Services
            this._serviceManager = new ServiceManager();
            this._serviceManager.AddSingleton<ModConfigService>(this.Helper, this.ModManifest);
            this._serviceManager.AddSingleton<ItemGrabMenuConstructedService>();
            this._serviceManager.AddSingleton<ItemGrabMenuChangedService>();
            this._serviceManager.AddSingleton<ItemGrabMenuSideButtonsService>();
            this._serviceManager.AddSingleton<RenderingActiveMenuService>();
            this._serviceManager.AddSingleton<RenderedActiveMenuService>();
            this._serviceManager.AddSingleton<HighlightItemsService>(InventoryType.Chest);
            this._serviceManager.AddSingleton<HighlightItemsService>(InventoryType.Player);
            this._serviceManager.AddSingleton<DisplayedInventoryService>(InventoryType.Chest);
            this._serviceManager.AddSingleton<DisplayedInventoryService>(InventoryType.Player);

            // Features
            this._featureManager = FeatureManager.GetSingleton(this._serviceManager);
            this._featureManager.AddSingleton<AccessCarriedFeature>();
            this._featureManager.AddSingleton<CapacityFeature>();
            this._featureManager.AddSingleton<CategorizeChestFeature>();
            this._featureManager.AddSingleton<ColorPickerFeature>();
            this._featureManager.AddSingleton<CraftFromChestFeature>();
            this._featureManager.AddSingleton<ExpandedMenuFeature>();
            this._featureManager.AddSingleton<FilterItemsFeature>();
            this._featureManager.AddSingleton<InventoryTabsFeature>();
            this._featureManager.AddSingleton<SearchItemsFeature>();
            this._featureManager.AddSingleton<StashToChestFeature>();
            this._featureManager.AddSingleton<UnbreakableFeature>();
            this._featureManager.AddSingleton<UnplaceableFeature>();
            this._featureManager.AddSingleton<VacuumItemsFeature>();

            // Activate
            this._featureManager.ActivateFeatures();
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }
    }
}