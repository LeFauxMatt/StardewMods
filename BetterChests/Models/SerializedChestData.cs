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
    /// Initializes a new instance of the <see cref="SerializedChestData"/> class.
    /// </summary>
    /// <param name="data">The Dictionary of string keys/values representing the Chest Data.</param>
    public SerializedChestData(IDictionary<string, string> data)
    {
        this.Data = data;
    }

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data["CarryChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/ChestMenuTabs"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data[$"{BetterChests.ModUniqueId}/ChestMenuTabSet"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/CollectItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/CraftFromChest"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data[$"{BetterChests.ModUniqueId}/CraftFromChestDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/CustomColorPicker"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/FilterItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data[$"{BetterChests.ModUniqueId}/FilterItemsList"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/OpenHeldChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/ResizeChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity
            : 0;
        set => this.Data[$"{BetterChests.ModUniqueId}/ResizeChestCapacity"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/ResizeChestMenu"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows
            : 0;
        set => this.Data[$"{BetterChests.ModUniqueId}/ResizeChestMenuRows"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/SearchItems"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/StashToChest"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data[$"{BetterChests.ModUniqueId}/StashToChestDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc/>
    public FeatureOption StashToChestStacks
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/StashToChestStacks"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc/>
    public FeatureOption UnloadChest
    {
        get => this.Data.TryGetValue($"{BetterChests.ModUniqueId}/UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option
            : FeatureOption.Default;
        set => this.Data[$"{BetterChests.ModUniqueId}/UnloadChest"] = value == FeatureOption.Default ? string.Empty : FormatHelper.GetOptionString(value);
    }

    private IDictionary<string, string> Data { get; }

    /// <summary>
    /// Converts a Chest Data instance into a dictionary representation.
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