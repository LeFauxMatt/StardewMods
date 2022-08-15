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
    ///     Populates all default values with specific values.
    /// </summary>
    public void FillDefaults()
    {
        if (this.AutoOrganize is FeatureOption.Default)
        {
            this.AutoOrganize = FeatureOption.Disabled;
        }

        if (this.CarryChest is FeatureOption.Default)
        {
            this.CarryChest = FeatureOption.Enabled;
        }

        if (this.CarryChestSlow is FeatureOption.Default)
        {
            this.CarryChestSlow = FeatureOption.Enabled;
        }

        if (this.ChestMenuTabs is FeatureOption.Default)
        {
            this.ChestMenuTabs = FeatureOption.Enabled;
        }

        if (this.CollectItems is FeatureOption.Default)
        {
            this.CollectItems = FeatureOption.Disabled;
        }

        if (this.Configurator is FeatureOption.Default)
        {
            this.Configurator = FeatureOption.Enabled;
        }

        if (this.ConfigureMenu is InGameMenu.Default)
        {
            this.ConfigureMenu = InGameMenu.Simple;
        }

        if (this.CraftFromChest is FeatureOptionRange.Default)
        {
            this.CraftFromChest = FeatureOptionRange.Location;
        }

        if (this.CraftFromChestDistance == 0)
        {
            this.CraftFromChestDistance = -1;
        }

        if (this.CustomColorPicker is FeatureOption.Default)
        {
            this.CustomColorPicker = FeatureOption.Enabled;
        }

        if (this.CustomColorPickerArea is not (ComponentArea.Left or ComponentArea.Right))
        {
            this.CustomColorPickerArea = ComponentArea.Right;
        }

        if (this.FilterItems is FeatureOption.Default)
        {
            this.FilterItems = FeatureOption.Enabled;
        }

        if (this.HideItems is FeatureOption.Default)
        {
            this.HideItems = FeatureOption.Disabled;
        }

        if (this.LabelChest is FeatureOption.Default)
        {
            this.LabelChest = FeatureOption.Enabled;
        }

        if (this.OpenHeldChest is FeatureOption.Default)
        {
            this.OpenHeldChest = FeatureOption.Enabled;
        }

        if (this.OrganizeChest is FeatureOption.Default)
        {
            this.OrganizeChest = FeatureOption.Disabled;
        }

        if (this.ResizeChest is FeatureOption.Default)
        {
            this.ResizeChest = FeatureOption.Enabled;
        }

        if (this.ResizeChestCapacity == 0)
        {
            this.ResizeChestCapacity = 60;
        }

        if (this.ResizeChestMenu is FeatureOption.Default)
        {
            this.ResizeChestMenu = FeatureOption.Enabled;
        }

        if (this.ResizeChestMenuRows == 0)
        {
            this.ResizeChestMenuRows = 5;
        }

        if (this.SearchItems is FeatureOption.Default)
        {
            this.SearchItems = FeatureOption.Enabled;
        }

        if (string.IsNullOrWhiteSpace(this.SearchTagSymbol.ToString()))
        {
            this.SearchTagSymbol = '#';
        }

        if (this.StashToChest is FeatureOptionRange.Disabled)
        {
            this.StashToChest = FeatureOptionRange.Location;
        }

        if (this.StashToChestDistance == 0)
        {
            this.StashToChestDistance = -1;
        }

        if (this.StashToChestStacks is FeatureOption.Default)
        {
            this.StashToChestStacks = FeatureOption.Enabled;
        }

        if (this.TransferItems is FeatureOption.Default)
        {
            this.TransferItems = FeatureOption.Enabled;
        }

        if (this.UnloadChest is FeatureOption.Default)
        {
            this.UnloadChest = FeatureOption.Disabled;
        }

        if (this.UnloadChestCombine is FeatureOption.Default)
        {
            this.UnloadChestCombine = FeatureOption.Disabled;
        }
    }

    /// <summary>
    ///     Resets <see cref="ModConfig" /> to default options.
    /// </summary>
    [MemberNotNull(nameof(ModConfig.ControlScheme), nameof(ModConfig.VanillaStorages))]
    public void Reset()
    {
        this.FillDefaults();
        this.BetterShippingBin = true;
        this.CarryChestLimit = 1;
        this.CarryChestSlowAmount = 1;
        this.ChestFinder = true;
        this.ControlScheme = new();
        this.SlotLock = true;
        this.SlotLockColor = Colors.Red;
        this.SlotLockHold = true;
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
        sb.AppendLine($"ConfigureMenu: {this.ConfigureMenu.ToStringFast()}");
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