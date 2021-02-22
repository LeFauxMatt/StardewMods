using System.Collections.Generic;
using System.Linq;
using ImJustMatt.ExpandedStorage.Framework.Integrations;
using ImJustMatt.ExpandedStorage.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace ImJustMatt.ExpandedStorage.Framework
{
    internal class ModConfig
    {
        /// <summary>Control scheme for Expanded Storage features.</summary>
        public ModConfigKeys Controls { get; set; } = new();

        /// <summary>Default config for unconfigured storages.</summary>
        public Storage DefaultStorage { get; set; } = new()
        {
            Tabs = new List<string> {"Crops", "Seeds", "Materials", "Cooking", "Fishing", "Equipment", "Clothing", "Misc"}
        };

        /// <summary>Default tabs for unconfigured storages.</summary>
        public IDictionary<string, StorageTab> DefaultTabs { get; set; } = new Dictionary<string, StorageTab>
        {
            {
                "Clothing", new StorageTab("Shirts.png",
                    "category_clothing",
                    "category_boots", "category_hat")
            },
            {
                "Cooking",
                new StorageTab("Cooking.png",
                    "category_syrup",
                    "category_artisan_goods",
                    "category_ingredients",
                    "category_sell_at_pierres_and_marnies",
                    "category_sell_at_pierres",
                    "category_meat",
                    "category_cooking",
                    "category_milk",
                    "category_egg")
            },
            {
                "Crops",
                new StorageTab("Crops.png",
                    "category_greens",
                    "category_flowers",
                    "category_fruits",
                    "category_vegetable")
            },
            {
                "Equipment",
                new StorageTab("Tools.png",
                    "category_equipment",
                    "category_ring",
                    "category_tool",
                    "category_weapon")
            },
            {
                "Fishing",
                new StorageTab("Fish.png",
                    "category_bait",
                    "category_fish",
                    "category_tackle",
                    "category_sell_at_fish_shop")
            },
            {
                "Materials",
                new StorageTab("Minerals.png",
                    "category_monster_loot",
                    "category_metal_resources",
                    "category_building_resources",
                    "category_minerals",
                    "category_crafting",
                    "category_gem")
            },
            {
                "Misc",
                new StorageTab("Misc.png",
                    "category_big_craftable",
                    "category_furniture",
                    "category_junk")
            },
            {
                "Seeds",
                new StorageTab("Seeds.png",
                    "category_seeds",
                    "category_fertilizer")
            }
        };

        /// <summary>Only vacuum to storages in the first row of player inventory.</summary>
        public bool VacuumToFirstRow { get; set; } = true;

        /// <summary>Adds three extra rows to the Inventory Menu.</summary>
        public bool ExpandInventoryMenu { get; set; } = true;

        /// <summary>Symbol used to search items by context tags.</summary>
        public string SearchTagSymbol { get; set; } = "#";

        protected internal string SummaryReport =>
            "Expanded Storage Configuration\n" +
            $"\tResize Menu        : {ExpandInventoryMenu}\n" +
            $"\tSearch Tag Symbol  : {SearchTagSymbol}\n" +
            $"\tVacuum First Row   : {VacuumToFirstRow}\n" +
            $"\tNext Tab           : {Controls.NextTab}\n" +
            $"\tPrevious Tab       : {Controls.PreviousTab}\n" +
            $"\tScroll Up          : {Controls.ScrollUp}\n" +
            $"\tScroll Down        : {Controls.ScrollDown}\n" +
            $"\tShow Crafting      : {Controls.OpenCrafting}";

        public static void RegisterModConfig(IManifest modManifest, IGenericModConfigMenuAPI modConfigApi, ModConfig config)
        {
            modConfigApi.RegisterLabel(modManifest,
                "Controls",
                "Controller/Keyboard controls");

            modConfigApi.RegisterSimpleOption(modManifest,
                "Scroll Up",
                "Button for scrolling up",
                () => config.Controls.ScrollUp.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.ScrollUp = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Scroll Down",
                "Button for scrolling down",
                () => config.Controls.ScrollDown.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.ScrollDown = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Previous Tab",
                "Button for switching to the previous tab",
                () => config.Controls.PreviousTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.PreviousTab = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(modManifest,
                "Next Tab",
                "Button for switching to the next tab",
                () => config.Controls.NextTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => config.Controls.NextTab = KeybindList.ForSingle(value));

            modConfigApi.RegisterLabel(modManifest,
                "Tweaks",
                "Modify behavior for certain features");

            modConfigApi.RegisterSimpleOption(modManifest,
                "Search Symbol",
                "Symbol used to search items by context tag",
                () => config.SearchTagSymbol,
                value => config.SearchTagSymbol = value);
            modConfigApi.RegisterSimpleOption(modManifest,
                "Vacuum To First Row",
                "Uncheck to allow vacuuming to any chest in player inventory",
                () => config.VacuumToFirstRow,
                value => config.VacuumToFirstRow = value);
        }
    }
}