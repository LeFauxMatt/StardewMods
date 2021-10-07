namespace XSPlus.Services
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common.Helpers;
    using CommonHarmony.Services;
    using Features;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    internal class InfoDumpService : BaseService
    {
        private static InfoDumpService Instance;
        private readonly ICommandHelper _commandHelper;
        private readonly ModConfigService _modConfigService;
        private readonly ServiceManager _serviceManager;

        internal InfoDumpService(ServiceManager serviceManager, ModConfigService modConfigService, ICommandHelper commandHelper)
            : base("InfoDumpService")
        {
            this._serviceManager = serviceManager;
            this._modConfigService = modConfigService;
            this._commandHelper = commandHelper;

            // Events
            Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="InfoDumpService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="InfoDumpService" /> class.</returns>
        public static async Task<InfoDumpService> Create(ServiceManager serviceManager)
        {
            return InfoDumpService.Instance ??= new(
                serviceManager,
                await serviceManager.Get<ModConfigService>(),
                serviceManager.Helper.ConsoleCommands);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this._commandHelper.Add("xs_dump", "Dumps a bunch of info to the logs.", this.DumpInfo);
        }

        private void DumpInfo(string command, string[] args)
        {
            var features = this._serviceManager.GetAll<BaseFeature>();
            var info = new StringBuilder();

            foreach (var feature in features)
            {
                var globalConfig = this._modConfigService.ModConfig.Global.TryGetValue(feature.FeatureName, out var option) switch
                {
                    true when option => "+",
                    true => "-",
                    false => " ",
                };

                info.Append(
                    @$"
{globalConfig}{feature.FeatureName}");

                switch (feature)
                {
                    case CapacityFeature:
                        info.Append(
                            @$"
    Global Capacity: {this._modConfigService.ModConfig.Capacity.ToString()}");

                        break;
                    case CraftFromChestFeature:
                        info.Append(
                            $@"
    Crafting Button: {this._modConfigService.ModConfig.OpenCrafting}
    Crafting Range: {this._modConfigService.ModConfig.CraftingRange}");

                        break;
                    case ExpandedMenuFeature:
                        info.Append(
                            $@"
    Menu Rows: {this._modConfigService.ModConfig.MenuRows.ToString()}
    Scroll Up: {this._modConfigService.ModConfig.ScrollUp}
    Scroll Down: {this._modConfigService.ModConfig.ScrollDown}");

                        break;
                    case InventoryTabsFeature inventoryTabsFeature:
                        info.Append(
                            $@"
    Previous Tab: {this._modConfigService.ModConfig.PreviousTab}
    Next Tab: {this._modConfigService.ModConfig.NextTab}
    Tabs:");

                        foreach (var tab in inventoryTabsFeature.Tabs)
                        {
                            info.Append(
                                $@"
    - {tab.Name}: {string.Join(",", tab.Tags)}");
                        }

                        break;
                    case SearchItemsFeature:
                        info.Append(
                            $@"
    Search Tag Symbol: {this._modConfigService.ModConfig.SearchTagSymbol}");

                        break;
                    case StashToChestFeature:
                        info.Append(
                            $@"
    Stashing Button: {this._modConfigService.ModConfig.StashItems}
    Stashing Range: {this._modConfigService.ModConfig.StashingRange}");

                        break;
                }
            }

            Log.Info(info.ToString());

            void ChestSummary(Chest chest, string location)
            {
                info.Clear();
                info.Append(
                    $@"
{chest.DisplayName} in {location}");

                foreach (var feature in features.OrderBy(feature => !feature.IsEnabledForItem(chest)))
                {
                    var config = feature.IsEnabledForItem(chest) ? "+" : "-";
                    info.Append(
                        $@"
{config}{feature.FeatureName}");

                    if (config == "-")
                    {
                        continue;
                    }

                    switch (feature)
                    {
                        case CapacityFeature capacityFeature:
                            if (capacityFeature.TryGetValueForItem(chest, out var capacity))
                            {
                                info.Append(
                                    @$"
    Capacity: {capacity.ToString()}");
                            }

                            break;
                        case CategorizeChestFeature:
                            var chestFilterItems = chest.GetFilterItems();
                            if (!string.IsNullOrWhiteSpace(chestFilterItems))
                            {
                                info.Append(
                                    $@"
    Filter Items: {chestFilterItems}");
                            }

                            break;
                        case ColorPickerFeature:
                            info.Append(
                                $@"
    Chest Color: {chest.playerChoiceColor.Value.ToString()}");

                            break;
                        case CraftFromChestFeature craftFromChestFeature:
                            if (craftFromChestFeature.TryGetValueForItem(chest, out var craftingRange))
                            {
                                info.Append(
                                    $@"
    Crafting Range: {craftingRange}");
                            }

                            break;
                        case FilterItemsFeature filterItemsFeature:
                            if (filterItemsFeature.TryGetValueForItem(chest, out var filterItems))
                            {
                                var includeItems = filterItems.Where(filterItem => filterItem.Value).Select(filterItem => filterItem.Key).ToList();
                                var excludeItems = filterItems.Where(filterItem => !filterItem.Value).Select(filterItem => filterItem.Key).ToList();

                                if (includeItems.Any())
                                {
                                    info.Append(
                                        $@"
    Included Items: {string.Join(",", includeItems)}");
                                }

                                if (excludeItems.Any())
                                {
                                    info.Append(
                                        $@"
    Excluded Items: {string.Join(",", excludeItems)}");
                                }
                            }

                            break;
                        case StashToChestFeature stashToChestFeature:
                            if (stashToChestFeature.TryGetValueForItem(chest, out var stashingRange))
                            {
                                info.Append(
                                    $@"
    Crafting Range: {stashingRange}");
                            }

                            break;
                    }
                }

                Log.Info(info.ToString());
            }

            for (var i = 0; i < Game1.player.Items.Count; i++)
            {
                if (Game1.player.Items[i] is Chest chest)
                {
                    ChestSummary(chest, $"player.{Game1.player.Name}.Items.{i.ToString()}");
                }
            }

            foreach (var location in XSPlus.AccessibleLocations)
            {
                foreach (var obj in location.Objects.Pairs)
                {
                    if (obj.Value is Chest chest)
                    {
                        ChestSummary(chest, $"GameLocation.{location.Name}.Objects.{obj.Key.ToString()}");
                    }
                }
            }
        }
    }
}