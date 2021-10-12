namespace XSPlus.Services
{
    using System.Linq;
    using System.Text;
    using Common.Helpers;
    using Common.Services;
    using Features;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Objects;

    internal class InfoDumpService : BaseService
    {
        private readonly ServiceManager _serviceManager;
        private ModConfigService _modConfig;

        private InfoDumpService(ServiceManager serviceManager)
            : base("InfoDumpService")
        {
            // Init
            this._serviceManager = serviceManager;
            serviceManager.Helper.ConsoleCommands.Add("xs_dump", "Dumps a bunch of info to the logs.", this.DumpInfo);

            // Dependencies
            this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);
        }

        private void DumpInfo(string command, string[] args)
        {
            var features = this._serviceManager.GetAll<BaseFeature>();
            var info = new StringBuilder();

            foreach (var feature in features)
            {
                var globalConfig = this._modConfig.ModConfig.Global.TryGetValue(feature.ServiceName, out var option) switch
                {
                    true when option => "+",
                    true => "-",
                    false => " ",
                };

                info.Append(
                    @$"
{globalConfig}{feature.ServiceName}");

                switch (feature)
                {
                    case CapacityFeature:
                        info.Append(
                            @$"
    Global Capacity: {this._modConfig.ModConfig.Capacity.ToString()}");

                        break;
                    case CraftFromChestFeature:
                        info.Append(
                            $@"
    Crafting Button: {this._modConfig.ModConfig.OpenCrafting}
    Crafting Range: {this._modConfig.ModConfig.CraftingRange}");

                        break;
                    case ExpandedMenuFeature:
                        info.Append(
                            $@"
    Menu Rows: {this._modConfig.ModConfig.MenuRows.ToString()}
    Scroll Up: {this._modConfig.ModConfig.ScrollUp}
    Scroll Down: {this._modConfig.ModConfig.ScrollDown}");

                        break;
                    case InventoryTabsFeature inventoryTabsFeature:
                        info.Append(
                            $@"
    Previous Tab: {this._modConfig.ModConfig.PreviousTab}
    Next Tab: {this._modConfig.ModConfig.NextTab}
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
    Search Tag Symbol: {this._modConfig.ModConfig.SearchTagSymbol}");

                        break;
                    case StashToChestFeature:
                        info.Append(
                            $@"
    Stashing Button: {this._modConfig.ModConfig.StashItems}
    Stashing Range: {this._modConfig.ModConfig.StashingRange}");

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
{config}{feature.ServiceName}");

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
                            if (chest.playerChoiceColor.Value != Color.Black)
                            {
                                info.Append(
                                    $@"
    Chest Color: {chest.playerChoiceColor.Value.ToString()}");
                            }

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