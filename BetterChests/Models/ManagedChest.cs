namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Helpers;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class ManagedChest : IManagedChest
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedChest" /> class.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> managed by this mod.</param>
    /// <param name="data">The <see cref="IChestData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this chest type.</param>
    public ManagedChest(Chest chest, IChestData data, string qualifiedItemId)
    {
        this.Chest = chest;
        this.Data = data;
        this.QualifiedItemId = qualifiedItemId;
        foreach (var item in this.FilterItemsList)
        {
            this.ItemMatcher.Add(item);
        }
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CarryChest,
                _ => option,
            }
            : this.Data.CarryChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CarryChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public Chest Chest { get; }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ChestMenuTabs,
                _ => option,
            }
            : this.Data.ChestMenuTabs;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ChestMenuTabs"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : this.Data.ChestMenuTabSet;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ChestMenuTabSet"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CollectItems,
                _ => option,
            }
            : this.Data.CollectItems;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CollectItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.CraftFromChest,
                _ => range,
            }
            : this.Data.CraftFromChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CraftFromChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.CraftFromChestDisableLocations.Concat(value.Split(',')))
            : this.Data.CraftFromChestDisableLocations;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.CraftFromChestDistance,
                _ => distance,
            }
            : this.Data.CraftFromChestDistance;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CraftFromChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CustomColorPicker,
                _ => option,
            }
            : this.Data.CustomColorPicker;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/CustomColorPicker"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.FilterItems,
                _ => option,
            }
            : this.Data.FilterItems;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/FilterItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.FilterItemsList.Concat(value.Split(',')))
            : this.Data.FilterItemsList;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/FilterItemsList"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public ItemMatcher ItemMatcher { get; } = new(true);

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.OpenHeldChest,
                _ => option,
            }
            : this.Data.OpenHeldChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/OpenHeldChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public string QualifiedItemId { get; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChest,
                _ => option,
            }
            : this.Data.ResizeChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ResizeChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity switch
            {
                0 => this.Data.ResizeChestCapacity,
                _ => capacity,
            }
            : this.Data.ResizeChestCapacity;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ResizeChestCapacity"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChestMenu,
                _ => option,
            }
            : this.Data.ResizeChestMenu;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ResizeChestMenu"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows switch
            {
                0 => this.Data.ResizeChestMenuRows,
                _ => rows,
            }
            : this.Data.ResizeChestMenuRows;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/ResizeChestMenuRows"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.SearchItems,
                _ => option,
            }
            : this.Data.SearchItems;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/SearchItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.StashToChest,
                _ => range,
            }
            : this.Data.StashToChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/StashToChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.StashToChestDisableLocations.Concat(value.Split(',')))
            : this.Data.StashToChestDisableLocations;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/StashToChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.StashToChestDistance,
                _ => distance,
            }
            : this.Data.StashToChestDistance;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/StashToChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.StashToChestStacks,
                _ => option,
            }
            : this.Data.StashToChestStacks;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/StashToChestStacks"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Chest.modData.TryGetValue($"{BetterChests.ModUniqueId}/UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.UnloadChest,
                _ => option,
            }
            : this.Data.UnloadChest;
        set => this.Chest.modData[$"{BetterChests.ModUniqueId}/UnloadChest"] = FormatHelper.GetOptionString(value);
    }

    private IChestData Data { get; }

    /// <inheritdoc />
    public Item StashItem(Item item)
    {
        var stack = item.Stack;

        if (this.ItemMatcher.Any() && this.ItemMatcher.Matches(item))
        {
            var tmp = this.Chest.addItem(item);
            if (tmp is null || tmp.Stack <= 0)
            {
                return null;
            }

            if (tmp.Stack != stack)
            {
                item.Stack = tmp.Stack;
            }
        }

        if (this.StashToChestStacks != FeatureOption.Disabled)
        {
            foreach (var chestItem in this.Chest.items.Where(chestItem => chestItem.maximumStackSize() > 1 && chestItem.canStackWith(item)))
            {
                if (chestItem.getRemainingStackSpace() > 0)
                {
                    item.Stack = chestItem.addToStack(item);
                }

                if (item.Stack <= 0)
                {
                    return null;
                }
            }
        }

        return item;
    }
}