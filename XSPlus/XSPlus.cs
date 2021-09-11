using System;
using System.Collections.Generic;
using Common.Integrations.GenericModConfigMenu;
using Common.Integrations.XSPlus;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using XSPlus.Features;

namespace XSPlus
{
    public class XSPlus : Mod
    {
        internal const string ModPrefix = "furyx639.ExpandedStorage";
        internal static readonly Dictionary<string, BaseFeature> Features = new();
        internal static ModConfig Config;
        private GenericModConfigMenuIntegration _modConfigMenu;
        private readonly IXSPlusAPI _api = new XSPlusAPI();
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            _modConfigMenu = new GenericModConfigMenuIntegration(helper.ModRegistry);
            
            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            
            // Patches
            var harmony = new Harmony(ModManifest.UniqueID);
            var unused = new Patches(Helper, Monitor, new Harmony(ModManifest.UniqueID));
            var featureTypes = new[]
            {
                typeof(AccessCarried),
                typeof(Capacity),
                typeof(CraftFromChest),
                typeof(ExpandedMenu),
                typeof(FilterItems),
                typeof(InventoryTabs),
                typeof(SearchItems),
                typeof(StashToChest),
                typeof(Unbreakable),
                typeof(Unplaceable),
                typeof(VacuumItems)
            };
            foreach (var featureType in featureTypes)
            {
                var featureName = featureType.Name;
                var feature = (BaseFeature) Activator.CreateInstance(featureType, featureName, Helper, Monitor, harmony);
                if (feature is null)
                    continue;
                Features.Add(featureName, feature);
                if (!Config.Global.TryGetValue(featureName, out var global) || global)
                    feature.IsDisabled = false;
            }
        }
        /// <inheritdoc />
        public override object GetApi()
        {
            return _api;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configChoices = new[] { "Default", "Enable", "Disable" };
            if (!_modConfigMenu.IsLoaded)
                return;
            
            string GetConfig(string featureName)
            {
                if (!Config.Global.TryGetValue(featureName, out var global))
                    return "Default";
                return global ? "Enable" : "Disable";
            }
            
            void SetConfig(string featureName, string value)
            {
                if (!Features.TryGetValue(featureName, out var feature))
                    return;
                switch (value)
                {
                    case "Enable":
                        Config.Global[featureName] = true;
                        break;
                    case "Disable":
                        Config.Global[featureName] = false;
                        break;
                    default:
                        Config.Global.Remove(featureName);
                        break;
                }
                feature.IsDisabled = Config.Global.TryGetValue(featureName, out var global) && !global;
            }
            
            // Register mod configuration
            _modConfigMenu.API.RegisterModConfig(
                mod: ModManifest,
                revertToDefault: () => Config = new ModConfig(),
                saveToFile: () => Helper.WriteConfig(Config)
            );
            
            // Allow config in game
            _modConfigMenu.API.SetDefaultIngameOptinValue(ModManifest, true);
            
            // Config options
            _modConfigMenu.API.RegisterLabel(ModManifest, "General", "");
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Open Crafting Button",
                optionDesc: "Key to open the crafting menu for accessible chests.",
                optionGet: () => Config.OpenCrafting,
                optionSet: value => Config.OpenCrafting = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Stash Items Button",
                optionDesc: "Key to stash items into accessible chests.",
                optionGet: () => Config.StashItems,
                optionSet: value => Config.StashItems = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Scroll Up",
                optionDesc: "Key to scroll up in expanded inventory menus.",
                optionGet: () => Config.ScrollUp,
                optionSet: value => Config.ScrollUp = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Scroll Down",
                optionDesc: "Key to scroll down in expanded inventory menus.",
                optionGet: () => Config.ScrollDown,
                optionSet: value => Config.ScrollDown = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Previous Tab",
                optionDesc: "Key to switch to previous tab.",
                optionGet: () => Config.PreviousTab,
                optionSet: value => Config.PreviousTab = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Next Tab",
                optionDesc: "Key to switch to next tab.",
                optionGet: () => Config.NextTab,
                optionSet: value => Config.NextTab = value
            );
            _modConfigMenu.API.RegisterSimpleOption(
                mod: ModManifest,
                optionName: "Capacity",
                optionDesc: "How many items each chest will hold (use -1 for maximum capacity).",
                optionGet: () => Config.Capacity,
                optionSet: value =>
                {
                    Config.Capacity = value;
                    if (value == 0)
                        Config.Global.Remove("Capacity");
                    else
                        Config.Global["Capacity"] = true;
                });
            _modConfigMenu.API.RegisterClampedOption(
                mod: ModManifest,
                optionName: "Menu Rows",
                optionDesc: "The most number of rows that the menu can expand into.",
                optionGet: () => Config.MenuRows,
                optionSet: value => Config.MenuRows = value,
                min: 3,
                max: 6,
                interval: 1
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Crafting Range",
                optionDesc: "The default range that chests can be remotely crafted from.",
                optionGet: () => Config.CraftingRange,
                optionSet: value => Config.CraftingRange = value,
                choices: new[] { "Inventory", "Location", "World" }
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Stashing Range",
                optionDesc: "The default range that chests can be remotely stashed into.",
                optionGet: () => Config.StashingRange,
                optionSet: value => Config.StashingRange = value,
                choices: new[] { "Inventory", "Location", "World" }
            );
            
            _modConfigMenu.API.RegisterLabel(ModManifest, "Global Overrides", "Enable/disable features for all chests");
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Access Carried",
                optionDesc: "Open the currently held chest in your inventory.",
                optionGet: () => GetConfig("AccessCarried"),
                optionSet: value => SetConfig("AccessCarried", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Craft from Chest",
                optionDesc: "Allows chest to be crafted from remotely.",
                optionGet: () => GetConfig("CraftFromChest"),
                optionSet: value => SetConfig("CraftFromChest", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Expanded Menu",
                optionDesc: "Expands or shrinks the chest menu.",
                optionGet: () => GetConfig("ExpandedMenu"),
                optionSet: value => SetConfig("ExpandedMenu", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Inventory Tabs",
                optionDesc: "Adds tabs to the chest menu.",
                optionGet: () => GetConfig("InventoryTabs"),
                optionSet: value => SetConfig("InventoryTabs", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Search Items",
                optionDesc: "Adds a search bar to the chest menu.",
                optionGet: () => GetConfig("SearchItems"),
                optionSet: value => SetConfig("SearchItems", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Stash to Chest",
                optionDesc: "Allows chest to be stashed into remotely.",
                optionGet: () => GetConfig("StashToChest"),
                optionSet: value => SetConfig("StashToChest", value),
                choices: configChoices
            );
            _modConfigMenu.API.RegisterChoiceOption(
                mod: ModManifest,
                optionName: "Vacuum Items",
                optionDesc: "Allows chests in player inventory to pick up dropped items.",
                optionGet: () => GetConfig("VacuumItems"),
                optionSet: value => SetConfig("VacuumItems", value),
                choices: configChoices
            );
        }
    }
}