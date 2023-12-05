namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
internal sealed class StorageData : IStorageData
{
    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption CarryChestSlow { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption ChestInfo { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public string ChestLabel { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet { get; set; } = new();

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption Configurator { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public InGameMenu ConfigureMenu { get; set; } = InGameMenu.Default;

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = new();

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption FilterItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public HashSet<string> FilterItemsList { get; set; } = new();

    /// <inheritdoc />
    public FeatureOption HideItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption LabelChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption OrganizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy { get; set; } = GroupBy.Default;

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy { get; set; } = SortBy.Default;

    /// <inheritdoc />
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; }

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = new();

    /// <inheritdoc />
    public int StashToChestDistance { get; set; }

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption TransferItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption UnloadChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public FeatureOption UnloadChestCombine { get; set; } = FeatureOption.Default;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (this.AutoOrganize is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"AutoOrganize: {this.AutoOrganize.ToStringFast()}");
        }

        if (this.CarryChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CarryChest: {this.CarryChest.ToStringFast()}");
        }

        if (this.CarryChestSlow is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CarryChestSlow: {this.CarryChestSlow.ToStringFast()}");
        }

        if (this.ChestInfo is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"ChestInfo: {this.ChestInfo.ToStringFast()}");
        }

        if (this.ChestMenuTabs is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"ChestMenuTabs: {this.ChestMenuTabs.ToStringFast()}");
        }

        if (this.CollectItems is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CollectItems: {this.CollectItems.ToStringFast()}");
        }

        if (this.Configurator is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Configurator: {this.Configurator.ToStringFast()}");
        }

        if (this.ConfigureMenu is not InGameMenu.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"ConfigureMenu: {this.ConfigureMenu.ToStringFast()}");
        }

        if (this.CraftFromChest is not FeatureOptionRange.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CraftFromChest: {this.CraftFromChest.ToStringFast()}");
        }

        if (this.CraftFromChestDisableLocations.Any())
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"CraftFromChestDisableLocations: {string.Join(',', this.CraftFromChestDisableLocations)}");
        }

        if (this.CraftFromChestDistance != 0)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"CraftFromChestDistance: {this.CraftFromChestDistance.ToString(CultureInfo.InvariantCulture)}");
        }

        if (this.CustomColorPicker is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"CustomColorPicker: {this.CustomColorPicker.ToStringFast()}");
        }

        if (this.FilterItems is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"FilterItems: {this.FilterItems.ToStringFast()}");
        }

        if (this.HideItems is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"HideItems: {this.HideItems.ToStringFast()}");
        }

        if (this.LabelChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"LabelChest: {this.LabelChest.ToStringFast()}");
        }

        if (this.OpenHeldChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"OpenHeldChest: {this.OpenHeldChest.ToStringFast()}");
        }

        if (this.OrganizeChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"OrganizeChest: {this.OrganizeChest.ToStringFast()}");
        }

        if (this.OrganizeChestGroupBy is not GroupBy.Default)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"OrganizeChestGroupBy: {this.OrganizeChestGroupBy.ToStringFast()}");
        }

        if (this.OrganizeChestSortBy is not SortBy.Default)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"OrganizeChestSortBy: {this.OrganizeChestSortBy.ToStringFast()}");
        }

        if (this.ResizeChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"ResizeChest: {this.ResizeChest.ToStringFast()}");
        }

        if (this.ResizeChestCapacity != 0)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"ResizeChestCapacity: {this.ResizeChestCapacity.ToString(CultureInfo.InvariantCulture)}");
        }

        if (this.SearchItems is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"SearchItems: {this.SearchItems.ToStringFast()}");
        }

        if (this.StashToChest is not FeatureOptionRange.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"StashToChest: {this.StashToChest.ToStringFast()}");
        }

        if (this.StashToChestDisableLocations.Any())
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"StashToChestDisableLocations: {string.Join(',', this.StashToChestDisableLocations)}");
        }

        if (this.StashToChestDistance != 0)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"StashToChestDistance: {this.StashToChestDistance.ToString(CultureInfo.InvariantCulture)}");
        }

        if (this.StashToChestStacks is not FeatureOption.Default)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"StashToChestStacks: {this.StashToChestStacks.ToStringFast()}");
        }

        if (this.TransferItems is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"TransferItems: {this.TransferItems.ToStringFast()}");
        }

        if (this.UnloadChest is not FeatureOption.Default)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"UnloadChest: {this.UnloadChest.ToStringFast()}");
        }

        if (this.UnloadChestCombine is not FeatureOption.Default)
        {
            sb.AppendLine(
                CultureInfo.InvariantCulture,
                $"UnloadChestCombine: {this.UnloadChestCombine.ToStringFast()}");
        }

        return sb.ToString();
    }
}
