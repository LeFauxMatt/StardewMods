namespace XSPlus
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.XSPlus;
    using Common.Services;
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

        private static XSPlus Instance;
        private IXSPlusAPI _api;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XSPlus" /> class.
        /// </summary>
        public XSPlus()
        {
            XSPlus.Instance = this;
        }

        internal ServiceManager ServiceManager { get; private set; }

        /// <summary>Gets placed Chests that are accessible to the player.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        public static IEnumerable<Chest> AccessibleChests
        {
            get
            {
                IList<Chest> chests = XSPlus.AccessibleLocations.SelectMany(location => location.Objects.Values.OfType<Chest>()).ToList();
                return chests;
            }
        }

        public static IEnumerable<GameLocation> AccessibleLocations
        {
            get
            {
                var locations = Context.IsMainPlayer
                    ? Game1.locations.Concat(Game1.locations.OfType<BuildableGameLocation>().SelectMany(location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)))
                    : XSPlus.Instance.Helper.Multiplayer.GetActiveLocations();

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

            this.ServiceManager = new(this.Helper, this.ModManifest);

            // Services
            this.ServiceManager.Create<DisplayedInventoryService>();
            this.ServiceManager.Create<HarmonyService>();
            this.ServiceManager.Create<HighlightItemsService>();
            this.ServiceManager.Create<InfoDumpService>();
            this.ServiceManager.Create<ItemGrabMenuChangedService>();
            this.ServiceManager.Create<ItemGrabMenuSideButtonsService>();
            this.ServiceManager.Create<ModConfigService>();
            this.ServiceManager.Create<RenderingActiveMenuService>();
            this.ServiceManager.Create<RenderedActiveMenuService>();

            // Features
            this.ServiceManager.Create<AccessCarriedFeature>();
            this.ServiceManager.Create<BiggerChestFeature>();
            this.ServiceManager.Create<CapacityFeature>();
            this.ServiceManager.Create<CarryChestFeature>();
            this.ServiceManager.Create<CategorizeChestFeature>();
            this.ServiceManager.Create<ColorPickerFeature>();
            this.ServiceManager.Create<CraftFromChestFeature>();
            this.ServiceManager.Create<ExpandedMenuFeature>();
            this.ServiceManager.Create<FilterItemsFeature>();
            this.ServiceManager.Create<InventoryTabsFeature>();
            this.ServiceManager.Create<OpenNearbyFeature>();
            this.ServiceManager.Create<SearchItemsFeature>();
            this.ServiceManager.Create<StashToChestFeature>();
            this.ServiceManager.Create<UnbreakableFeature>();
            this.ServiceManager.Create<UnplaceableFeature>();
            this.ServiceManager.Create<VacuumItemsFeature>();

            this.ServiceManager.ResolveDependencies();

            // Activate
            this.ServiceManager.ActivateFeatures();

            this._api = new XSPlusAPI(this);
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }
    }
}