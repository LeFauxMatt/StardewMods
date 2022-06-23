namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using Common.Enums;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;

/// <inheritdoc />
public class BetterChests : Mod
{
    private ModConfig? _config;

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

    private IDictionary<string, IFeature> Features { get; } = new Dictionary<string, IFeature>();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(helper.Translation);
        IntegrationHelper.Init(this.Helper, this.Config);
        StorageHelper.Init(this.Helper.Multiplayer, this.Config);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

        // Features
        AutoOrganize.Init(this.Helper);
        this.Features.Add(nameof(AutoOrganize), AutoOrganize.Init(this.Helper));
        this.Features.Add(nameof(BetterColorPicker), BetterColorPicker.Init(this.Helper, this.Config));
        this.Features.Add(nameof(BetterItemGrabMenu), BetterItemGrabMenu.Init(this.Helper));
        this.Features.Add(nameof(BetterShippingBin), BetterShippingBin.Init(this.Helper));
        this.Features.Add(nameof(CarryChest), CarryChest.Init(this.Helper, this.Config));
        this.Features.Add(nameof(CategorizeChest), CategorizeChest.Init(this.Helper));
        this.Features.Add(nameof(ChestMenuTabs), ChestMenuTabs.Init(this.Helper, this.Config));
        this.Features.Add(nameof(CollectItems), CollectItems.Init(this.Helper));
        this.Features.Add(nameof(CraftFromChest), CraftFromChest.Init(this.Helper, this.Config));
        this.Features.Add(nameof(FilterItems), FilterItems.Init(this.Helper));
        this.Features.Add(nameof(OpenHeldChest), OpenHeldChest.Init(this.Helper));
        this.Features.Add(nameof(OrganizeChest), OrganizeChest.Init(this.Helper));
        this.Features.Add(nameof(ResizeChest), ResizeChest.Init(this.Helper));
        this.Features.Add(nameof(ResizeChestMenu), ResizeChestMenu.Init(this.Helper));
        this.Features.Add(nameof(SearchItems), SearchItems.Init(this.Helper, this.Config));
        this.Features.Add(nameof(SlotLock), SlotLock.Init(this.Helper, this.Config));
        this.Features.Add(nameof(StashToChest), StashToChest.Init(this.Helper, this.Config));
        this.Features.Add(nameof(UnloadChest), UnloadChest.Init(this.Helper));
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs"))
        {
            e.LoadFromModFile<Dictionary<string, string>>("assets/tabs.json", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);

        this.Features[nameof(CarryChest)].Activate();
        this.Features[nameof(ChestMenuTabs)].Activate();
        this.Features[nameof(SearchItems)].Activate();

        // Activate Features
        /*if (this.Config.DefaultChest.AutoOrganize != FeatureOption.Disabled)
        {
            this.Features[nameof(AutoOrganize)].Activate();
        }

        this.Features[nameof(BetterItemGrabMenu)].Activate();
        this.Features[nameof(BetterShippingBin)].Activate();

        if (this.Config.DefaultChest.CarryChest != FeatureOption.Disabled)
        {
            this.Features[nameof(CarryChest)].Activate();
        }

        if (this.Config.CategorizeChest)
        {
            this.Features[nameof(CategorizeChest)].Activate();
        }

        if (this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled)
        {
            this.Features[nameof(ChestMenuTabs)].Activate();
        }

        if (this.Config.DefaultChest.CollectItems != FeatureOption.Disabled)
        {
            this.Features[nameof(CollectItems)].Activate();
        }

        if (this.Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled)
        {
            this.Features[nameof(CraftFromChest)].Activate();
        }

        if (this.Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled)
        {
            this.Features[nameof(BetterColorPicker)].Activate();
        }

        if (this.Config.DefaultChest.FilterItems != FeatureOption.Disabled)
        {
            this.Features[nameof(FilterItems)].Activate();
        }

        if (this.Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled)
        {
            this.Features[nameof(OpenHeldChest)].Activate();
        }

        if (this.Config.DefaultChest.OrganizeChest != FeatureOption.Disabled)
        {
            this.Features[nameof(OrganizeChest)].Activate();
        }

        if (this.Config.DefaultChest.ResizeChest != FeatureOption.Disabled)
        {
            this.Features[nameof(ResizeChest)].Activate();
        }

        if (this.Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled)
        {
            this.Features[nameof(ResizeChestMenu)].Activate();
        }

        if (this.Config.DefaultChest.SearchItems != FeatureOption.Disabled)
        {
            this.Features[nameof(SearchItems)].Activate();
        }

        if (this.Config.SlotLock)
        {
            this.Features[nameof(SlotLock)].Activate();
        }

        if (this.Config.DefaultChest.StashToChest != FeatureOptionRange.Disabled)
        {
            this.Features[nameof(StashToChest)].Activate();
        }

        if (this.Config.DefaultChest.UnloadChest != FeatureOption.Disabled)
        {
            this.Features[nameof(UnloadChest)].Activate();
        }*/

        if (gmcm.IsLoaded)
        {
            // Register mod configuration
            gmcm.Register(
                this.ModManifest,
                () => this._config = new(),
                () => this.Helper.WriteConfig(this.Config));

            // Auto Organize
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.AutoOrganize.ToStringFast(),
                value => this.Config.DefaultChest.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.AutoOrganize));

