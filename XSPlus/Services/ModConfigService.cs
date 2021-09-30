namespace XSPlus.Services
{
    using System;
    using Common.Helpers;
    using Common.Integrations.GenericModConfigMenu;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    /// <summary>
    /// Service to handle read/write to <see cref="Models.ModConfig"/>.
    /// </summary>
    internal class ModConfigService : BaseService
    {
        private static ModConfigService Instance;
        private readonly string[] _configChoices = { "Default", "Enable", "Disable" };
        private readonly string[] _rangeChoices = { "Inventory", "Location", "World", "Default", "Disabled" };
        private readonly IModHelper _helper;
        private readonly GenericModConfigMenuIntegration _modConfigMenu;
        private readonly IManifest _manifest;

        private ModConfigService(IModHelper helper, IManifest manifest)
            : base("ModConfig")
        {
            this._helper = helper;
            this._manifest = manifest;
            this._modConfigMenu = new GenericModConfigMenuIntegration(this._helper.ModRegistry);

            this.ModConfig = this._helper.ReadConfig<ModConfig>();

            Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ModConfigService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// <param name="manifest">A manifest which describes a mod for SMAPI.</param>
        /// <returns>Returns an instance of the <see cref="ModConfigService"/> class.</returns>
        public static ModConfigService GetSingleton(ServiceManager serviceManager, IModHelper helper, IManifest manifest)
        {
            return ModConfigService.Instance ??= new ModConfigService(helper, manifest);
        }

        /// <summary>
        /// Gets config containing default values and config options for features.
        /// </summary>
        public ModConfig ModConfig { get; private set; }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!this._modConfigMenu.IsLoaded)
            {
                return;
            }

            // Register mod configuration
            this._modConfigMenu.API.RegisterModConfig(this._manifest, this.RevertToDefault, this.SaveToFile);

            // Allow config in game
            this._modConfigMenu.API.SetDefaultIngameOptinValue(this._manifest, true);

            // Config options
            this._modConfigMenu.API.RegisterLabel(this._manifest, "General", string.Empty);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Open Crafting Button",
                optionDesc: "Key to open the crafting menu for accessible chests.",
                optionGet: () => this.ModConfig.OpenCrafting,
                optionSet: value => this.ModConfig.OpenCrafting = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Stash Items Button",
                optionDesc: "Key to stash items into accessible chests.",
                optionGet: () => this.ModConfig.StashItems,
                optionSet: value => this.ModConfig.StashItems = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Scroll Up",
                optionDesc: "Key to scroll up in expanded inventory menus.",
                optionGet: () => this.ModConfig.ScrollUp,
                optionSet: value => this.ModConfig.ScrollUp = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Scroll Down",
                optionDesc: "Key to scroll down in expanded inventory menus.",
                optionGet: () => this.ModConfig.ScrollDown,
                optionSet: value => this.ModConfig.ScrollDown = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Previous Tab",
                optionDesc: "Key to switch to previous tab.",
                optionGet: () => this.ModConfig.PreviousTab,
                optionSet: value => this.ModConfig.PreviousTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Next Tab",
                optionDesc: "Key to switch to next tab.",
                optionGet: () => this.ModConfig.NextTab,
                optionSet: value => this.ModConfig.NextTab = value);
            this._modConfigMenu.API.RegisterSimpleOption(
                mod: this._manifest,
                optionName: "Capacity",
                optionDesc: "How many items each chest will hold (use -1 for maximum capacity).",
                optionGet: () => this.ModConfig.Capacity,
                optionSet: this.SetCapacity);
            this._modConfigMenu.API.RegisterClampedOption(
                mod: this._manifest,
                optionName: "Menu Rows",
                optionDesc: "The most number of rows that the menu can expand into.",
                optionGet: () => this.ModConfig.MenuRows,
                optionSet: this.SetMenuRows,
                min: 3,
                max: 6,
                interval: 1);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Crafting Range",
                optionDesc: "The default range that chests can be remotely crafted from.",
                optionGet: () => this.ModConfig.CraftingRange,
                optionSet: this.SetCraftingRange,
                choices: this._rangeChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Stashing Range",
                optionDesc: "The default range that chests can be remotely stashed into.",
                optionGet: () => this.ModConfig.StashingRange,
                optionSet: this.SetStashingRange,
                choices: this._rangeChoices);

            this._modConfigMenu.API.RegisterLabel(this._manifest, "Global Overrides", "Enable/disable features for all chests");
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Access Carried",
                optionDesc: "Open the currently held chest in your inventory.",
                optionGet: this.GetConfig("AccessCarried"),
                optionSet: this.SetConfig("AccessCarried"),
                choices: this._configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Categorized Chest",
                optionDesc: "Organize chests by assigning categories of items.",
                optionGet: this.GetConfig("CategorizedChest"),
                optionSet: this.SetConfig("CategorizedChest"),
                choices: this._configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Color Picker",
                optionDesc: "Adds an HSL Color Picker to the chest menu.",
                optionGet: this.GetConfig("ColorPicker"),
                optionSet: this.SetConfig("ColorPicker"),
                choices: this._configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Inventory Tabs",
                optionDesc: "Adds tabs to the chest menu.",
                optionGet: this.GetConfig("InventoryTabs"),
                optionSet: this.SetConfig("InventoryTabs"),
                choices: this._configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Search Items",
                optionDesc: "Adds a search bar to the chest menu.",
                optionGet: this.GetConfig("SearchItems"),
                optionSet: this.SetConfig("SearchItems"),
                choices: this._configChoices);
            this._modConfigMenu.API.RegisterChoiceOption(
                mod: this._manifest,
                optionName: "Vacuum Items",
                optionDesc: "Allows chests in player inventory to pick up dropped items.",
                optionGet: this.GetConfig("VacuumItems"),
                optionSet: this.SetConfig("VacuumItems"),
                choices: this._configChoices);
        }

        private void RevertToDefault()
        {
            this.ModConfig = new ModConfig();
        }

        private void SaveToFile()
        {
            this._helper.WriteConfig(this.ModConfig);
        }


        private Func<string> GetConfig(string featureName)
        {
            return () => this.ModConfig.Global.TryGetValue(featureName, out bool global)
                ? (global ? "Enable" : "Disable")
                : "Default";
        }

        private Action<string> SetConfig(string featureName)
        {
            return value =>
            {
                switch (value)
                {
                    case "Enable":
                        this.ModConfig.Global[featureName] = true;
                        FeatureManager.ActivateFeature(featureName);
                        break;
                    case "Disable":
                        this.ModConfig.Global[featureName] = false;
                        FeatureManager.DeactivateFeature(featureName);
                        break;
                    default:
                        this.ModConfig.Global.Remove(featureName);
                        FeatureManager.ActivateFeature(featureName);
                        break;
                }
            };
        }

        private void SetCapacity(int value)
        {
            this.ModConfig.Capacity = value;
            if (value == 0)
            {
                this.ModConfig.Global.Remove("Capacity");
            }
            else
            {
                this.ModConfig.Global["Capacity"] = true;
            }
        }

        private void SetMenuRows(int value)
        {
            this.ModConfig.MenuRows = value;
            if (value <= 3)
            {
                this.ModConfig.Global.Remove("ExpandedMenu");
            }
            else
            {
                this.ModConfig.Global["ExpandedMenu"] = true;
            }
        }

        private void SetCraftingRange(string value)
        {
            switch (value)
            {
                case "Default":
                    this.ModConfig.CraftingRange = "Location";
                    this.ModConfig.Global.Remove("CraftFromChest");
                    break;
                case "Disabled":
                    this.ModConfig.Global["CraftFromChest"] = false;
                    break;
                default:
                    this.ModConfig.CraftingRange = value;
                    this.ModConfig.Global["CraftFromChest"] = true;
                    break;
            }
        }

        private void SetStashingRange(string value)
        {
            switch (value)
            {
                case "Default":
                    this.ModConfig.StashingRange = "Location";
                    this.ModConfig.Global.Remove("StashToChest");
                    break;
                case "Disabled":
                    this.ModConfig.Global["StashToChest"] = false;
                    break;
                default:
                    this.ModConfig.StashingRange = value;
                    this.ModConfig.Global["StashToChest"] = true;
                    break;
            }
        }
    }
}