namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using StardewMods.BetterChests.Features;
using StardewMods.Common.Enums;

/// <summary>
///     Deprecated mod config data.
/// </summary>
internal sealed class ModConfigOld
{
    /// <summary>
    ///     Gets or sets a value indicating whether advanced config options will be shown.
    /// </summary>
    public bool AdvancedConfig { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether shipping bin will be relaunched as a regular chest inventory menu.
    /// </summary>
    public bool BetterShippingBin { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public int CarryChestLimit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public int CarryChestSlowAmount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be searched for.
    /// </summary>
    public bool ChestFinder { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether Configurator will be enabled.
    /// </summary>
    public bool Configurator { get; set; }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="BetterColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

    /// <summary>
    ///     Gets or sets the default storage configuration.
    /// </summary>
    public StorageData DefaultChest { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether items will be hidden or grayed out.
    /// </summary>
    public bool HideItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be labeled.
    /// </summary>
    public bool LabelChest { get; set; }

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; }

    /// <summary>
    ///     Gets or sets the color of locked slots.
    /// </summary>
    public Colors SlotLockColor { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock button needs to be held down.
    /// </summary>
    public bool SlotLockHold { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to add button for transferring items to/from a chest.
    /// </summary>
    public bool TransferItems { get; set; }

    /// <summary>
    ///     Gets or sets storage data for vanilla storage types.
    /// </summary>
    public Dictionary<string, StorageData> VanillaStorages { get; set; } = new();

    /// <summary>
    ///     Attempt to load new config from old config model.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">The mod config.</param>
    /// <returns>Returns true if the config could be loaded.</returns>
    public static bool TryUpdate(IModHelper helper, [NotNullWhen(true)] out ModConfig? config)
    {
        ModConfigOld? configOld = null;
        try
        {
            configOld = helper.ReadConfig<ModConfigOld>();
        }
        catch (Exception)
        {
            // ignored
        }

        if (configOld is null)
        {
            config = null;
            return false;
        }

        config = new()
        {
            AutoOrganize = configOld.DefaultChest.AutoOrganize,
            BetterShippingBin = configOld.BetterShippingBin,
            CarryChest = configOld.DefaultChest.CarryChest,
            CarryChestLimit = configOld.CarryChestLimit,
            CarryChestSlow = configOld.DefaultChest.CarryChestSlow,
            CarryChestSlowAmount = configOld.CarryChestSlowAmount,
            ChestFinder = configOld.ChestFinder,
            ChestLabel = configOld.DefaultChest.ChestLabel,
            ChestMenuTabs = configOld.DefaultChest.ChestMenuTabs,
            ChestMenuTabSet = configOld.DefaultChest.ChestMenuTabSet,
            CollectItems = configOld.DefaultChest.CollectItems,
            Configurator = configOld.Configurator ? FeatureOption.Enabled : FeatureOption.Default,
            ConfigureMenu = configOld.AdvancedConfig ? InGameMenu.Advanced : InGameMenu.Default,
            ControlScheme = configOld.ControlScheme,
            CraftFromChest = configOld.DefaultChest.CraftFromChest,
            CraftFromChestDisableLocations = configOld.DefaultChest.CraftFromChestDisableLocations,
            CraftFromChestDistance = configOld.DefaultChest.CraftFromChestDistance,
            CustomColorPicker = configOld.DefaultChest.CustomColorPicker,
            CustomColorPickerArea = configOld.CustomColorPickerArea,
            FilterItems = configOld.DefaultChest.FilterItems,
            FilterItemsList = configOld.DefaultChest.FilterItemsList,
            HideItems = configOld.HideItems ? FeatureOption.Enabled : FeatureOption.Default,
            LabelChest = configOld.LabelChest ? FeatureOption.Enabled : FeatureOption.Default,
            OpenHeldChest = configOld.DefaultChest.OpenHeldChest,
            OrganizeChest = configOld.DefaultChest.OrganizeChest,
            OrganizeChestGroupBy = configOld.DefaultChest.OrganizeChestGroupBy,
            OrganizeChestSortBy = configOld.DefaultChest.OrganizeChestSortBy,
            ResizeChest = configOld.DefaultChest.ResizeChest,
            ResizeChestCapacity = configOld.DefaultChest.ResizeChestCapacity,
            ResizeChestMenu = configOld.DefaultChest.ResizeChestMenu,
            ResizeChestMenuRows = configOld.DefaultChest.ResizeChestMenuRows,
            SearchItems = configOld.DefaultChest.SearchItems,
            SearchTagSymbol = configOld.SearchTagSymbol,
            SlotLock = configOld.SlotLock,
            SlotLockColor = configOld.SlotLockColor,
            SlotLockHold = configOld.SlotLockHold,
            StashToChest = configOld.DefaultChest.StashToChest,
            StashToChestDisableLocations = configOld.DefaultChest.StashToChestDisableLocations,
            StashToChestDistance = configOld.DefaultChest.StashToChestDistance,
            StashToChestPriority = configOld.DefaultChest.StashToChestPriority,
            StashToChestStacks = configOld.DefaultChest.StashToChestStacks,
            TransferItems = configOld.TransferItems ? FeatureOption.Enabled : FeatureOption.Default,
            UnloadChest = configOld.DefaultChest.UnloadChest,
            UnloadChestCombine = configOld.DefaultChest.UnloadChestCombine,
            VanillaStorages = configOld.VanillaStorages,
        };
        return true;
    }
}