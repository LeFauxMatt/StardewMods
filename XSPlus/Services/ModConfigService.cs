namespace XSPlus.Services;

using System;
using Common.Integrations.GenericModConfigMenu;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

/// <summary>
///     Service to handle read/write to <see cref="Models.ModConfig" />.
/// </summary>
internal class ModConfigService : BaseService
{
    private readonly Action<string> _activateFeature;
    private readonly Action<string> _deactivateFeature;
    private readonly IManifest _manifest;
    private readonly GenericModConfigMenuIntegration _modConfigMenu;
    private readonly ITranslationHelper _translation;
    private readonly Action<ModConfig> _writeConfig;

    private ModConfigService(ServiceLocator serviceLocator)
        : base("ModConfig")
    {
        // Init
        this.ModConfig = serviceLocator.Helper.ReadConfig<ModConfig>();
        this._activateFeature = serviceLocator.ActivateFeature;
        this._deactivateFeature = serviceLocator.DeactivateFeature;
        this._manifest = serviceLocator.ModManifest;
        this._modConfigMenu = new(serviceLocator.Helper.ModRegistry);
        this._translation = serviceLocator.Helper.Translation;
        this._writeConfig = serviceLocator.Helper.WriteConfig;

        // Events
        serviceLocator.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <summary>
    ///     Gets config containing default values and config options for features.
    /// </summary>
    public ModConfig ModConfig { get; private set; }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this._modConfigMenu.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        this._modConfigMenu.API.Register(this._manifest, this.Reset, this.Save);

        // Config options
        this._modConfigMenu.API.AddSectionTitle(this._manifest, () => this._translation.Get("section.general.name"));
        this._modConfigMenu.API.AddKeybindList(this._manifest,
            name: () => this._translation.Get("config.open-crafting.name"),
            tooltip: () => this._translation.Get("config.open-crafting.tooltip"),
            getValue: () => this.ModConfig.OpenCrafting,
            setValue: value => this.ModConfig.OpenCrafting = value);

        this._modConfigMenu.API.AddKeybindList(
            this._manifest,
            name: () => this._translation.Get("config.stash-items.name"),
            tooltip: () => this._translation.Get("config.stash-items.tooltip"),
            getValue: () => this.ModConfig.StashItems,
            setValue: value => this.ModConfig.StashItems = value);

        this._modConfigMenu.API.AddKeybindList(
            this._manifest,
            name: () => this._translation.Get("config.scroll-up.name"),
            tooltip: () => this._translation.Get("config.scroll-up.tooltip"),
            getValue: () => this.ModConfig.ScrollUp,
            setValue: value => this.ModConfig.ScrollUp = value);

        this._modConfigMenu.API.AddKeybindList(
            this._manifest,
            name: () => this._translation.Get("config.scroll-down.name"),
            tooltip: () => this._translation.Get("config.scroll-down.tooltip"),
            getValue: () => this.ModConfig.ScrollDown,
            setValue: value => this.ModConfig.ScrollDown = value);

        this._modConfigMenu.API.AddKeybindList(
            this._manifest,
            name: () => this._translation.Get("config.previous-tab.name"),
            tooltip: () => this._translation.Get("config.previous-tab.tooltip"),
            getValue: () => this.ModConfig.PreviousTab,
            setValue: value => this.ModConfig.PreviousTab = value);

        this._modConfigMenu.API.AddKeybindList(this._manifest,
            name: () => this._translation.Get("config.next-tab.name"),
            tooltip: () => this._translation.Get("config.previous-tab.tooltip"),
            getValue: () => this.ModConfig.NextTab,
            setValue: value => this.ModConfig.NextTab = value);

        this._modConfigMenu.API.AddNumberOption(
            this._manifest,
            name: () => this._translation.Get("config.capacity.name"),
            tooltip: () => this._translation.Get("config.capacity.tooltip"),
            getValue: () => this.ModConfig.Capacity,
            setValue: this.SetCapacity);

        this._modConfigMenu.API.AddNumberOption(
            this._manifest,
            name: () => this._translation.Get("config.menu-rows.name"),
            tooltip: () => this._translation.Get("config.menu-rows.tooltip"),
            getValue: () => this.ModConfig.MenuRows,
            setValue: this.SetMenuRows,
            min: 3,
            max: 6,
            interval: 1);

        var rangeValues = new[]
        {
            "Inventory", "Location", "World", "Default", "Disabled",
        };

        string FormatRangeValues(string value)
        {
            return value switch
            {
                "Inventory" => this._translation.Get("choice.inventory.name"),
                "Location" => this._translation.Get("choice.location.name"),
                "World" => this._translation.Get("choice.world.name"),
                "Default" => this._translation.Get("choice.default.name"),
                "Disabled" => this._translation.Get("choice.disabled.name"),
                _ => value,
            };
        }

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.crafting-range.name"),
            tooltip: () => this._translation.Get("config.crafting-range.tooltip"),
            getValue: () => this.ModConfig.CraftingRange,
            setValue: this.SetCraftingRange,
            allowedValues: rangeValues,
            formatAllowedValue: FormatRangeValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.stashing-range.name"),
            tooltip: () => this._translation.Get("config.stashing-range.tooltip"),
            getValue: () => this.ModConfig.StashingRange,
            setValue: this.SetStashingRange,
            allowedValues: rangeValues,
            formatAllowedValue: FormatRangeValues);

