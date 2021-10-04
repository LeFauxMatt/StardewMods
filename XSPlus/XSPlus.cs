namespace XSPlus
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
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
                var locations = Context.IsMainPlayer
                    ? Game1.locations.Concat(Game1.locations.OfType<BuildableGameLocation>().SelectMany(location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)))
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
            Translations.Init(this.Helper.Translation);
            Reflection.Init(this.Helper.Reflection);

            var serviceManager = ServiceManager.Create(this.Helper, this.ModManifest);

            // Services
            Task.WaitAll(
                serviceManager.Create<ModConfigService>(),
                serviceManager.Create<ItemGrabMenuConstructedService>(),
                serviceManager.Create<ItemGrabMenuChangedService>(),
                serviceManager.Create<ItemGrabMenuSideButtonsService>(),
                serviceManager.Create<RenderingActiveMenuService>(),
                serviceManager.Create<RenderedActiveMenuService>(),
                serviceManager.Create<DisplayedInventoryService>(),
                serviceManager.Create<HighlightItemsService>());

            // Features
            Task.WaitAll(
                serviceManager.Create<AccessCarriedFeature>(),
                serviceManager.Create<CapacityFeature>(),
                serviceManager.Create<CategorizeChestFeature>(),
                serviceManager.Create<ColorPickerFeature>(),
                serviceManager.Create<CraftFromChestFeature>(),
                serviceManager.Create<ExpandedMenuFeature>(),
                serviceManager.Create<FilterItemsFeature>(),
                serviceManager.Create<InventoryTabsFeature>(),
                serviceManager.Create<SearchItemsFeature>(),
                serviceManager.Create<StashToChestFeature>(),
                serviceManager.Create<UnbreakableFeature>(),
                serviceManager.Create<UnplaceableFeature>(),
                serviceManager.Create<VacuumItemsFeature>());

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