            // Carry Chest
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.CarryChest.ToStringFast(),
                value => this.Config.DefaultChest.CarryChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChest));

            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.CarryChestLimit switch
                {
                    _ when this.Config.DefaultChest.CarryChestSlow is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when this.Config.DefaultChest.CarryChestSlow is FeatureOption.Disabled => (int)FeatureOption.Disabled,
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
                    this.Config.DefaultChest.CarryChestSlow = value switch
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

            gmcm.API.AddNumberOption(
                this.ModManifest,
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
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.CategorizeChest,
                value => this.Config.CategorizeChest = value,
                I18n.Config_CategorizeChest_Name,
                I18n.Config_CategorizeChest_Tooltip,
                nameof(ModConfig.CategorizeChest));

            // Chest Menu Tabs
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.ChestMenuTabs.ToStringFast(),
                value => this.Config.DefaultChest.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_ChestMenuTabs_Name,
                I18n.Config_ChestMenuTabs_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.ChestMenuTabs));

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.PreviousTab,
                value => this.Config.ControlScheme.PreviousTab = value,
                I18n.Config_PreviousTab_Name,
                I18n.Config_PreviousTab_Tooltip,
                nameof(Controls.PreviousTab));

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.NextTab,
                value => this.Config.ControlScheme.NextTab = value,
                I18n.Config_NextTab_Name,
                I18n.Config_NextTab_Tooltip,
                nameof(Controls.NextTab));

            // Collect Items
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.CollectItems.ToStringFast(),
                value => this.Config.DefaultChest.CollectItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CollectItems));

            // Craft From Chest
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DefaultChest.CraftFromChestDistance switch
                {
                    _ when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                    _ when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                    _ when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                    >= 2 when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(this.Config.DefaultChest.CraftFromChestDistance)) - 1,
                    _ when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                    _ when this.Config.DefaultChest.CraftFromChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    this.Config.DefaultChest.CraftFromChestDistance = value switch
                    {
                        (int)FeatureOptionRange.Default => 0,
                        (int)FeatureOptionRange.Disabled => 0,
                        (int)FeatureOptionRange.Inventory => 0,
                        (int)FeatureOptionRange.World - 1 => -1,
                        (int)FeatureOptionRange.World => 0,
                        >= (int)FeatureOptionRange.Location => 2 ^ (1 + value - (int)FeatureOptionRange.Location),
                        _ => 0,
                    };
                    this.Config.DefaultChest.CraftFromChest = value switch
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

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.OpenCrafting,
                value => this.Config.ControlScheme.OpenCrafting = value,
                I18n.Config_OpenCrafting_Name,
                I18n.Config_OpenCrafting_Tooltip,
                nameof(Controls.OpenCrafting));

            // Custom Color Picker
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.CarryChest.ToStringFast(),
                value => this.Config.DefaultChest.CarryChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChest));

            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.CustomColorPickerArea.ToStringFast(),
                value => this.Config.CustomColorPickerArea = ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
                I18n.Config_CustomColorPickerArea_Name,
                I18n.Config_CustomColorPickerArea_Tooltip,
                new[] { ComponentArea.Left.ToStringFast(), ComponentArea.Right.ToStringFast() },
                FormatHelper.FormatArea,
                nameof(ModConfig.CustomColorPickerArea));

            // Filter Items
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.CustomColorPicker.ToStringFast(),
                value => this.Config.DefaultChest.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_CustomColorPicker_Name,
                I18n.Config_CustomColorPicker_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CustomColorPicker));

            // Open Held Chest
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.OpenHeldChest.ToStringFast(),
                value => this.Config.DefaultChest.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.OpenHeldChest));

            // Organize Chest
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.OrganizeChest.ToStringFast(),
                value => this.Config.DefaultChest.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_OrganizeChest_Name,
                I18n.Config_OrganizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.OrganizeChest));

            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.OrganizeChestGroupBy.ToStringFast(),
                value => this.Config.DefaultChest.OrganizeChestGroupBy = GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                I18n.Config_OrganizeChestGroupBy_Name,
                I18n.Config_OrganizeChestGroupBy_Tooltip,
                GroupByExtensions.GetNames(),
                FormatHelper.FormatGroupBy,
                nameof(IStorageData.OrganizeChestGroupBy));

            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.OrganizeChestSortBy.ToStringFast(),
                value => this.Config.DefaultChest.OrganizeChestSortBy = SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                I18n.Config_OrganizeChestSortBy_Name,
                I18n.Config_OrganizeChestSortBy_Tooltip,
                SortByExtensions.GetNames(),
                FormatHelper.FormatSortBy,
                nameof(IStorageData.OrganizeChestSortBy));

            // Resize Chest
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DefaultChest.ResizeChestCapacity switch
                {
                    _ when this.Config.DefaultChest.ResizeChest is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when this.Config.DefaultChest.ResizeChest is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    -1 => 8,
                    _ => (int)FeatureOption.Enabled + this.Config.DefaultChest.ResizeChestCapacity / 12 - 1,
                },
                value =>
                {
                    this.Config.DefaultChest.ResizeChestCapacity = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        8 => -1,
                        >= (int)FeatureOption.Enabled => 12 * (1 + value - (int)FeatureOption.Enabled),
                        _ => 0,
                    };
                    this.Config.DefaultChest.ResizeChest = value switch
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
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DefaultChest.ResizeChestMenuRows switch
                {
                    _ when this.Config.DefaultChest.ResizeChestMenu is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when this.Config.DefaultChest.ResizeChestMenu is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    _ => (int)FeatureOption.Enabled + this.Config.DefaultChest.ResizeChestMenuRows - 1,
                },
                value =>
                {
                    this.Config.DefaultChest.ResizeChestMenuRows = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        _ => 1 + value - (int)FeatureOption.Enabled,
                    };
                    this.Config.DefaultChest.ResizeChestMenu = value switch
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

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.ScrollUp,
                value => this.Config.ControlScheme.ScrollUp = value,
                I18n.Config_ScrollUp_Name,
                I18n.Config_ScrollUp_Tooltip,
                nameof(Controls.ScrollUp));

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.ScrollDown,
                value => this.Config.ControlScheme.ScrollDown = value,
                I18n.Config_ScrollDown_Name,
                I18n.Config_ScrollDown_Tooltip,
                nameof(Controls.ScrollDown));

            // Search Items
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.SearchItems.ToStringFast(),
                value => this.Config.DefaultChest.SearchItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.SearchItems));

            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.SearchTagSymbol.ToString(),
                value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
                I18n.Config_SearchItemsSymbol_Name,
                I18n.Config_SearchItemsSymbol_Tooltip,
                fieldId: nameof(ModConfig.SearchTagSymbol));

            // Slot Lock
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.SlotLock,
                value => this.Config.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip,
                nameof(ModConfig.SlotLock));

            gmcm.API.AddKeybind(
                this.ModManifest,
                () => this.Config.ControlScheme.LockSlot,
                value => this.Config.ControlScheme.LockSlot = value,
                I18n.Config_LockSlot_Name,
                I18n.Config_LockSlot_Tooltip,
                nameof(Controls.LockSlot));

            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.SlotLockHold,
                value => this.Config.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip,
                nameof(ModConfig.SlotLockHold));

            // Stash To Chest
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DefaultChest.StashToChestDistance switch
                {
                    _ when this.Config.DefaultChest.StashToChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                    _ when this.Config.DefaultChest.StashToChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                    _ when this.Config.DefaultChest.StashToChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                    >= 2 when this.Config.DefaultChest.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(this.Config.DefaultChest.StashToChestDistance)) - 1,
                    _ when this.Config.DefaultChest.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                    _ when this.Config.DefaultChest.StashToChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    this.Config.DefaultChest.StashToChestDistance = value switch
                    {
                        (int)FeatureOptionRange.Default => 0,
                        (int)FeatureOptionRange.Disabled => 0,
                        (int)FeatureOptionRange.Inventory => 0,
                        (int)FeatureOptionRange.World - 1 => -1,
                        (int)FeatureOptionRange.World => 0,
                        >= (int)FeatureOptionRange.Location => 2 ^ (1 + value - (int)FeatureOptionRange.Location),
                        _ => 0,
                    };
                    this.Config.DefaultChest.StashToChest = value switch
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

            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.StashItems,
                value => this.Config.ControlScheme.StashItems = value,
                I18n.Config_StashItems_Name,
                I18n.Config_StashItems_Tooltip,
                nameof(Controls.StashItems));

            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DefaultChest.StashToChestPriority,
                value => this.Config.DefaultChest.StashToChestPriority = value,
                I18n.Config_StashToChestPriority_Name,
                I18n.Config_StashToChestPriority_Tooltip,
                fieldId: nameof(IStorageData.StashToChestPriority));

            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.StashToChestStacks.ToStringFast(),
                value => this.Config.DefaultChest.StashToChestStacks = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_StashToChestStacks_Name,
                I18n.Config_StashToChestStacks_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.StashToChestStacks));

            // Unload Chest
            gmcm.API.AddTextOption(
                this.ModManifest,
                () => this.Config.DefaultChest.UnloadChest.ToStringFast(),
                value => this.Config.DefaultChest.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.UnloadChest));
        }
    }
}