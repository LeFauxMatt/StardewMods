namespace XSPlus.Services
{
    using System;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Integrations.GenericModConfigMenu;
    using CommonHarmony.Services;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    /// <summary>
    ///     Service to handle read/write to <see cref="Models.ModConfig" />.
    /// </summary>
    internal class ModConfigService : BaseService
    {
        private static ModConfigService Instance;
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        private readonly GenericModConfigMenuIntegration _modConfigMenu;
        private readonly ServiceManager _serviceManager;

        private ModConfigService(ServiceManager serviceManager)
            : base("ModConfig")
        {
            this._serviceManager = serviceManager;
            this._helper = serviceManager.Helper;
            this._manifest = serviceManager.ModManifest;
            this._modConfigMenu = new(this._helper.ModRegistry);

            this.ModConfig = this._helper.ReadConfig<ModConfig>();

            Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>
        ///     Gets config containing default values and config options for features.
        /// </summary>
        public ModConfig ModConfig { get; private set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="ModConfigService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ModConfigService" /> class.</returns>
        public static async Task<ModConfigService> Create(ServiceManager serviceManager)
        {
            return ModConfigService.Instance ??= new(serviceManager);
        }

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
                this._manifest,
                "Open Crafting Button",
                "Key to open the crafting menu for accessible chests.",
                () => this.ModConfig.OpenCrafting,
                value => this.ModConfig.OpenCrafting = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Stash Items Button",
                "Key to stash items into accessible chests.",
                () => this.ModConfig.StashItems,
                value => this.ModConfig.StashItems = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Scroll Up",
                "Key to scroll up in expanded inventory menus.",
                () => this.ModConfig.ScrollUp,
                value => this.ModConfig.ScrollUp = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Scroll Down",
                "Key to scroll down in expanded inventory menus.",
                () => this.ModConfig.ScrollDown,
                value => this.ModConfig.ScrollDown = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Previous Tab",
                "Key to switch to previous tab.",
                () => this.ModConfig.PreviousTab,
                value => this.ModConfig.PreviousTab = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Next Tab",
                "Key to switch to next tab.",
                () => this.ModConfig.NextTab,
                value => this.ModConfig.NextTab = value);

            this._modConfigMenu.API.RegisterSimpleOption(
                this._manifest,
                "Capacity",
                "How many items each chest will hold (use -1 for maximum capacity).",
                () => this.ModConfig.Capacity,
                this.SetCapacity);

            this._modConfigMenu.API.RegisterClampedOption(
                this._manifest,
                "Menu Rows",
                "The most number of rows that the menu can expand into.",
                () => this.ModConfig.MenuRows,
                this.SetMenuRows,
                3,
                6,
                1);

            var rangeChoices = new[] { "Inventory", "Location", "World", "Default", "Disabled" };
            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Crafting Range",
                "The default range that chests can be remotely crafted from.",
                () => this.ModConfig.CraftingRange,
                this.SetCraftingRange,
                rangeChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Stashing Range",
                "The default range that chests can be remotely stashed into.",
                () => this.ModConfig.StashingRange,
                this.SetStashingRange,
                rangeChoices);

            var configChoices = new[] { "Default", "Enable", "Disable" };
            this._modConfigMenu.API.RegisterLabel(this._manifest, "Global Overrides", "Enable/disable features for all chests");
            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Access Carried",
                "Open the currently held chest in your inventory.",
                this.GetConfig("AccessCarried"),
                this.SetConfig("AccessCarried"),
                configChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Categorized Chest",
                "Organize chests by assigning categories of items.",
                this.GetConfig("CategorizeChest"),
                this.SetConfig("CategorizeChest"),
                configChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Color Picker",
                "Adds an HSL Color Picker to the chest menu.",
                this.GetConfig("ColorPicker"),
                this.SetConfig("ColorPicker"),
                configChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Inventory Tabs",
                "Adds tabs to the chest menu.",
                this.GetConfig("InventoryTabs"),
                this.SetConfig("InventoryTabs"),
                configChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Search Items",
                "Adds a search bar to the chest menu.",
                this.GetConfig("SearchItems"),
                this.SetConfig("SearchItems"),
                configChoices);

            this._modConfigMenu.API.RegisterChoiceOption(
                this._manifest,
                "Vacuum Items",
                "Allows chests in player inventory to pick up dropped items.",
                this.GetConfig("VacuumItems"),
                this.SetConfig("VacuumItems"),
                configChoices);
        }

        private void RevertToDefault()
        {
            this.ModConfig = new();
        }

        private void SaveToFile()
        {
            this._helper.WriteConfig(this.ModConfig);
        }

        private Func<string> GetConfig(string featureName)
        {
            return () => this.ModConfig.Global.TryGetValue(featureName, out var global)
                ? global ? "Enable" : "Disable"
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
                        this._serviceManager.ActivateFeature(featureName);
                        break;
                    case "Disable":
                        this.ModConfig.Global[featureName] = false;
                        this._serviceManager.DeactivateFeature(featureName);
                        break;
                    default:
                        this.ModConfig.Global.Remove(featureName);
                        this._serviceManager.ActivateFeature(featureName);
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