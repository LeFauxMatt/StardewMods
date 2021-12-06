namespace BetterChests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Services;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    internal class ModConfigService : BaseService
    {
        private readonly GenericModConfigMenuIntegration _gmcm;
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        private ManagedChestService _managedChestService;
        private bool _isRegistered;

        private ModConfigService(ServiceManager serviceManager)
            : base("ModConfig")
        {
            // Init
            this.AddDependency<ManagedChestService>(service => this._managedChestService = service as ManagedChestService);
            this._helper = serviceManager.Helper;
            this.ModConfig = this._helper.ReadConfig<ModConfig>();
            this._gmcm = new(this._helper.ModRegistry);
            this._manifest = serviceManager.ModManifest;

            // Events
            this._helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <summary>
        /// Gets the configuration for the mod and known chest types.
        /// </summary>
        public ModConfig ModConfig { get; private set; }

        /// <summary>
        /// Gets a <see cref="ChestConfig" /> for a chest by its name and creates one if it does not already exist.
        /// </summary>
        /// <param name="name">The name of the chest.</param>
        /// <returns>Returns a ChestConfig for the chest.</returns>
        public ChestConfig GetChestConfig(string name)
        {
            if (!this.ModConfig.ChestConfigs.TryGetValue(name, out var chestConfig))
            {
                chestConfig = new();
                this.ModConfig.ChestConfigs.Add(name, chestConfig);
            }

            return chestConfig;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!this._gmcm.IsLoaded)
            {
                return;
            }

            this.GenerateModConfigMenu();
        }

        private void GenerateModConfigMenu()
        {
            if (this._isRegistered)
            {
                this._gmcm.API.Unregister(this._manifest);
            }

            this._isRegistered = true;

            // Register mod configuration
            this._gmcm.API.Register(this._manifest, this.Reset, this.Save);

            // Mod Config
            this._gmcm.API.AddSectionTitle(this._manifest, () => this._helper.Translation.Get("section.general.name"));
            this.GenerateModConfigOptions();

            // Controls
            this._gmcm.API.AddSectionTitle(this._manifest, () => this._helper.Translation.Get("section.controls.name"));
            this.GenerateControlOptions();

            // Global Chest Config
            this._gmcm.API.AddPageLink(this._manifest, "Default Options", () => this._helper.Translation.Get("section.default-options.name"));

            // Chest Configs
            var chestTypes = this._managedChestService.GetManagedChestTypes().OrderBy(name => name).Distinct().ToList();

            foreach (var chestType in chestTypes)
            {
                this._gmcm.API.AddPageLink(this._manifest, chestType, () => chestType);
            }

            this._gmcm.API.AddPage(this._manifest, "Default Options");
            this.GenerateChestConfigOptions();

            foreach (var chestType in chestTypes)
            {
                this._gmcm.API.AddPage(this._manifest, chestType);
                this.GenerateChestConfigOptions(chestType);
            }
        }

        private void GenerateModConfigOptions()
        {
            // Categorize Chest
            this._gmcm.API.AddBoolOption(
                this._manifest,
                () => this.ModConfig.CategorizedChests,
                value => this.ModConfig.CategorizedChests = value,
                () => this._helper.Translation.Get("config.categorize-chest.name"),
                () => this._helper.Translation.Get("config.categorize-chest.tooltip"));

            // Chest Tabs
            this._gmcm.API.AddBoolOption(
                this._manifest,
                () => this.ModConfig.ChestTabs,
                value => this.ModConfig.ChestTabs = value,
                () => this._helper.Translation.Get("config.chest-tabs.name"),
                () => this._helper.Translation.Get("config.chest-tabs.tooltip"));

            // Color Picker
            this._gmcm.API.AddBoolOption(
                this._manifest,
                () => this.ModConfig.ColorPicker,
                value => this.ModConfig.ColorPicker = value,
                () => this._helper.Translation.Get("config.color-picker.name"),
                () => this._helper.Translation.Get("config.color-picker.tooltip"));

            // Menu Rows
            this._gmcm.API.AddNumberOption(
                this._manifest,
                () => this.ModConfig.MenuRows,
                value => this.ModConfig.MenuRows = value,
                () => this._helper.Translation.Get("config.menu-rows.name"),
                () => this._helper.Translation.Get("config.menu-rows.tooltip"),
                3,
                6,
                1);

            // Search Items
            this._gmcm.API.AddBoolOption(
                this._manifest,
                () => this.ModConfig.SearchItems,
                value => this.ModConfig.SearchItems = value,
                () => this._helper.Translation.Get("config.search-items.name"),
                () => this._helper.Translation.Get("config.search-items.tooltip"));

            // Search Tag Symbol
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.ModConfig.SearchTagSymbol.ToString(),
                value => this.ModConfig.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.Trim().ToCharArray()[0],
                () => this._helper.Translation.Get("config.search-tag-symbol.name"),
                () => this._helper.Translation.Get("config.search-tag-symbol.tooltip"));
        }

        private void GenerateControlOptions()
        {
            // Open Crafting
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.OpenCrafting,
                value => this.ModConfig.OpenCrafting = value,
                () => this._helper.Translation.Get("config.open-crafting.name"),
                () => this._helper.Translation.Get("config.open-crafting.tooltip"));

            // Stash Items
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.StashItems,
                value => this.ModConfig.StashItems = value,
                () => this._helper.Translation.Get("config.stash-items.name"),
                () => this._helper.Translation.Get("config.stash-items.tooltip"));

            // Scroll Up
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.ScrollUp,
                value => this.ModConfig.ScrollUp = value,
                () => this._helper.Translation.Get("config.scroll-up.name"),
                () => this._helper.Translation.Get("config.scroll-up.tooltip"));

            // Scroll Down
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.ScrollDown,
                value => this.ModConfig.ScrollDown = value,
                () => this._helper.Translation.Get("config.scroll-down.name"),
                () => this._helper.Translation.Get("config.scroll-down.tooltip"));

            // Previous Tab
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.PreviousTab,
                value => this.ModConfig.PreviousTab = value,
                () => this._helper.Translation.Get("config.previous-tab.name"),
                () => this._helper.Translation.Get("config.previous-tab.tooltip"));

            // Next Tab
            this._gmcm.API.AddKeybindList(
                this._manifest,
                () => this.ModConfig.NextTab,
                value => this.ModConfig.NextTab = value,
                () => this._helper.Translation.Get("config.next-tab.name"),
                () => this._helper.Translation.Get("config.next-tab.tooltip"));
        }

        private void GenerateChestConfigOptions(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                this.GenerateChestConfigOptions(this.ModConfig.DefaultConfig);
                return;
            }

            var chestConfig = this.GetChestConfig(name);
            this.GenerateChestConfigOptions(chestConfig);
        }

        private void GenerateChestConfigOptions(ChestConfig chestConfig)
        {
            string[] optionValues =
            {
                "Default",
                "Disabled",
                "Enabled",
            };

            string[] rangeValues =
            {
                "Default",
                "Disabled",
                "Inventory",
                "Location",
                "World",
            };

            // Capacity
            this._gmcm.API.AddNumberOption(
                this._manifest,
                () => chestConfig.Capacity,
                value => chestConfig.Capacity = value,
                () => this._helper.Translation.Get("config.capacity.name"),
                () => this._helper.Translation.Get("config.capacity.tooltip"));

            // Access Carried
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.GetOptionName(chestConfig.AccessCarried),
                value => chestConfig.AccessCarried = ModConfigService.GetOptionValue(value),
                () => this._helper.Translation.Get("config.access-carried.name"),
                () => this._helper.Translation.Get("config.access-carried.tooltip"),
                optionValues,
                value => this._helper.Translation.Get($"option.{value}.name"));

            // Carry Chest
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.GetOptionName(chestConfig.CarryChest),
                value => chestConfig.CarryChest = ModConfigService.GetOptionValue(value),
                () => this._helper.Translation.Get("config.carry-chest.name"),
                () => this._helper.Translation.Get("config.carry-chest.tooltip"),
                optionValues,
                value => this._helper.Translation.Get($"option.{value}.name"));

            // Collect Items
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.GetOptionName(chestConfig.CollectItems),
                value => chestConfig.CollectItems = ModConfigService.GetOptionValue(value),
                () => this._helper.Translation.Get("config.collect-items.name"),
                () => this._helper.Translation.Get("config.collect-items.tooltip"),
                optionValues,
                value => this._helper.Translation.Get($"option.{value}.name"));

            // Crafting Range
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.GetRangeName(chestConfig.CraftingRange),
                value => chestConfig.CraftingRange = ModConfigService.GetRangeValue(value),
                () => this._helper.Translation.Get("config.crafting-range.name"),
                () => this._helper.Translation.Get("config.crafting-range.tooltip"),
                rangeValues,
                value => this._helper.Translation.Get($"option.{value}.name"));

            // Stashing Range
            this._gmcm.API.AddTextOption(
                this._manifest,
                () => this.GetRangeName(chestConfig.StashingRange),
                value => chestConfig.StashingRange = ModConfigService.GetRangeValue(value),
                () => this._helper.Translation.Get("config.stashing-range.name"),
                () => this._helper.Translation.Get("config.stashing-range.tooltip"),
                rangeValues,
                value => this._helper.Translation.Get($"option.{value}.name"));
        }

        private string GetOptionName(FeatureOption option)
        {
            return option switch
            {
                FeatureOption.Default => this._helper.Translation.Get("option.default.name"),
                FeatureOption.Disabled => this._helper.Translation.Get("option.disabled.name"),
                FeatureOption.Enabled => this._helper.Translation.Get("option.enabled.name"),
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
            };
        }

        private static FeatureOption GetOptionValue(string value)
        {
            return Enum.TryParse(value, out FeatureOption option) ? option: FeatureOption.Default;
        }

        private string GetRangeName(FeatureOptionRange option)
        {
            return option switch
            {
                FeatureOptionRange.Default => this._helper.Translation.Get("option.default.name"),
                FeatureOptionRange.Disabled => this._helper.Translation.Get("option.disabled.name"),
                FeatureOptionRange.Inventory => this._helper.Translation.Get("option.inventory.name"),
                FeatureOptionRange.Location => this._helper.Translation.Get("option.location.name"),
                FeatureOptionRange.World => this._helper.Translation.Get("option.world.name"),
                _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
            };
        }

        private static FeatureOptionRange GetRangeValue(string value)
        {
            return Enum.TryParse(value, out FeatureOptionRange option) ? option: FeatureOptionRange.Default;
        }

        private void Reset()
        {
            this.ModConfig = new();
        }

        private void Save()
        {
            this._helper.WriteConfig(this.ModConfig);
        }
    }
}