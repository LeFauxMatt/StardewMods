namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;

/// <inheritdoc />
internal class SerializedChestData : IChestData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializedChestData" /> class.
    /// </summary>
    /// <param name="data">The Dictionary of string keys/values representing the Chest Data.</param>
    public SerializedChestData(IDictionary<string, string> data)
    {
        this.Data = data;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Data.TryGetValue("CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["CarryChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.TryGetValue("ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["ChestMenuTabs"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Data.TryGetValue("ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["ChestMenuTabSet"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Data.TryGetValue("CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["CollectItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.TryGetValue("CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data["CraftFromChest"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.Data.TryGetValue("CraftFromChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["CraftFromChestDisableLocations"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Data.TryGetValue("CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["CraftFromChestDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.Data.TryGetValue("CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["CustomColorPicker"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.Data.TryGetValue("FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["FilterItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.Data.TryGetValue("FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["FilterItemsList"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Data.TryGetValue("OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["OpenHeldChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.Data.TryGetValue("ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["ResizeChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Data.TryGetValue("ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity
            : 0;
        set => this.Data["ResizeChestCapacity"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.Data.TryGetValue("ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["ResizeChestMenu"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.Data.TryGetValue("ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows
            : 0;
        set => this.Data["ResizeChestMenuRows"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Data.TryGetValue("SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["SearchItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.Data.TryGetValue("StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data["StashToChest"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.Data.TryGetValue("StashToChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["StashToChestDisableLocations"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Data.TryGetValue("StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["StashToChestDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.Data.TryGetValue("StashToChestPriority", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["StashToChestPriority"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.Data.TryGetValue("StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["StashToChestStacks"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Data.TryGetValue("UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["UnloadChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    private IDictionary<string, string> Data { get; }

    /// <summary>
    ///     Converts a Chest Data instance into a dictionary representation.
    /// </summary>
    /// <param name="data">The Chest Data to create a data dictionary out of.</param>
    /// <returns>A dictionary of string keys/values representing the Chest Data.</returns>
    public static IDictionary<string, string> GetData(IChestData data)
    {
        var outDict = new Dictionary<string, string>();
        data.CopyTo(new SerializedChestData(outDict));
        return outDict;
    }
}