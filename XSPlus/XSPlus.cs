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
            Content.Init(this.Helper.Content);
            Events.Init(this.Helper.Events);
            Input.Init(this.Helper.Input);
            Locale.Init(this.Helper.Translation);

            // Services
            var serviceManager = ServiceManager.GetSingleton();
            serviceManager.AddSingleton<ModConfigService>(this.Helper, this.ModManifest);
            serviceManager.AddSingleton<ItemGrabMenuConstructedService>();
            serviceManager.AddSingleton<ItemGrabMenuChangedService>();
            serviceManager.AddSingleton<ItemGrabMenuSideButtonsService>();
            serviceManager.AddSingleton<RenderingActiveMenuService>();
            serviceManager.AddSingleton<RenderedActiveMenuService>();
            serviceManager.AddSingleton<DisplayedInventoryService>();
            serviceManager.AddSingleton<HighlightItemsService>();

            // Features
            serviceManager.AddSingleton<AccessCarriedFeature>();
            serviceManager.AddSingleton<CapacityFeature>();
            serviceManager.AddSingleton<CategorizeChestFeature>();
            serviceManager.AddSingleton<ColorPickerFeature>();
            serviceManager.AddSingleton<CraftFromChestFeature>();
            serviceManager.AddSingleton<ExpandedMenuFeature>();
            serviceManager.AddSingleton<FilterItemsFeature>();
            serviceManager.AddSingleton<InventoryTabsFeature>();
            serviceManager.AddSingleton<SearchItemsFeature>();
            serviceManager.AddSingleton<StashToChestFeature>();
            serviceManager.AddSingleton<UnbreakableFeature>();
            serviceManager.AddSingleton<UnplaceableFeature>();
            serviceManager.AddSingleton<VacuumItemsFeature>();

            // Activate
            serviceManager.ActivateFeatures();

            this._api = new XSPlusAPI(serviceManager);
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }
    }
}