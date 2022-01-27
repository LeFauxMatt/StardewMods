namespace BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using BetterChests.Features;
using BetterChests.Helpers;
using BetterChests.Interfaces;
using BetterChests.Models;
using Common.Integrations.GenericModConfigMenu;
using FuryCore.Enums;
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
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public ModConfigMenu(IConfigModel config, IModHelper helper, IManifest manifest, IServiceLocator services)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.Services = services;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private IServiceLocator Services { get; }

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

        // General
        this.GeneralConfig();

        // Pages
        this.GMCM.API.AddPageLink(this.Manifest, "Features", I18n.Section_Features_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Features_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Controls", I18n.Section_Controls_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Controls_Description);

        // Features
        this.GMCM.API.AddPage(this.Manifest, "Features");
        this.ChestConfig(this.Config.DefaultChest, true);

        // Controller
        this.GMCM.API.AddPage(this.Manifest, "Controls");
        this.ControlsConfig(this.Config.ControlScheme);
    }

    private void GeneralConfig()
    {
        var areaValues =
            new[] { ComponentArea.Right, ComponentArea.Left }
                .Select(FormatHelper.GetAreaString)
                .ToArray();

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Custom Color Picker Area
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetAreaString(this.Config.CustomColorPickerArea),
            value => this.Config.CustomColorPickerArea = Enum.TryParse(value, out ComponentArea area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            areaValues,
            FormatHelper.FormatArea,
            nameof(this.Config.CustomColorPickerArea));

        // Search Tag Symbol
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => this.Config.SearchTagSymbol.ToString(),
            value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip,
            fieldId: nameof(this.Config.SearchTagSymbol));

        // Slot Lock
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.SlotLock,
            value => this.Config.SlotLock = value,
            I18n.Config_SlotLock_Name,
            I18n.Config_SlotLock_Tooltip,
            nameof(SlotLock));
    }

    private void ChestConfig(IChestData config, bool defaultConfig = false)
    {
        var optionValues = (defaultConfig
                               ? new[] { FeatureOption.Disabled, FeatureOption.Enabled }
                               : new[] { FeatureOption.Disabled, FeatureOption.Default, FeatureOption.Enabled })
                           .Select(FormatHelper.GetOptionString)
                           .ToArray();
        var rangeValues = (defaultConfig
                              ? new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Inventory, FeatureOptionRange.Location, FeatureOptionRange.World }
                              : new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Default, FeatureOptionRange.Inventory, FeatureOptionRange.Location, FeatureOptionRange.World })
                          .Select(FormatHelper.GetRangeString)
                          .ToArray();
        var defaultOption = defaultConfig ? FeatureOption.Enabled : FeatureOption.Default;
        var defaultRange = defaultConfig ? FeatureOptionRange.Location : FeatureOptionRange.Default;

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Features_Name, I18n.Section_Features_Description);

        // Carry Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.CarryChest),
            value => config.CarryChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CarryChest));

        // Categorize Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.CategorizeChest),
            value => config.CategorizeChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CategorizeChest_Name,
            I18n.Config_CategorizeChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CategorizeChest));

        // Chest Menu Tabs
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.ChestMenuTabs),
            value => config.ChestMenuTabs = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_ChestMenuTabs_Name,
            I18n.Config_ChestMenuTabs_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(ChestMenuTabs));

        // Collect Items
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.CollectItems),
            value => config.CollectItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CollectItems));

        // Craft from Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetRangeString(config.CraftFromChest),
            value => config.CraftFromChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_CraftFromChest_Name,
            I18n.Config_CraftFromChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(CraftFromChest));

        // Craft from Chest Distance
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => config.CraftFromChestDistance switch
            {
                -1 => 6,
                _ => config.CraftFromChestDistance,
            },
            value => config.CraftFromChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_CraftFromChestDistance_Name,
            I18n.Config_CraftFromChestDistance_Tooltip,
            1,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IChestData.CraftFromChestDistance));

        // Custom Color Picker
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.CustomColorPicker),
            value => config.CustomColorPicker = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CustomColorPicker_Name,
            I18n.Config_CustomColorPicker_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CustomColorPicker));

        // Filter Items
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.FilterItems),
            value => config.FilterItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_FilterItems_Name,
            I18n.Config_FilterItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(FilterItems));

        // Open Held Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.OpenHeldChest),
            value => config.OpenHeldChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(OpenHeldChest));

        // Resize Chest Capacity
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => config.ResizeChestCapacity switch
            {
                -2 => 0, // Disabled
                -1 => 8, // Unlimited
                0 => 1, // Default
                _ => 1 + (config.ResizeChestCapacity / 12),
            },
            value =>
            {
                config.ResizeChestCapacity = value switch
                {
                    0 => -2, // Disabled
                    8 => -1, // Unlimited
                    1 => 0, // Default
                    _ => (value - 1) * 12,
                };
                config.ResizeChest = config.ResizeChestCapacity switch
                {
                    -2 => FeatureOption.Disabled,
                    0 => FeatureOption.Default,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestCapacity_Name,
            I18n.Config_ResizeChestCapacity_Tooltip,
            0,
            8,
            1,
            FormatHelper.FormatChestCapacity,
            nameof(ResizeChest));

        // Resize Chest Menu
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => config.ResizeChestMenuRows,
            value =>
            {
                config.ResizeChestMenuRows = value switch
                {
                    0 => 0,
                    _ => value,
                };
                config.ResizeChestMenu = value switch
                {
                    0 => FeatureOption.Disabled,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestMenuRows_Name,
            I18n.Config_ResizeChestMenuRows_Tooltip,
            0,
            6,
            1,
            FormatHelper.FormatChestMenuRows,
            nameof(ResizeChestMenu));

        // Search Items
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.SearchItems),
            value => config.SearchItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(SearchItems));

        // Stash to Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetRangeString(config.StashToChest),
            value => config.StashToChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_StashToChest_Name,
            I18n.Config_StashToChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(StashToChest));

        // Stash to Chest Distance
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => config.StashToChestDistance switch
            {
                -1 => 6,
                _ => config.StashToChestDistance,
            },
            value => config.StashToChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_StashToChestDistance_Name,
            I18n.Config_StashToChestDistance_Tooltip,
            1,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IChestData.StashToChestDistance));

        // Stash to Chest Stacks
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => config.StashToChestStacks,
            value => config.StashToChestStacks = value,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            nameof(IChestData.StashToChestStacks));

        // Unload Chest
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetOptionString(config.UnloadChest),
            value => config.UnloadChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(UnloadChest));
    }

    private void ControlsConfig(IControlScheme config)
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Open Crafting
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.OpenCrafting,
            value => config.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip,
            nameof(IControlScheme.OpenCrafting));

        // Stash Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.StashItems,
            value => config.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip,
            nameof(IControlScheme.StashItems));

        // Lock Slots
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.LockSlot,
            value => config.LockSlot = value,
            I18n.Config_SlotLock_Name,
            I18n.Config_SlotLock_Tooltip,
            nameof(IControlScheme.LockSlot));

        // Scroll Up
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.ScrollUp,
            value => config.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip,
            nameof(IControlScheme.ScrollUp));

        // Scroll Down
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.ScrollDown,
            value => config.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip,
            nameof(IControlScheme.ScrollDown));

        // Previous Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.PreviousTab,
            value => config.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip,
            nameof(IControlScheme.PreviousTab));

        // Next Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => config.NextTab,
            value => config.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip,
            nameof(IControlScheme.NextTab));
    }
}