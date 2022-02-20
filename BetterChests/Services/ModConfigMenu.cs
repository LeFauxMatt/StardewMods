namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Models.Config;
using StardewMods.BetterChests.Models.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class ModConfigMenu : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfigMenu" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ModConfigMenu(IConfigModel config, IModHelper helper, IManifest manifest, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this._assetHandler = services.Lazy<AssetHandler>();
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IConfigModel Config { get; }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    /// <summary>
    ///     Add storage feature options to GMCM based on a dictionary of string keys/values representing
    ///     <see cref="IStorageData" />.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="data">The chest data to base the config on.</param>
    public void ChestConfig(IManifest manifest, IDictionary<string, string> data)
    {
        var chestData = new SerializedStorageData(data);
        this.ChestConfig(manifest, chestData, false);
    }

    private void ChestConfig(IManifest manifest, IStorageData storageData, bool defaultConfig)
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

        this.GMCM.API.AddSectionTitle(manifest, I18n.Section_Features_Name, I18n.Section_Features_Description);

        // Carry Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.CarryChest),
            value => storageData.CarryChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CarryChest));

        // Chest Menu Tabs
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.ChestMenuTabs),
            value => storageData.ChestMenuTabs = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_ChestMenuTabs_Name,
            I18n.Config_ChestMenuTabs_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(ChestMenuTabs));

        // Collect Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.CollectItems),
            value => storageData.CollectItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CollectItems));

        // Craft from Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(storageData.CraftFromChest),
            value => storageData.CraftFromChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_CraftFromChest_Name,
            I18n.Config_CraftFromChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(CraftFromChest));

        // Craft from Chest Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => storageData.CraftFromChestDistance switch
            {
                -1 => 6,
                _ => storageData.CraftFromChestDistance,
            },
            value => storageData.CraftFromChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_CraftFromChestDistance_Name,
            I18n.Config_CraftFromChestDistance_Tooltip,
            defaultConfig ? 1 : 0,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IStorageData.CraftFromChestDistance));

        // Custom Color Picker
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.CustomColorPicker),
            value => storageData.CustomColorPicker = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_CustomColorPicker_Name,
            I18n.Config_CustomColorPicker_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(CustomColorPicker));

        // Filter Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.FilterItems),
            value => storageData.FilterItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_FilterItems_Name,
            I18n.Config_FilterItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(FilterItems));

        // Open Held Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.OpenHeldChest),
            value => storageData.OpenHeldChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(OpenHeldChest));

        // Organize Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.OrganizeChest),
            value => storageData.OrganizeChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_OrganizeChest_Name,
            I18n.Config_OrganizeChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(OrganizeChest));

        // Organize Chest Group By
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetGroupByString(storageData.OrganizeChestGroupBy),
            value => storageData.OrganizeChestGroupBy = Enum.TryParse(value, out GroupBy groupBy) ? groupBy : GroupBy.Default,
            I18n.Config_OrganizeChestGroupBy_Name,
            I18n.Config_OrganizeChestGroupBy_Tooltip,
            Enum.GetValues<GroupBy>().Select(FormatHelper.GetGroupByString).ToArray(),
            FormatHelper.FormatGroupBy,
            nameof(IStorageData.OrganizeChestGroupBy));

        // Organize Chest Sort By
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetSortByString(storageData.OrganizeChestSortBy),
            value => storageData.OrganizeChestSortBy = Enum.TryParse(value, out SortBy sortBy) ? sortBy : SortBy.Default,
            I18n.Config_OrganizeChestSortBy_Name,
            I18n.Config_OrganizeChestSortBy_Tooltip,
            Enum.GetValues<SortBy>().Select(FormatHelper.GetSortByString).ToArray(),
            FormatHelper.FormatGroupBy,
            nameof(IStorageData.OrganizeChestSortBy));

        // Resize Chest Capacity
        this.GMCM.API.AddNumberOption(
            manifest,
            () => storageData.ResizeChestCapacity switch
            {
                0 when storageData.ResizeChest is FeatureOption.Disabled => 0,
                0 => 1, // Default
                -1 => 8, // Unlimited
                _ => 1 + storageData.ResizeChestCapacity / 12,
            },
            value =>
            {
                storageData.ResizeChestCapacity = value switch
                {
                    0 or 1 => 0, // Disabled or Default
                    8 => -1, // Unlimited
                    _ => (value - 1) * 12,
                };
                storageData.ResizeChest = value switch
                {
                    0 => FeatureOption.Disabled,
                    1 => FeatureOption.Default,
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
            manifest,
            () => storageData.ResizeChestMenuRows switch
            {
                0 when storageData.ResizeChestMenu is FeatureOption.Disabled => 0,
                0 => 1, // Default
                _ => storageData.ResizeChestMenuRows + 1,
            },
            value =>
            {
                storageData.ResizeChestMenuRows = value switch
                {
                    0 or 1 => 0, // Disabled or Default
                    _ => value - 1,
                };
                storageData.ResizeChestMenu = value switch
                {
                    0 => FeatureOption.Disabled,
                    1 => FeatureOption.Default,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestMenuRows_Name,
            I18n.Config_ResizeChestMenuRows_Tooltip,
            0,
            7,
            1,
            FormatHelper.FormatChestMenuRows,
            nameof(ResizeChestMenu));

        // Search Items
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.SearchItems),
            value => storageData.SearchItems = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(SearchItems));

        // Stash to Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(storageData.StashToChest),
            value => storageData.StashToChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_StashToChest_Name,
            I18n.Config_StashToChest_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(StashToChest));

        // Stash to Chest Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => storageData.StashToChestDistance switch
            {
                -1 => 6,
                _ => storageData.StashToChestDistance,
            },
            value => storageData.StashToChestDistance = value switch
            {
                6 => -1,
                _ => value,
            },
            I18n.Config_StashToChestDistance_Name,
            I18n.Config_StashToChestDistance_Tooltip,
            defaultConfig ? 1 : 0,
            6,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IStorageData.StashToChestDistance));

        // Stash to Chest Priority
        this.GMCM.API.AddNumberOption(
            manifest,
            () => storageData.StashToChestPriority,
            value => storageData.StashToChestPriority = value,
            I18n.Config_StashToChestPriority_Name,
            I18n.Config_StashToChestPriority_Tooltip,
            fieldId: nameof(IStorageData.StashToChestPriority));

        // Stash to Chest Stacks
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.StashToChestStacks),
            value => storageData.StashToChestStacks = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(IStorageData.StashToChestStacks));

        // Unload Chest
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetOptionString(storageData.UnloadChest),
            value => storageData.UnloadChest = Enum.TryParse(value, out FeatureOption option) ? option : defaultOption,
            I18n.Config_UnloadChest_Name,
            I18n.Config_UnloadChest_Tooltip,
            optionValues,
            FormatHelper.FormatOption,
            nameof(UnloadChest));
    }

    private void ControlsConfig(IControlScheme controls)
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Lock Slot
        this.GMCM.API.AddKeybind(
            this.Manifest,
            () => controls.LockSlot,
            value => controls.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip,
            nameof(IControlScheme.LockSlot));

        // Open Crafting
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.OpenCrafting,
            value => controls.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip,
            nameof(IControlScheme.OpenCrafting));

        // Stash Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.StashItems,
            value => controls.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip,
            nameof(IControlScheme.StashItems));

        // Scroll Up
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.ScrollUp,
            value => controls.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip,
            nameof(IControlScheme.ScrollUp));

        // Scroll Down
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.ScrollDown,
            value => controls.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip,
            nameof(IControlScheme.ScrollDown));

        // Previous Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.PreviousTab,
            value => controls.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip,
            nameof(IControlScheme.PreviousTab));

        // Next Tab
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.NextTab,
            value => controls.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip,
            nameof(IControlScheme.NextTab));
    }

    private void GeneralConfig()
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Carry Chest Limit
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.CarryChestLimit switch
            {
                0 => 7,
                _ => this.Config.CarryChestLimit,
            },
            value => this.Config.CarryChestLimit = value switch
            {
                7 => 0,
                _ => value,
            },
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip,
            1,
            7,
            1,
            FormatHelper.FormatCarryChestLimit,
            nameof(IConfigData.CarryChestLimit));

        // Carry Chest Slow
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.CarryChestSlow,
            value => this.Config.CarryChestSlow = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            FormatHelper.FormatCarryChestSlow,
            nameof(IConfigData.CarryChestSlow));

        // Categorize Chest
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.CategorizeChest,
            value => this.Config.CategorizeChest = value,
            I18n.Config_CategorizeChest_Name,
            I18n.Config_CategorizeChest_Tooltip,
            nameof(CategorizeChest));

        // Slot Lock
        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.SlotLock,
            value => this.Config.SlotLock = value,
            I18n.Config_SlotLock_Name,
            I18n.Config_SlotLock_Tooltip,
            nameof(SlotLock));

        // Custom Color Picker Area
        this.GMCM.API.AddTextOption(
            this.Manifest,
            () => FormatHelper.GetAreaString(this.Config.CustomColorPickerArea),
            value => this.Config.CustomColorPickerArea = Enum.TryParse(value, out ComponentArea area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            new[] { ComponentArea.Left, ComponentArea.Right }.Select(FormatHelper.GetAreaString).ToArray(),
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
    }

    private void GenerateConfig()
    {
        // Register mod configuration
        this.GMCM.Register(
            this.Manifest,
            () =>
            {
                this.Config.Reset();
                foreach (var (_, data) in this.Assets.ChestData)
                {
                    ((IStorageData)new StorageData()).CopyTo(data);
                }
            },
            () =>
            {
                this.Config.Save();
                this.Assets.SaveChestData();
            });

        // General
        this.GeneralConfig();

        // Pages
        this.GMCM.API.AddPageLink(this.Manifest, "Features", I18n.Section_Features_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Features_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Controls", I18n.Section_Controls_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Controls_Description);
        this.GMCM.API.AddPageLink(this.Manifest, "Chests", I18n.Section_Chests_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Chests_Description);

        // Features
        this.GMCM.API.AddPage(this.Manifest, "Features");
        this.ChestConfig(this.Manifest, this.Config.DefaultChest, true);

        // Controller
        this.GMCM.API.AddPage(this.Manifest, "Controls");
        this.ControlsConfig(this.Config.ControlScheme);

        // Chests
        this.GMCM.API.AddPage(this.Manifest, "Chests");

        foreach (var (name, _) in this.Assets.ChestData.OrderBy(chestData => chestData.Key))
        {
            this.GMCM.API.AddPageLink(this.Manifest, name, () => name);
        }

        foreach (var (name, data) in this.Assets.ChestData)
        {
            this.GMCM.API.AddPage(this.Manifest, name);
            this.ChestConfig(this.Manifest, data, false);
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateConfig();
    }
}