namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly LocalizedTextManager localizedTextManager;
    private readonly ILog log;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="localizedTextManager">Dependency used for formatting and translating text.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        IEventPublisher eventPublisher,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        LocalizedTextManager localizedTextManager,
        ILog log,
        IManifest manifest,
        IModHelper modHelper)
        : base(eventPublisher, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.localizedTextManager = localizedTextManager;
        this.log = log;
        this.manifest = manifest;

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupMainConfig();
        }
    }

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions => this.Config.DefaultOptions;

    /// <inheritdoc />
    public int CarryChestLimit => this.Config.CarryChestLimit;

    /// <inheritdoc />
    public int CarryChestSlowLimit => this.Config.CarryChestSlowLimit;

    /// <inheritdoc />
    public Method CategorizeChestMethod => this.Config.CategorizeChestMethod;

    /// <inheritdoc />
    public Controls Controls => this.Config.Controls;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations => this.Config.CraftFromChestDisableLocations;

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench => this.Config.CraftFromWorkbench;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance => this.Config.CraftFromWorkbenchDistance;

    /// <inheritdoc />
    public bool Experimental => this.Config.Experimental;

    /// <inheritdoc />
    public Method InventoryTabMethod => this.Config.InventoryTabMethod;

    /// <inheritdoc />
    public bool LabelChest => this.Config.LabelChest;

    /// <inheritdoc />
    public FeatureOption LockItem => this.Config.LockItem;

    /// <inheritdoc />
    public bool LockItemHold => this.Config.LockItemHold;

    /// <inheritdoc />
    public Method SearchItemsMethod => this.Config.SearchItemsMethod;

    /// <inheritdoc />
    public char SearchTagSymbol => this.Config.SearchTagSymbol;

    /// <inheritdoc />
    public char SearchNegationSymbol => this.Config.SearchNegationSymbol;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations => this.Config.StashToChestDisableLocations;

    /// <inheritdoc />
    public bool StashToChestStacks => this.Config.StashToChestStacks;

    /// <inheritdoc />
    public bool UnloadChestSwap => this.Config.UnloadChestSwap;

    /// <summary>Setup the main config options.</summary>
    public void SetupMainConfig()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddPageLink(this.manifest, "Main", I18n.Section_Main_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Main_Description);

        gmcm.AddPageLink(this.manifest, "Controls", I18n.Section_Controls_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Controls_Description);

        gmcm.AddPageLink(this.manifest, "Tweaks", I18n.Section_Tweaks_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Tweaks_Description);

        gmcm.AddPageLink(this.manifest, "Storages", I18n.Section_Storages_Name);
        gmcm.AddParagraph(this.manifest, I18n.Section_Storages_Description);

        gmcm.AddPage(this.manifest, "Main", I18n.Section_Main_Name);
        this.AddMainOption(config.DefaultOptions);

        gmcm.AddPage(this.manifest, "Controls", I18n.Section_Controls_Name);
        this.AddControls(config.Controls);

        gmcm.AddPage(this.manifest, "Tweaks", I18n.Section_Tweaks_Name);
        this.AddTweaks(config);

        gmcm.AddPage(this.manifest, "Storages", I18n.Section_Storages_Name);
        gmcm.AddSectionTitle(this.manifest, I18n.Storage_Default_Name);
        gmcm.AddParagraph(this.manifest, I18n.Storage_Default_Tooltip);
    }

    /// <summary>Adds the main options to the config menu.</summary>
    /// <param name="options">The storage options to add.</param>
    public void AddMainOption(IStorageOptions options)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Auto Organize
        if (options == this.DefaultOptions || this.DefaultOptions.AutoOrganize != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.AutoOrganize.ToStringFast(),
                value => options.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Carry Chest
        if (options == this.DefaultOptions || this.DefaultOptions.CarryChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CarryChest.ToStringFast(),
                value => options.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Categorize Chest
        if (options == this.DefaultOptions || this.DefaultOptions.CategorizeChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CategorizeChest.ToStringFast(),
                value => options.CategorizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CategorizeChest_Name,
                I18n.Config_CategorizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Chest Finder
        if (options == this.DefaultOptions || this.DefaultOptions.ChestFinder != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ChestFinder.ToStringFast(),
                value => options.ChestFinder = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestFinder_Name,
                I18n.Config_ChestFinder_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Chest Info
        if (options == this.DefaultOptions || this.DefaultOptions.ChestInfo != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ChestInfo.ToStringFast(),
                value => options.ChestInfo = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestInfo_Name,
                I18n.Config_ChestInfo_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Collect Items
        if (options == this.DefaultOptions || this.DefaultOptions.CollectItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.CollectItems.ToStringFast(),
                value => options.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Configure Chest
        if (options == this.DefaultOptions || this.DefaultOptions.ConfigureChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ConfigureChest.ToStringFast(),
                value => options.ConfigureChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ConfigureChest_Name,
                I18n.Config_ConfigureChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Craft from Chest
        if (options == this.DefaultOptions || this.DefaultOptions.CraftFromChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => options.CraftFromChestDistance switch
                {
                    _ when options.CraftFromChest is RangeOption.Default => (int)RangeOption.Default,
                    _ when options.CraftFromChest is RangeOption.Disabled => (int)RangeOption.Disabled,
                    _ when options.CraftFromChest is RangeOption.Inventory => (int)RangeOption.Inventory,
                    _ when options.CraftFromChest is RangeOption.World => (int)RangeOption.World,
                    >= 2 when options.CraftFromChest is RangeOption.Location => (int)RangeOption.Location
                        + (int)Math.Ceiling(Math.Log2(options.CraftFromChestDistance))
                        - 1,
                    _ when options.CraftFromChest is RangeOption.Location => (int)RangeOption.World - 1,
                    _ => (int)RangeOption.Default,
                },
                value =>
                {
                    options.CraftFromChestDistance = value switch
                    {
                        (int)RangeOption.Default => 0,
                        (int)RangeOption.Disabled => 0,
                        (int)RangeOption.Inventory => 0,
                        (int)RangeOption.World => 0,
                        (int)RangeOption.World - 1 => -1,
                        >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                        _ => 0,
                    };

                    options.CraftFromChest = value switch
                    {
                        (int)RangeOption.Default => RangeOption.Default,
                        (int)RangeOption.Disabled => RangeOption.Disabled,
                        (int)RangeOption.Inventory => RangeOption.Inventory,
                        (int)RangeOption.World => RangeOption.World,
                        (int)RangeOption.World - 1 => RangeOption.Location,
                        _ => RangeOption.Location,
                    };
                },
                I18n.Config_CraftFromChest_Name,
                I18n.Config_CraftFromChest_Tooltip,
                (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                this.localizedTextManager.Distance);
        }

        // HSL Color Picker
        if (options == this.DefaultOptions || this.DefaultOptions.HslColorPicker != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.HslColorPicker.ToStringFast(),
                value => options.HslColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_HslColorPicker_Name,
                I18n.Config_HslColorPicker_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Inventory Tabs
        if (options == this.DefaultOptions || this.DefaultOptions.InventoryTabs != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.InventoryTabs.ToStringFast(),
                value => options.InventoryTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_InventoryTabs_Name,
                I18n.Config_InventoryTabs_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Open Held Chest
        if (options == this.DefaultOptions || this.DefaultOptions.OpenHeldChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.OpenHeldChest.ToStringFast(),
                value => options.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Organize Items
        if (options == this.DefaultOptions || this.DefaultOptions.OrganizeItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.OrganizeItems.ToStringFast(),
                value => options.OrganizeItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OrganizeItems_Name,
                I18n.Config_OrganizeItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);

            gmcm.AddTextOption(
                this.manifest,
                () => options.OrganizeItemsGroupBy.ToStringFast(),
                value => options.OrganizeItemsGroupBy = GroupByExtensions.TryParse(value, out var groupBy)
                    ? groupBy
                    : GroupBy.Default,
                I18n.Config_OrganizeItemsGroupBy_Name,
                I18n.Config_OrganizeItemsGroupBy_Tooltip,
                GroupByExtensions.GetNames(),
                this.localizedTextManager.FormatGroupBy);

            gmcm.AddTextOption(
                this.manifest,
                () => options.OrganizeItemsSortBy.ToStringFast(),
                value => options.OrganizeItemsSortBy = SortByExtensions.TryParse(value, out var sortBy)
                    ? sortBy
                    : SortBy.Default,
                I18n.Config_OrganizeItemsSortBy_Name,
                I18n.Config_OrganizeItemsSortBy_Tooltip,
                SortByExtensions.GetNames(),
                this.localizedTextManager.FormatSortBy);
        }

        // Resize Chest
        if (options == this.DefaultOptions || this.DefaultOptions.ResizeChest != CapacityOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.ResizeChest.ToStringFast(),
                value => options.ResizeChest = CapacityOptionExtensions.TryParse(value, out var capacity)
                    ? capacity
                    : CapacityOption.Default,
                I18n.Config_ResizeChest_Name,
                I18n.Config_ResizeChest_Tooltip,
                CapacityOptionExtensions.GetNames(),
                this.localizedTextManager.Capacity);
        }

        // Search Items
        if (options == this.DefaultOptions || this.DefaultOptions.SearchItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.SearchItems.ToStringFast(),
                value => options.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Stash to Chest
        if (options == this.DefaultOptions || this.DefaultOptions.StashToChest != RangeOption.Disabled)
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => options.StashToChestDistance switch
                {
                    _ when options.StashToChest is RangeOption.Default => (int)RangeOption.Default,
                    _ when options.StashToChest is RangeOption.Disabled => (int)RangeOption.Disabled,
                    _ when options.StashToChest is RangeOption.Inventory => (int)RangeOption.Inventory,
                    _ when options.StashToChest is RangeOption.World => (int)RangeOption.World,
                    >= 2 when options.StashToChest is RangeOption.Location => (int)RangeOption.Location
                        + (int)Math.Ceiling(Math.Log2(options.StashToChestDistance))
                        - 1,
                    _ when options.StashToChest is RangeOption.Location => (int)RangeOption.World - 1,
                    _ => (int)RangeOption.Default,
                },
                value =>
                {
                    options.StashToChestDistance = value switch
                    {
                        (int)RangeOption.Default => 0,
                        (int)RangeOption.Disabled => 0,
                        (int)RangeOption.Inventory => 0,
                        (int)RangeOption.World => 0,
                        (int)RangeOption.World - 1 => -1,
                        >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                        _ => 0,
                    };

                    options.StashToChest = value switch
                    {
                        (int)RangeOption.Default => RangeOption.Default,
                        (int)RangeOption.Disabled => RangeOption.Disabled,
                        (int)RangeOption.Inventory => RangeOption.Inventory,
                        (int)RangeOption.World => RangeOption.World,
                        (int)RangeOption.World - 1 => RangeOption.Location,
                        _ => RangeOption.Location,
                    };
                },
                I18n.Config_StashToChest_Name,
                I18n.Config_StashToChest_Tooltip,
                (int)RangeOption.Default,
                (int)RangeOption.World,
                1,
                this.localizedTextManager.Distance);
        }

        // Transfer Items
        if (options == this.DefaultOptions || this.DefaultOptions.TransferItems != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.TransferItems.ToStringFast(),
                value => options.TransferItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_TransferItems_Name,
                I18n.Config_TransferItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }

        // Unload Chest
        if (options == this.DefaultOptions || this.DefaultOptions.UnloadChest != FeatureOption.Disabled)
        {
            gmcm.AddTextOption(
                this.manifest,
                () => options.UnloadChest.ToStringFast(),
                value => options.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                this.localizedTextManager.FormatOption);
        }
    }

    private void AddControls(Controls controls)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Configure Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ConfigureChest,
            value => controls.ConfigureChest = value,
            I18n.Controls_ConfigureChest_Name,
            I18n.Controls_ConfigureChest_Tooltip);

        // Chest Finder
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.FindChest,
            value => controls.FindChest = value,
            I18n.Controls_FindChest_Name,
            I18n.Controls_FindChest_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.OpenFoundChest,
            value => controls.OpenFoundChest = value,
            I18n.Controls_OpenFoundChest_Name,
            I18n.Controls_OpenFoundChest_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.CloseChestFinder,
            value => controls.CloseChestFinder = value,
            I18n.Controls_CloseChestFinder_Name,
            I18n.Controls_CloseChestFinder_Tooltip);

        // Craft from Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.OpenCrafting,
            value => controls.OpenCrafting = value,
            I18n.Controls_OpenCrafting_Name,
            I18n.Controls_OpenCrafting_Tooltip);

        // Stash to Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.StashItems,
            value => controls.StashItems = value,
            I18n.Controls_StashItems_Name,
            I18n.Controls_StashItems_Tooltip);

        // Inventory Tabs
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.PreviousTab,
            value => controls.PreviousTab = value,
            I18n.Controls_PreviousTab_Name,
            I18n.Controls_PreviousTab_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.NextTab,
            value => controls.NextTab = value,
            I18n.Controls_NextTab_Name,
            I18n.Controls_NextTab_Tooltip);

        // Resize Chest
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollUp,
            value => controls.ScrollUp = value,
            I18n.Controls_ScrollUp_Name,
            I18n.Controls_ScrollUp_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollDown,
            value => controls.ScrollDown = value,
            I18n.Controls_ScrollDown_Name,
            I18n.Controls_ScrollDown_Tooltip);

        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ScrollPage,
            value => controls.ScrollPage = value,
            I18n.Controls_ScrollPage_Name,
            I18n.Controls_ScrollPage_Tooltip);

        // Lock Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.LockSlot,
            value => controls.LockSlot = value,
            I18n.Controls_LockItem_Name,
            I18n.Controls_LockItem_Tooltip);

        // Chest Info
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleInfo,
            value => controls.ToggleInfo = value,
            I18n.Controls_ToggleInfo_Name,
            I18n.Controls_ToggleInfo_Tooltip);

        // Collect Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleCollectItems,
            value => controls.ToggleCollectItems = value,
            I18n.Controls_ToggleCollectItems_Name,
            I18n.Controls_ToggleCollectItems_Tooltip);

        // Search Items
        gmcm.AddKeybindList(
            this.manifest,
            () => controls.ToggleSearch,
            value => controls.ToggleSearch = value,
            I18n.Controls_ToggleSearch_Name,
            I18n.Controls_ToggleSearch_Tooltip);
    }

    private void AddTweaks(DefaultConfig config)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;

        // Carry Chest Limit
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CarryChestLimit,
            value => config.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip,
            0,
            36,
            1,
            this.localizedTextManager.CarryChestLimit);

        // Carry Chest Slow Limit
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CarryChestSlowLimit,
            value => config.CarryChestSlowLimit = value,
            I18n.Config_CarryChestSlowLimit_Name,
            I18n.Config_CarryChestSlowLimit_Tooltip,
            0,
            4,
            1,
            this.localizedTextManager.CarryChestLimit);

        // Categorize Chest Method
        gmcm.AddTextOption(
            this.manifest,
            () => config.CategorizeChestMethod.ToStringFast(),
            value => config.CategorizeChestMethod =
                MethodExtensions.TryParse(value, out var method) ? method : Method.Default,
            I18n.Config_CategorizeChestMethod_Name,
            I18n.Config_CategorizeChestMethod_Tooltip,
            MethodExtensions.GetNames(),
            this.localizedTextManager.FormatMethod);

        // TODO: Move Workbench into an object type config for workbench
        // Craft From Workbench
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CraftFromWorkbenchDistance switch
            {
                _ when config.CraftFromWorkbench is RangeOption.Default => (int)RangeOption.Default,
                _ when config.CraftFromWorkbench is RangeOption.Disabled => (int)RangeOption.Disabled,
                _ when config.CraftFromWorkbench is RangeOption.Inventory => (int)RangeOption.Inventory,
                _ when config.CraftFromWorkbench is RangeOption.World => (int)RangeOption.World,
                >= 2 when config.CraftFromWorkbench is RangeOption.Location => (int)RangeOption.Location
                    + (int)Math.Ceiling(Math.Log2(config.CraftFromWorkbenchDistance))
                    - 1,
                _ when config.CraftFromWorkbench is RangeOption.Location => (int)RangeOption.World - 1,
                _ => (int)RangeOption.Default,
            },
            value =>
            {
                config.CraftFromWorkbenchDistance = value switch
                {
                    (int)RangeOption.Default => 0,
                    (int)RangeOption.Disabled => 0,
                    (int)RangeOption.Inventory => 0,
                    (int)RangeOption.World => 0,
                    (int)RangeOption.World - 1 => -1,
                    >= (int)RangeOption.Location => (int)Math.Pow(2, 1 + value - (int)RangeOption.Location),
                    _ => 0,
                };

                config.CraftFromWorkbench = value switch
                {
                    (int)RangeOption.Default => RangeOption.Default,
                    (int)RangeOption.Disabled => RangeOption.Disabled,
                    (int)RangeOption.Inventory => RangeOption.Inventory,
                    (int)RangeOption.World => RangeOption.World,
                    (int)RangeOption.World - 1 => RangeOption.Location,
                    _ => RangeOption.Location,
                };
            },
            I18n.Config_CraftFromWorkbench_Name,
            I18n.Config_CraftFromWorkbench_Tooltip,
            (int)RangeOption.Default,
            (int)RangeOption.World,
            1,
            this.localizedTextManager.Distance);

        gmcm.AddBoolOption(
            this.manifest,
            () => config.Experimental,
            value => config.Experimental = value,
            I18n.Config_Experimental_Name,
            I18n.Config_Experimental_Tooltip);

        // Inventory Tab Method
        gmcm.AddTextOption(
            this.manifest,
            () => config.InventoryTabMethod.ToStringFast(),
            value => config.InventoryTabMethod =
                MethodExtensions.TryParse(value, out var method) ? method : Method.Default,
            I18n.Config_CategorizeChestMethod_Name,
            I18n.Config_CategorizeChestMethod_Tooltip,
            MethodExtensions.GetNames(),
            this.localizedTextManager.FormatMethod);

        // Label Chest
        gmcm.AddBoolOption(
            this.manifest,
            () => config.LabelChest,
            value => config.LabelChest = value,
            I18n.Config_LabelChest_Name,
            I18n.Config_LabelChest_Tooltip);

        // Lock Item
        gmcm.AddTextOption(
            this.manifest,
            () => config.LockItem.ToStringFast(),
            value => config.LockItem =
                FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_LockItem_Name,
            I18n.Config_LockItem_Tooltip,
            FeatureOptionExtensions.GetNames(),
            this.localizedTextManager.FormatOption);

        // Lock Item Hold
        gmcm.AddBoolOption(
            this.manifest,
            () => config.LockItemHold,
            value => config.LockItemHold = value,
            I18n.Config_LockItemHold_Name,
            I18n.Config_LockItemHold_Tooltip);

        // Search Items Method
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchItemsMethod.ToStringFast(),
            value => config.SearchItemsMethod =
                MethodExtensions.TryParse(value, out var method) ? method : Method.Default,
            I18n.Config_CategorizeChestMethod_Name,
            I18n.Config_CategorizeChestMethod_Tooltip,
            MethodExtensions.GetNames(),
            this.localizedTextManager.FormatMethod);

        // Search Tag Symbol
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchTagSymbol.ToString(),
            value => config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchTagSymbol_Name,
            I18n.Config_SearchTagSymbol_Tooltip);

        // Search Negation Symbol
        gmcm.AddTextOption(
            this.manifest,
            () => config.SearchNegationSymbol.ToString(),
            value => config.SearchNegationSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchNegationSymbol_Name,
            I18n.Config_SearchNegationSymbol_Tooltip);
    }
}