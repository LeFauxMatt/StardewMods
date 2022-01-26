namespace BetterChests.Services;

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
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    public ModConfigMenu(IConfigModel config, IModHelper helper, IManifest manifest)
    {
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Config = config;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private IConfigModel Config { get; }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateConfig();
    }

    private void GenerateConfig()
    {
        if (this._isRegistered)
        {
            this.GMCM.API.Unregister(this.Manifest);
        }

        this._isRegistered = true;

        // Register mod configuration
        this.GMCM.API.Register(this.Manifest, this.Config.Reset, this.Config.Save);

        // Pages
        this.GMCM.API.AddPageLink(this.Manifest, "General", I18n.Section_General_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_General_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Features", I18n.Section_Features_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Features_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Controls", I18n.Section_Controls_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Controls_Description);

        // General
        this.GMCM.API.AddPage(this.Manifest, "General");
        this.GeneralConfig();

        // Features
        this.GMCM.API.AddPage(this.Manifest, "Features");
        this.FeatureConfig();

        // Controller
        this.GMCM.API.AddPage(this.Manifest, "Controls");
        this.ControllerConfig();
    }

    private void GeneralConfig()
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Config_CraftFromChest_Name, I18n.Config_CraftFromChest_Tooltip);

        // Craft from Chest Distance
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.CraftFromChestDistance,
            value => this.Config.CraftFromChestDistance = value,
            I18n.Config_CraftFromChestDistance_Name,
            I18n.Config_CraftFromChestDistance_Tooltip);

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Config_CustomColorPicker_Name, I18n.Config_CustomColorPicker_Tooltip);

        // Custom Color Picker Area
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.CustomColorPickerAreaString,
            value => this.Config.CustomColorPickerAreaString = value,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            ConfigModel.AreaValues,
            ConfigModel.FormatAreaValue);

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Config_SearchItems_Name, I18n.Config_SearchItems_Tooltip);

        // Search Tag Symbol
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.SearchTagSymbolString,
            value => this.Config.SearchTagSymbolString = value,
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip);

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Config_StashToChest_Name, I18n.Config_StashToChest_Tooltip);

        // Stash to Chest Distance
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.StashToChestDistance,
            value => this.Config.StashToChestDistance = value,
            I18n.Config_StashToChestDistance_Name,
            I18n.Config_StashToChestDistance_Tooltip);

        // Fill Stacks
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.FillStacks,
            value => this.Config.FillStacks = value,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip);
    }

    private void FeatureConfig()
    {
        // Carry Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CarryChest != FeatureOption.Disabled,
            value => this.Config.CarryChest = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip);

        // Categorize Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CategorizeChest != FeatureOption.Disabled,
            value => this.Config.CategorizeChest = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_CategorizeChest_Name,
            I18n.Config_CategorizeChest_Tooltip);

        // Chest Menu Tabs
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ChestMenuTabs != FeatureOption.Disabled,
            value => this.Config.ChestMenuTabs = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_ChestMenuTabs_Name,
            I18n.Config_ChestMenuTabs_Tooltip);

        // Collect Items
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CollectItems != FeatureOption.Disabled,
            value => this.Config.CollectItems = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip);

        // Craft from Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.CraftFromChestString,
            value => this.Config.CraftFromChestString = value,
            I18n.Config_CraftFromChest_Name,
            I18n.Config_CraftFromChest_Tooltip,
            ConfigModel.RangeValues,
            ConfigModel.FormatRangeValue);

        // Custom Color Picker
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CustomColorPicker != FeatureOption.Disabled,
            value => this.Config.CustomColorPicker = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_CustomColorPicker_Name,
            I18n.Config_CustomColorPicker_Tooltip);

        // Open Held Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.OpenHeldChest != FeatureOption.Disabled,
            value => this.Config.OpenHeldChest = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip);

        // Resize Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ResizeChest != FeatureOption.Disabled,
            value => this.Config.ResizeChest = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip);

        // Resize Chest Menu
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ResizeChestMenu != FeatureOption.Disabled,
            value => this.Config.ResizeChestMenu = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip);

        // Resize Chest
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.ResizeChestCapacity,
            value => this.Config.ResizeChestCapacity = value,
            I18n.Config_ResizeChest_Name,
            I18n.Config_ResizeChest_Tooltip);

        // Resize Chest Menu
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.ResizeChestMenuRows,
            value => this.Config.ResizeChestMenuRows = value,
            I18n.Config_ResizeChestMenu_Name,
            I18n.Config_ResizeChestMenu_Tooltip,
            0,
            6,
            1);

        // Search Items
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.SearchItems != FeatureOption.Disabled,
            value => this.Config.SearchItems = value ? FeatureOption.Enabled : FeatureOption.Disabled,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip);

        // Stash to Chest Range
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.StashToChestString,
            value => this.Config.StashToChestString = value,
            I18n.Config_StashToChest_Name,
            I18n.Config_StashToChest_Tooltip,
            ConfigModel.RangeValues,
            ConfigModel.FormatRangeValue);
    }

    private void ControllerConfig()
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
}