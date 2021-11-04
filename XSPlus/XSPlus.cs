namespace XSPlus
{
    using System;
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

        private static Func<IEnumerable<GameLocation>> GetActiveLocations;

        private IXSPlusApi _api;

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
                    : XSPlus.GetActiveLocations();

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

            // Init
            Log.Init(this.Monitor);
            this.ServiceManager = new(this.Helper, this.ModManifest);
            XSPlus.GetActiveLocations = this.Helper.Multiplayer.GetActiveLocations;

            // Services
            this.ServiceManager.Create(new []
            {
                typeof(DisplayedInventoryService),
                typeof(HarmonyService),
                typeof(HighlightItemsService),
                typeof(InfoDumpService),
                typeof(ItemGrabMenuChangedService),
                typeof(ItemGrabMenuSideButtonsService),
                typeof(ModConfigService),
                typeof(RenderedActiveMenuService),
                typeof(RenderingActiveMenuService),
            });

            // Features
            this.ServiceManager.Create(new []
            {
                typeof(AccessCarriedFeature),
                typeof(BiggerChestFeature),
                typeof(CapacityFeature),
                typeof(CarryChestFeature),
                typeof(CategorizeChestFeature),
                typeof(ColorPickerFeature),
                typeof(CraftFromChestFeature),
                typeof(ExpandedMenuFeature),
                typeof(FilterItemsFeature),
                typeof(InventoryTabsFeature),
                typeof(OpenNearbyFeature),
                typeof(SearchItemsFeature),
                typeof(StashToChestFeature),
                typeof(UnbreakableFeature),
                typeof(UnplaceableFeature),
                typeof(VacuumItemsFeature),
            });

            // Activate
            this.ServiceManager.ActivateFeatures();

            this._api = new XSPlusApi(this);
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }
    }
}