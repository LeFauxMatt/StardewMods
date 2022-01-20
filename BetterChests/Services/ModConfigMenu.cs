namespace BetterChests.Services;

using System;
using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using BetterChests.Models;
using Common.Integrations.GenericModConfigMenu;
using FuryCore.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;

/// <inheritdoc />
internal class ModConfigMenu : IService
{
    private bool _isRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModConfigMenu"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="manifest"></param>
    public ModConfigMenu(ModConfig config, IModHelper helper, IManifest manifest)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private ModConfig Config { get; set; }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateModConfigMenu();
    }

    private void GenerateModConfigMenu()
    {
        if (this._isRegistered)
        {
            this.GMCM.API.Unregister(this.Manifest);
        }

        this._isRegistered = true;

        // Register mod configuration
        this.GMCM.API.Register(this.Manifest, this.Reset, this.Save);

        // Mod Config
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_General_Name);
        this.GenerateModConfigOptions();

        // Controls
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name);
        this.GenerateControlOptions();

        // Global Chest Config
        this.GMCM.API.AddPageLink(this.Manifest, "Default Options", I18n.Section_DefaultOptions_Name);

        // Chest Configs
        var chestTypes = this.Config.ChestConfigs.Keys.OrderBy(name => name).Distinct().ToList();
        chestTypes.Remove(string.Empty);
        foreach (var chestType in chestTypes)
        {
            this.GMCM.API.AddPageLink(this.Manifest, chestType, () => chestType);
        }

        this.GMCM.API.AddPage(this.Manifest, "Default Options");
        this.GenerateChestConfigOptions(string.Empty);

        foreach (var chestType in chestTypes)
        {
            this.GMCM.API.AddPage(this.Manifest, chestType);
            this.GenerateChestConfigOptions(chestType);
        }
    }

    private void GenerateModConfigOptions()
    {
        // Categorize Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CategorizedChests,
            value => this.Config.CategorizedChests = value,
            I18n.Config_CategorizeChest_Name,
            I18n.Config_CategorizeChest_Tooltip);

        // Chest Tabs
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ChestTabs,
            value => this.Config.ChestTabs = value,
            I18n.Config_ChestTabs_Name,
            I18n.Config_ChestTabs_Tooltip);

        // Color Picker
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ColorPicker,
            value => this.Config.ColorPicker = value,
            I18n.Config_ColorPicker_Name,
            I18n.Config_ColorPicker_Tooltip);

        // Fill Stacks
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.FillStacks,
            value => this.Config.FillStacks = value,
            I18n.Config_FillStacks_Name,
            I18n.Config_FillStacks_Tooltip);

        // Menu Rows
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.MenuRows,
            value => this.Config.MenuRows = value,
            I18n.Config_MenuRows_Name,
            I18n.Config_MenuRows_Tooltip,
            3,
            6,
            1);

        // Search Items
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.SearchItems,
            value => this.Config.SearchItems = value,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip);

        // Search Tag Symbol
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.SearchTagSymbol.ToString(),
            value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.Trim().ToCharArray()[0],
            I18n.Config_SearchTagSymbol_Name,
            I18n.Config_SearchTagSymbol_Tooltip);
    }

    private void GenerateControlOptions()
    {
        // Open Crafting
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.OpenCrafting,
            value => this.Config.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip);

        // Stash Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.StashItems,
            value => this.Config.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip);

        // Scroll Up
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.ScrollUp,
            value => this.Config.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip);

        // Scroll Down
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.ScrollDown,
            value => this.Config.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip);

        // Previous Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.PreviousTab,
            value => this.Config.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip);

        // Next Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.NextTab,
            value => this.Config.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip);
    }

    private void GenerateChestConfigOptions(string name)
    {
        if (!this.Config.ChestConfigs.TryGetValue(name, out var chestConfig))
        {
            chestConfig = new();
            this.Config.ChestConfigs.Add(name, chestConfig);
        }

        this.GenerateChestConfigOptions(chestConfig);
    }

    internal void GenerateChestConfigOptions(IChestConfig chestConfig, IManifest manifest = null, string[] features = null)
    {
        manifest ??= this.Manifest;
        features ??= new[] { "capacity", "access-carried", "carry-chest", "collect-items", "crafting-range", "stashing-range" };

        var optionValues = new[]
        {
            "Default",
            "Disabled",
            "Enabled",
        };

        var rangeValues = new[]
        {
            "Default",
            "Disabled",
            "Inventory",
            "Location",
            "World",
        };

        // Capacity
        if (features.Contains("capacity"))
        {
            this.GMCM.API.AddNumberOption(
                manifest,
                () => chestConfig.Capacity,
                value => chestConfig.Capacity = value,
                I18n.Config_Capacity_Name,
                I18n.Config_Capacity_Tooltip);
        }

        // Collect Items
        if (features.Contains("collect-items"))
        {
            this.GMCM.API.AddTextOption(
                manifest,
                () => ModConfigMenu.GetOptionName(chestConfig.CollectItems),
                value => chestConfig.CollectItems = ModConfigMenu.GetOptionValue(value),
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                optionValues,
                ModConfigMenu.FormatAllowedValue);
        }

        // Crafting Range
        if (features.Contains("crafting-range"))
        {
            this.GMCM.API.AddTextOption(
                manifest,
                () => ModConfigMenu.GetRangeName(chestConfig.CraftingRange),
                value => chestConfig.CraftingRange = ModConfigMenu.GetRangeValue(value),
                I18n.Config_CraftingRange_Name,
                I18n.Config_CraftingRange_Tooltip,
                rangeValues,
                ModConfigMenu.FormatAllowedValue);
        }

        // Stashing Range
        if (features.Contains("stashing-range"))
        {
            this.GMCM.API.AddTextOption(
                manifest,
                () => ModConfigMenu.GetRangeName(chestConfig.StashingRange),
                value => chestConfig.StashingRange = ModConfigMenu.GetRangeValue(value),
                I18n.Config_StashingRange_Name,
                I18n.Config_StashingRange_Tooltip,
                rangeValues,
                ModConfigMenu.FormatAllowedValue);
        }
    }

    private static string FormatAllowedValue(string value)
    {
        return value switch
        {
            "Default" => I18n.Option_Default_Name(),
            "Disabled" => I18n.Option_Disabled_Name(),
            "Enabled" => I18n.Option_Enabled_Name(),
            "Inventory" => I18n.Option_Inventory_Name(),
            "Location" => I18n.Option_Location_Name(),
            "World" => I18n.Option_World_Name(),
            _ => I18n.Option_Default_Name(),
        };
    }

    private static string GetOptionName(FeatureOption option)
    {
        return option switch
        {
            FeatureOption.Default => I18n.Option_Default_Name(),
            FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            FeatureOption.Enabled => I18n.Option_Enabled_Name(),
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
        };
    }

    private static FeatureOption GetOptionValue(string value)
    {
        return Enum.TryParse(value, out FeatureOption option) ? option: FeatureOption.Default;
    }

    private static string GetRangeName(FeatureOptionRange option)
    {
        return option switch
        {
            FeatureOptionRange.Default => I18n.Option_Default_Name(),
            FeatureOptionRange.Disabled => I18n.Option_Disabled_Name(),
            FeatureOptionRange.Inventory => I18n.Option_Inventory_Name(),
            FeatureOptionRange.Location => I18n.Option_Location_Name(),
            FeatureOptionRange.World => I18n.Option_World_Name(),
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
        };
    }

    private static FeatureOptionRange GetRangeValue(string value)
    {
        return Enum.TryParse(value, out FeatureOptionRange option) ? option: FeatureOptionRange.Default;
    }

    private void Reset()
    {
        this.Config = new();
    }

    private void Save()
    {
        this.Helper.WriteConfig(this.Config);
    }
}