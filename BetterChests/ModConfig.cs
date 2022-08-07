namespace StardewMods.BetterChests;

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;

/// <summary>
///     Mod config data.
/// </summary>
internal class ModConfig
{
    /// <summary>
    ///     Gets or sets a value indicating whether advanced config options will be shown.
    /// </summary>
    public bool AdvancedConfig { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether shipping bin will be relaunched as a regular chest inventory menu.
    /// </summary>
    public bool BetterShippingBin { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public int CarryChestLimit { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public int CarryChestSlowAmount { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be searched for.
    /// </summary>
    public bool ChestFinder { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether Configurator will be enabled.
    /// </summary>
    public bool Configurator { get; set; } = true;

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; } = new();

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="BetterColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; } = ComponentArea.Right;

    /// <summary>
    ///     Gets or sets the default storage configuration.
    /// </summary>
    public StorageData DefaultChest { get; set; } = new()
    {
        CarryChest = FeatureOption.Enabled,
        CarryChestSlow = FeatureOption.Enabled,
        ChestMenuTabs = FeatureOption.Enabled,
        CraftFromChest = FeatureOptionRange.Location,
        CraftFromChestDistance = -1,
        CustomColorPicker = FeatureOption.Enabled,
        FilterItems = FeatureOption.Enabled,
        OpenHeldChest = FeatureOption.Enabled,
        ResizeChest = FeatureOption.Enabled,
        ResizeChestCapacity = 60,
        ResizeChestMenu = FeatureOption.Enabled,
        ResizeChestMenuRows = 5,
        SearchItems = FeatureOption.Enabled,
        StashToChest = FeatureOptionRange.Location,
        StashToChestDistance = -1,
    };

    /// <summary>
    ///     Gets or sets a value indicating whether items will be hidden or grayed out.
    /// </summary>
    public bool HideItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be labeled.
    /// </summary>
    public bool LabelChest { get; set; } = true;

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; } = '#';

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; } = true;

    /// <summary>
    ///     Gets or sets the color of locked slots.
    /// </summary>
    public Colors SlotLockColor { get; set; } = Colors.Red;

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock button needs to be held down.
    /// </summary>
    public bool SlotLockHold { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to add button for transferring items to/from a chest.
    /// </summary>
    public bool TransferItems { get; set; } = true;

    /// <summary>
    ///     Gets or sets storage data for vanilla storage types.
    /// </summary>
    public Dictionary<string, StorageData> VanillaStorages { get; set; } = new();

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"AutoOrganize: {this.DefaultChest.AutoOrganize.ToStringFast()}");
        sb.AppendLine($"BetterShippingBin: {this.BetterShippingBin.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChest: {this.DefaultChest.CarryChest.ToStringFast()}");
        sb.AppendLine($"CarryChestLimit: {this.CarryChestLimit.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChestSlow: {this.DefaultChest.CarryChestSlow.ToStringFast()}");
        sb.AppendLine($"CarryChestSlowAmount: {this.CarryChestSlowAmount.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestFinder: {this.ChestFinder.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestMenuTabs: {this.DefaultChest.ChestMenuTabs.ToStringFast()}");
        sb.AppendLine($"CollectItems: {this.DefaultChest.CollectItems.ToStringFast()}");
        sb.AppendLine($"Configurator: {this.Configurator.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CraftFromChest: {this.DefaultChest.CraftFromChest.ToStringFast()}");
        sb.AppendLine(
            $"CraftFromChestDistance: {this.DefaultChest.CraftFromChestDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine(
            $"CraftFromChestDisableLocations: {string.Join(',', this.DefaultChest.CraftFromChestDisableLocations)}");
        sb.AppendLine($"CustomColorPicker: {this.DefaultChest.CustomColorPicker.ToStringFast()}");
        sb.AppendLine($"CustomColorPickerArea: {this.CustomColorPickerArea.ToStringFast()}");
        sb.AppendLine($"FilterItems: {this.DefaultChest.FilterItems.ToStringFast()}");
        sb.AppendLine($"HideItems: {this.HideItems.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"LabelChest: {this.LabelChest.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"OpenHeldChest: {this.DefaultChest.OpenHeldChest.ToStringFast()}");
        sb.AppendLine($"OrganizeChest: {this.DefaultChest.OrganizeChest.ToStringFast()}");
        sb.AppendLine($"OrganizeChestGroupBy: {this.DefaultChest.OrganizeChestGroupBy.ToStringFast()}");
        sb.AppendLine($"OrganizeChestSortBy: {this.DefaultChest.OrganizeChestSortBy.ToStringFast()}");
        sb.AppendLine($"ResizeChest: {this.DefaultChest.ResizeChest.ToStringFast()}");
        sb.AppendLine(
            $"ResizeChestCapacity: {this.DefaultChest.ResizeChestCapacity.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ResizeChestMenu: {this.DefaultChest.ResizeChestMenu.ToStringFast()}");
        sb.AppendLine(
            $"ResizeChestMenuRows: {this.DefaultChest.ResizeChestMenuRows.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SearchItems: {this.DefaultChest.SearchItems.ToStringFast()}");
        sb.AppendLine($"SearchTagSymbol: {this.SearchTagSymbol.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLock: {this.SlotLock.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLockColor: {this.SlotLockColor.ToStringFast()}");
        sb.AppendLine($"SlotLockHold: {this.SlotLockHold.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"StashToChest: {this.DefaultChest.StashToChest.ToStringFast()}");
        sb.AppendLine(
            $"StashToChestDistance: {this.DefaultChest.StashToChestDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine(
            $"StashToChestDisableLocations: {string.Join(',', this.DefaultChest.StashToChestDisableLocations)}");
        sb.AppendLine($"StashToChestStacks: {this.DefaultChest.StashToChestStacks.ToStringFast()}");
        sb.AppendLine($"TransferItems: {this.TransferItems.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"UnloadChest: {this.DefaultChest.UnloadChest.ToStringFast()}");
        return sb.ToString();
    }
}