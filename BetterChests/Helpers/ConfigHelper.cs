namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     Handles config options.
/// </summary>
internal class ConfigHelper
{
    private ModConfig? _config;

    private ConfigHelper(
        IModHelper helper,
        IManifest manifest,
        Dictionary<string, (IFeature Feature, Func<bool> Condition)> features)
    {
        this.Helper = helper;
        this.ModManifest = manifest;
        this.Features = features;
        this.Helper.Events.GameLoop.GameLaunched += ConfigHelper.OnGameLaunched;
    }

    private static ConfigHelper? Instance { get; set; }

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            return this._config = config ?? new ModConfig();
        }
    }

    private Dictionary<string, (IFeature Feature, Func<bool> Condition)> Features { get; }

    private IModHelper Helper { get; }

    private IManifest ModManifest { get; }

    /// <summary>
    ///     Initializes <see cref="ConfigHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="features">Mod features.</param>
    /// <returns>Returns an instance of the <see cref="ConfigHelper" /> class.</returns>
    public static ModConfig Init(
        IModHelper helper,
        IManifest manifest,
        Dictionary<string, (IFeature Feature, Func<bool> Condition)> features)
    {
        ConfigHelper.Instance ??= new(helper, manifest, features);
        return ConfigHelper.Instance.Config;
    }

    /// <summary>
    ///     Sets up the main config menu.
    /// </summary>
    public static void SetupMainConfig()
    {
        ConfigHelper.Instance!.SetupConfig(ConfigHelper.Instance.ModManifest, ConfigHelper.Instance.Config.DefaultChest, true);
    }

    /// <summary>
    ///     Sets up a config menu for a specific storage.
    /// </summary>
    /// <param name="storage">The storage to configure for.</param>
    public static void SetupSpecificConfig(IStorageData storage)
    {
        ConfigHelper.Instance!.SetupConfig(ConfigHelper.Instance.ModManifest, storage, false);
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (IntegrationHelper.GMCM.IsLoaded)
        {
            ConfigHelper.SetupMainConfig();
        }
    }

    private void SaveConfig()
    {
        this.Helper.WriteConfig(this.Config);
        foreach (var (feature, condition) in this.Features.Values)
        {
            if (condition())
            {
                feature.Activate();
                continue;
            }

            feature.Deactivate();
        }
    }

    private void SetupConfig(IManifest manifest, IStorageData storage, bool main)
    {
        if (!IntegrationHelper.GMCM.IsLoaded)
        {
            return;
        }

        if (IntegrationHelper.GMCM.IsRegistered(manifest))
        {
            IntegrationHelper.GMCM.Unregister(manifest);
        }

        // Register mod configuration
        IntegrationHelper.GMCM.Register(
            manifest,
            () => this._config = new(),
            this.SaveConfig);

        // Auto Organize
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.AutoOrganize.ToStringFast(),
            value => storage.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_AutoOrganize_Name,
            I18n.Config_AutoOrganize_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.AutoOrganize));

        // Carry Chest
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.CarryChest.ToStringFast(),
            value => storage.CarryChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.CarryChest));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => this.Config.CarryChestLimit switch
                {
                    _ when storage.CarryChestSlow is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when storage.CarryChestSlow is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    _ => (int)FeatureOption.Enabled + this.Config.CarryChestLimit - 1,
                },
                value =>
                {
                    this.Config.CarryChestLimit = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        >= (int)FeatureOption.Enabled => 1 + value - (int)FeatureOption.Enabled,
                        _ => 0,
                    };
                    storage.CarryChestSlow = value switch
                    {
                        (int)FeatureOption.Default => FeatureOption.Default,
                        (int)FeatureOption.Disabled => FeatureOption.Disabled,
                        _ => FeatureOption.Enabled,
                    };
                },
                I18n.Config_CarryChestLimit_Name,
                I18n.Config_CarryChestLimit_Tooltip,
                (int)FeatureOption.Default,
                7,
                1,
                FormatHelper.FormatCarryChestLimit,
                nameof(ModConfig.CarryChestLimit));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => this.Config.CarryChestSlowAmount,
                value => this.Config.CarryChestSlowAmount = value,
                I18n.Config_CarryChestSlow_Name,
                I18n.Config_CarryChestSlow_Tooltip,
                0,
                4,
                1,
                FormatHelper.FormatCarryChestSlow,
                nameof(ModConfig.CarryChestSlowAmount));

            // Categorize Chest
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.CategorizeChest,
                value => this.Config.CategorizeChest = value,
                I18n.Config_CategorizeChest_Name,
                I18n.Config_CategorizeChest_Tooltip,
                nameof(ModConfig.CategorizeChest));
        }
        else
        {
            // Chest Label
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => storage.ChestLabel,
                value => storage.ChestLabel = value,
                I18n.Config_ChestLabel_Name,
                I18n.Config_ChestLabel_Tooltip,
                fieldId: nameof(IStorageData.ChestLabel));
        }

        // Chest Menu Tabs
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.ChestMenuTabs.ToStringFast(),
            value => storage.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_ChestMenuTabs_Name,
            I18n.Config_ChestMenuTabs_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.ChestMenuTabs));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.PreviousTab,
                value => this.Config.ControlScheme.PreviousTab = value,
                I18n.Config_PreviousTab_Name,
                I18n.Config_PreviousTab_Tooltip,
                nameof(Controls.PreviousTab));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.NextTab,
                value => this.Config.ControlScheme.NextTab = value,
                I18n.Config_NextTab_Name,
                I18n.Config_NextTab_Tooltip,
                nameof(Controls.NextTab));
        }

        // Collect Items
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.CollectItems.ToStringFast(),
            value => storage.CollectItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.CollectItems));

        if (main)
        {
            // Configurator
            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.Configure,
                value => this.Config.ControlScheme.Configure = value,
                I18n.Config_Configure_Name,
                I18n.Config_Configure_Tooltip,
                nameof(Controls.Configure));
        }

        // Craft From Chest
        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => storage.CraftFromChestDistance switch
            {
                _ when storage.CraftFromChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                _ when storage.CraftFromChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                _ when storage.CraftFromChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                >= 2 when storage.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(storage.CraftFromChestDistance)) - 1,
                _ when storage.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                _ when storage.CraftFromChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                _ => (int)FeatureOptionRange.Default,
            },
            value =>
            {
                storage.CraftFromChestDistance = value switch
                {
                    (int)FeatureOptionRange.Default => 0,
                    (int)FeatureOptionRange.Disabled => 0,
                    (int)FeatureOptionRange.Inventory => 0,
                    (int)FeatureOptionRange.World - 1 => -1,
                    (int)FeatureOptionRange.World => 0,
                    >= (int)FeatureOptionRange.Location => (int)Math.Pow(2, 1 + value - (int)FeatureOptionRange.Location),
                    _ => 0,
                };
                storage.CraftFromChest = value switch
                {
                    (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
                    (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
                    (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
                    (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
                    (int)FeatureOptionRange.World => FeatureOptionRange.World,
                    _ => FeatureOptionRange.Location,
                };
            },
            I18n.Config_CraftFromChestDistance_Name,
            I18n.Config_CraftFromChestDistance_Tooltip,
            (int)FeatureOptionRange.Default,
            (int)FeatureOptionRange.World,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IStorageData.CraftFromChest));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.OpenCrafting,
                value => this.Config.ControlScheme.OpenCrafting = value,
                I18n.Config_OpenCrafting_Name,
                I18n.Config_OpenCrafting_Tooltip,
                nameof(Controls.OpenCrafting));
        }

        // Custom Color Picker
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.CarryChest.ToStringFast(),
            value => storage.CarryChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_CarryChest_Name,
            I18n.Config_CarryChest_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.CarryChest));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => this.Config.CustomColorPickerArea.ToStringFast(),
                value => this.Config.CustomColorPickerArea = ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
                I18n.Config_CustomColorPickerArea_Name,
                I18n.Config_CustomColorPickerArea_Tooltip,
                new[] { ComponentArea.Left.ToStringFast(), ComponentArea.Right.ToStringFast() },
                FormatHelper.FormatArea,
                nameof(ModConfig.CustomColorPickerArea));
        }

        // Filter Items
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.CustomColorPicker.ToStringFast(),
            value => storage.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_CustomColorPicker_Name,
            I18n.Config_CustomColorPicker_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.CustomColorPicker));

        if (main)
        {
            // Label Chest
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.LabelChest,
                value => this.Config.LabelChest = value,
                I18n.Config_ChestLabel_Name,
                I18n.Config_ChestLabel_Tooltip,
                nameof(ModConfig.LabelChest));
        }

        // Open Held Chest
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.OpenHeldChest.ToStringFast(),
            value => storage.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_OpenHeldChest_Name,
            I18n.Config_OpenHeldChest_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.OpenHeldChest));

        // Organize Chest
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.OrganizeChest.ToStringFast(),
            value => storage.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_OrganizeChest_Name,
            I18n.Config_OrganizeChest_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.OrganizeChest));

        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.OrganizeChestGroupBy.ToStringFast(),
            value => storage.OrganizeChestGroupBy = GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
            I18n.Config_OrganizeChestGroupBy_Name,
            I18n.Config_OrganizeChestGroupBy_Tooltip,
            GroupByExtensions.GetNames(),
            FormatHelper.FormatGroupBy,
            nameof(IStorageData.OrganizeChestGroupBy));

        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.OrganizeChestSortBy.ToStringFast(),
            value => storage.OrganizeChestSortBy = SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
            I18n.Config_OrganizeChestSortBy_Name,
            I18n.Config_OrganizeChestSortBy_Tooltip,
            SortByExtensions.GetNames(),
            FormatHelper.FormatSortBy,
            nameof(IStorageData.OrganizeChestSortBy));

        // Resize Chest
        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => storage.ResizeChestCapacity switch
            {
                _ when storage.ResizeChest is FeatureOption.Default => (int)FeatureOption.Default,
                _ when storage.ResizeChest is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                -1 => 8,
                _ => (int)FeatureOption.Enabled + storage.ResizeChestCapacity / 12 - 1,
            },
            value =>
            {
                storage.ResizeChestCapacity = value switch
                {
                    (int)FeatureOption.Default => 0,
                    (int)FeatureOption.Disabled => 0,
                    8 => -1,
                    >= (int)FeatureOption.Enabled => 12 * (1 + value - (int)FeatureOption.Enabled),
                    _ => 0,
                };
                storage.ResizeChest = value switch
                {
                    (int)FeatureOption.Default => FeatureOption.Default,
                    (int)FeatureOption.Disabled => FeatureOption.Disabled,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestCapacity_Name,
            I18n.Config_ResizeChestCapacity_Tooltip,
            (int)FeatureOption.Default,
            8,
            1,
            FormatHelper.FormatChestCapacity,
            nameof(IStorageData.ResizeChest));

        // Resize Chest Menu
        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => storage.ResizeChestMenuRows switch
            {
                _ when storage.ResizeChestMenu is FeatureOption.Default => (int)FeatureOption.Default,
                _ when storage.ResizeChestMenu is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                _ => (int)FeatureOption.Enabled + storage.ResizeChestMenuRows - 1,
            },
            value =>
            {
                storage.ResizeChestMenuRows = value switch
                {
                    (int)FeatureOption.Default => 0,
                    (int)FeatureOption.Disabled => 0,
                    _ => 1 + value - (int)FeatureOption.Enabled,
                };
                storage.ResizeChestMenu = value switch
                {
                    (int)FeatureOption.Default => FeatureOption.Default,
                    (int)FeatureOption.Disabled => FeatureOption.Disabled,
                    _ => FeatureOption.Enabled,
                };
            },
            I18n.Config_ResizeChestMenuRows_Name,
            I18n.Config_ResizeChestMenuRows_Tooltip,
            (int)FeatureOption.Default,
            7,
            1,
            FormatHelper.FormatChestMenuRows,
            nameof(IStorageData.ResizeChestMenu));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.ScrollUp,
                value => this.Config.ControlScheme.ScrollUp = value,
                I18n.Config_ScrollUp_Name,
                I18n.Config_ScrollUp_Tooltip,
                nameof(Controls.ScrollUp));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.ScrollDown,
                value => this.Config.ControlScheme.ScrollDown = value,
                I18n.Config_ScrollDown_Name,
                I18n.Config_ScrollDown_Tooltip,
                nameof(Controls.ScrollDown));
        }

        // Search Items
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.SearchItems.ToStringFast(),
            value => storage.SearchItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_SearchItems_Name,
            I18n.Config_SearchItems_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.SearchItems));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => this.Config.SearchTagSymbol.ToString(),
                value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
                I18n.Config_SearchItemsSymbol_Name,
                I18n.Config_SearchItemsSymbol_Tooltip,
                fieldId: nameof(ModConfig.SearchTagSymbol));

            // Slot Lock
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.SlotLock,
                value => this.Config.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip,
                nameof(ModConfig.SlotLock));

            IntegrationHelper.GMCM.API.AddKeybind(
                manifest,
                () => this.Config.ControlScheme.LockSlot,
                value => this.Config.ControlScheme.LockSlot = value,
                I18n.Config_LockSlot_Name,
                I18n.Config_LockSlot_Tooltip,
                nameof(Controls.LockSlot));

            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.SlotLockHold,
                value => this.Config.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip,
                nameof(ModConfig.SlotLockHold));
        }

        // Stash To Chest
        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => storage.StashToChestDistance switch
            {
                _ when storage.StashToChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                _ when storage.StashToChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                _ when storage.StashToChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                >= 2 when storage.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(storage.StashToChestDistance)) - 1,
                _ when storage.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                _ when storage.StashToChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                _ => (int)FeatureOptionRange.Default,
            },
            value =>
            {
                storage.StashToChestDistance = value switch
                {
                    (int)FeatureOptionRange.Default => 0,
                    (int)FeatureOptionRange.Disabled => 0,
                    (int)FeatureOptionRange.Inventory => 0,
                    (int)FeatureOptionRange.World - 1 => -1,
                    (int)FeatureOptionRange.World => 0,
                    >= (int)FeatureOptionRange.Location => (int)Math.Pow(2, 1 + value - (int)FeatureOptionRange.Location),
                    _ => 0,
                };
                storage.StashToChest = value switch
                {
                    (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
                    (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
                    (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
                    (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
                    (int)FeatureOptionRange.World => FeatureOptionRange.World,
                    _ => FeatureOptionRange.Location,
                };
            },
            I18n.Config_StashToChestDistance_Name,
            I18n.Config_StashToChestDistance_Tooltip,
            (int)FeatureOptionRange.Default,
            (int)FeatureOptionRange.World,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IStorageData.StashToChest));

        if (main)
        {
            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.StashItems,
                value => this.Config.ControlScheme.StashItems = value,
                I18n.Config_StashItems_Name,
                I18n.Config_StashItems_Tooltip,
                nameof(Controls.StashItems));
        }

        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => storage.StashToChestPriority,
            value => storage.StashToChestPriority = value,
            I18n.Config_StashToChestPriority_Name,
            I18n.Config_StashToChestPriority_Tooltip,
            fieldId: nameof(IStorageData.StashToChestPriority));

        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.StashToChestStacks.ToStringFast(),
            value => storage.StashToChestStacks = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.StashToChestStacks));

        // Unload Chest
        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => storage.UnloadChest.ToStringFast(),
            value => storage.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_UnloadChest_Name,
            I18n.Config_UnloadChest_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.UnloadChest));

        if (!main)
        {
            IntegrationHelper.GMCM.API.OpenModMenu(manifest);
        }
    }
}