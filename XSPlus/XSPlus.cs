namespace XSPlus
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Enums;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Integrations.XSPlus;
    using Common.Services;
    using Features;
    using HarmonyLib;
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
            Log.Init(this.Monitor);

            // Services
            var harmony = new Harmony(this.ModManifest.UniqueID);
            var modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);
            var modConfigService = new ModConfigService(this.Helper, modConfigMenu, this.ModManifest);
            var itemGrabMenuConstructedService = new ItemGrabMenuConstructedService(harmony);
            var itemGrabMenuChangedService = new ItemGrabMenuChangedService(this.Helper.Events.Display);
            var renderingActiveMenuService = new RenderingActiveMenuService(this.Helper.Events.Display, itemGrabMenuChangedService);
            var renderedActiveMenuService = new RenderedActiveMenuService(this.Helper.Events.Display, itemGrabMenuChangedService);
            var highlightPlayerItemsService = new HighlightItemsService(itemGrabMenuConstructedService, InventoryType.Player);
            var displayedChestInventoryService = DisplayedInventoryService.Init(harmony, itemGrabMenuConstructedService, InventoryType.Chest);

            // Features
            this._featureManager = FeatureManager.Init(this.Helper, harmony, modConfigService);
            this._featureManager.AddFeature(new AccessCarriedFeature(this.Helper.Input));
            this._featureManager.AddFeature(new CapacityFeature(modConfigService));
            this._featureManager.AddFeature(new CategorizeChestFeature(this.Helper, modConfigService, itemGrabMenuChangedService, renderedActiveMenuService));
            this._featureManager.AddFeature(new ColorPickerFeature(this.Helper.Content, this.Helper.Events.Input, itemGrabMenuConstructedService, itemGrabMenuChangedService, renderedActiveMenuService));
            this._featureManager.AddFeature(new CraftFromChestFeature(this.Helper.Input, this.Helper.Events.GameLoop, modConfigService));
            this._featureManager.AddFeature(new ExpandedMenuFeature(this.Helper.Input, this.Helper.Events.Input, modConfigService, itemGrabMenuConstructedService, itemGrabMenuChangedService, displayedChestInventoryService));
            this._featureManager.AddFeature(new FilterItemsFeature(itemGrabMenuChangedService, highlightPlayerItemsService));
            this._featureManager.AddFeature(new InventoryTabsFeature(this.Helper.Content, this.Helper.Input, this.Helper.Translation, this.Helper.Events.Input, modConfigService, itemGrabMenuChangedService, displayedChestInventoryService, renderingActiveMenuService, renderedActiveMenuService));
            this._featureManager.AddFeature(new SearchItemsFeature(this.Helper.Content, this.Helper.Input, this.Helper.Events.GameLoop, this.Helper.Events.Input, modConfigService, itemGrabMenuConstructedService, itemGrabMenuChangedService, displayedChestInventoryService, renderedActiveMenuService));
            this._featureManager.AddFeature(new StashToChestFeature(this.Helper.Input, modConfigService));
            this._featureManager.AddFeature(new UnbreakableFeature());
            this._featureManager.AddFeature(new UnplaceableFeature());
            this._featureManager.AddFeature(new VacuumItemsFeature());

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