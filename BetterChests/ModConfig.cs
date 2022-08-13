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
internal class ModConfig : StorageData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfig" /> class.
    /// </summary>
    public ModConfig()
    {
        this.Reset();
    }

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
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="BetterColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

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
    ///     Gets or sets storage data for vanilla storage types.
    /// </summary>
    public Dictionary<string, StorageData> VanillaStorages { get; set; }

    /// <summary>
    ///     Resets <see cref="ModConfig" /> to default options.
    /// </summary>
    [MemberNotNull(nameof(ModConfig.ControlScheme), nameof(ModConfig.VanillaStorages))]
    public void Reset()
    {
        this.AdvancedConfig = false;
        this.BetterShippingBin = true;
        this.CarryChest = FeatureOption.Enabled;
        this.CarryChestLimit = 1;
        this.CarryChestSlow = FeatureOption.Enabled;
        this.CarryChestSlowAmount = 1;
        this.ChestFinder = true;
        this.ChestMenuTabs = FeatureOption.Enabled;
        this.Configurator = FeatureOption.Enabled;
        this.ControlScheme = new();
        this.CraftFromChest = FeatureOptionRange.Location;
        this.CraftFromChestDistance = -1;
        this.CustomColorPicker = FeatureOption.Enabled;
        this.CustomColorPickerArea = ComponentArea.Right;
        this.FilterItems = FeatureOption.Enabled;
        this.HideItems = FeatureOption.Disabled;
        this.LabelChest = FeatureOption.Enabled;
        this.OpenHeldChest = FeatureOption.Enabled;
        this.ResizeChest = FeatureOption.Enabled;
        this.ResizeChestCapacity = 60;
        this.ResizeChestMenu = FeatureOption.Enabled;
        this.ResizeChestMenuRows = 5;
        this.SearchItems = FeatureOption.Enabled;
        this.SearchTagSymbol = '#';
        this.SlotLockColor = Colors.Red;
        this.SlotLockHold = true;
        this.StashToChest = FeatureOptionRange.Location;
        this.StashToChestDistance = -1;
        this.TransferItems = FeatureOption.Enabled;
        this.VanillaStorages = new();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"AutoOrganize: {this.AutoOrganize.ToStringFast()}");
        sb.AppendLine($"BetterShippingBin: {this.BetterShippingBin.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChest: {this.CarryChest.ToStringFast()}");
        sb.AppendLine($"CarryChestLimit: {this.CarryChestLimit.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CarryChestSlow: {this.CarryChestSlow.ToStringFast()}");
        sb.AppendLine($"CarryChestSlowAmount: {this.CarryChestSlowAmount.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestFinder: {this.ChestFinder.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ChestMenuTabs: {this.ChestMenuTabs.ToStringFast()}");
        sb.AppendLine($"CollectItems: {this.CollectItems.ToStringFast()}");
        sb.AppendLine($"Configurator: {this.Configurator.ToStringFast()}");
        sb.AppendLine($"CraftFromChest: {this.CraftFromChest.ToStringFast()}");
        sb.AppendLine($"CraftFromChestDistance: {this.CraftFromChestDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CraftFromChestDisableLocations: {string.Join(',', this.CraftFromChestDisableLocations)}");
        sb.AppendLine($"CustomColorPicker: {this.CustomColorPicker.ToStringFast()}");
        sb.AppendLine($"CustomColorPickerArea: {this.CustomColorPickerArea.ToStringFast()}");
        sb.AppendLine($"FilterItems: {this.FilterItems.ToStringFast()}");
        sb.AppendLine($"HideItems: {this.HideItems.ToStringFast()}");
        sb.AppendLine($"LabelChest: {this.LabelChest.ToStringFast()}");
        sb.AppendLine($"OpenHeldChest: {this.OpenHeldChest.ToStringFast()}");
        sb.AppendLine($"OrganizeChest: {this.OrganizeChest.ToStringFast()}");
        sb.AppendLine($"OrganizeChestGroupBy: {this.OrganizeChestGroupBy.ToStringFast()}");
        sb.AppendLine($"OrganizeChestSortBy: {this.OrganizeChestSortBy.ToStringFast()}");
        sb.AppendLine($"ResizeChest: {this.ResizeChest.ToStringFast()}");
        sb.AppendLine($"ResizeChestCapacity: {this.ResizeChestCapacity.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"ResizeChestMenu: {this.ResizeChestMenu.ToStringFast()}");
        sb.AppendLine($"ResizeChestMenuRows: {this.ResizeChestMenuRows.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SearchItems: {this.SearchItems.ToStringFast()}");
        sb.AppendLine($"SearchTagSymbol: {this.SearchTagSymbol.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLock: {this.SlotLock.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"SlotLockColor: {this.SlotLockColor.ToStringFast()}");
        sb.AppendLine($"SlotLockHold: {this.SlotLockHold.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"StashToChest: {this.StashToChest.ToStringFast()}");
        sb.AppendLine($"StashToChestDistance: {this.StashToChestDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"StashToChestDisableLocations: {string.Join(',', this.StashToChestDisableLocations)}");
        sb.AppendLine($"StashToChestStacks: {this.StashToChestStacks.ToStringFast()}");
        sb.AppendLine($"TransferItems: {this.TransferItems.ToStringFast()}");
        sb.AppendLine($"UnloadChest: {this.UnloadChest.ToStringFast()}");
        sb.AppendLine($"UnloadChestCombine: {this.UnloadChestCombine.ToStringFast()}");
        return sb.ToString();
    }
}