        var configValues = new[]
        {
            "Default", "Enable", "Disable",
        };

        string FormatConfigValues(string value)
        {
            return this._translation.Get($"choice.{value}.name");
        };

        this._modConfigMenu.API.AddSectionTitle(
            this._manifest, 
            () => this._translation.Get("section.global-overrides.name"), 
            () => this._translation.Get("section.global-overrides.tooltip"));

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.access-carried.name"),
            tooltip: () => this._translation.Get("config.access-carried.tooltip"),
            getValue: this.GetConfig("AccessCarried"),
            setValue: this.SetConfig("AccessCarried"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.carry-chest.name"),
            tooltip: () => this._translation.Get("config.carry-chest.tooltip"),
            getValue: this.GetConfig("CarryChest"),
            setValue: this.SetConfig("CarryChest"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.categorize-chest.name"),
            tooltip: () => this._translation.Get("config.categorize-chest.tooltip"),
            getValue: this.GetConfig("CategorizeChest"),
            setValue: this.SetConfig("CategorizeChest"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.color-picker.name"),
            tooltip: () => this._translation.Get("config.color-picker.tooltip"),
            getValue: this.GetConfig("ColorPicker"),
            setValue: this.SetConfig("ColorPicker"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.inventory-tabs.name"),
            tooltip: () => this._translation.Get("config.inventory-tabs.tooltip"),
            getValue: this.GetConfig("InventoryTabs"),
            setValue: this.SetConfig("InventoryTabs"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.search-items.name"),
            tooltip: () => this._translation.Get("config.search-items.tooltip"),
            getValue: this.GetConfig("SearchItems"),
            setValue: this.SetConfig("SearchItems"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);

        this._modConfigMenu.API.AddTextOption(
            this._manifest,
            name: () => this._translation.Get("config.vacuum-items.name"),
            tooltip: () => this._translation.Get("config.vacuum-items.tooltip"),
            getValue: this.GetConfig("VacuumItems"),
            setValue: this.SetConfig("VacuumItems"),
            allowedValues: configValues,
            formatAllowedValue: FormatConfigValues);
    }

    private void Reset()
    {
        this.ModConfig = new();
    }

    private void Save()
    {
        this._writeConfig(this.ModConfig);
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
                    this._activateFeature(featureName);
                    break;
                case "Disable":
                    this.ModConfig.Global[featureName] = false;
                    this._deactivateFeature(featureName);
                    break;
                default:
                    this.ModConfig.Global.Remove(featureName);
                    this._activateFeature(featureName);
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