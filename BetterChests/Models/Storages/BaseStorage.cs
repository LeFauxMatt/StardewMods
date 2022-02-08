namespace StardewMods.BetterChests.Models.Storages;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Helpers;
using StardewValley;

/// <inheritdoc />
internal abstract class BaseStorage : IManagedStorage
{
    private const int DefaultCapacity = 36;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseStorage" /> class.
    /// </summary>
    /// <param name="context">The item storage object.</param>
    /// <param name="data">The <see cref="IStorageData" /> associated with this object.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this chest type.</param>
    protected BaseStorage(object context, IStorageData data, string qualifiedItemId)
    {
        this.Context = context;
        this.Data = data;
        this.QualifiedItemId = qualifiedItemId;
    }

    /// <inheritdoc />
    public virtual int Capacity
    {
        get => BaseStorage.DefaultCapacity;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CarryChest,
                _ => option,
            }
            : this.Data.CarryChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CarryChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ChestMenuTabs,
                _ => option,
            }
            : this.Data.ChestMenuTabs;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ChestMenuTabs"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : this.Data.ChestMenuTabSet;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ChestMenuTabSet"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CollectItems,
                _ => option,
            }
            : this.Data.CollectItems;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CollectItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public object Context { get; }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.CraftFromChest,
                _ => range,
            }
            : this.Data.CraftFromChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CraftFromChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.CraftFromChestDisableLocations.Concat(value.Split(',')))
            : this.Data.CraftFromChestDisableLocations;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.CraftFromChestDistance,
                _ => distance,
            }
            : this.Data.CraftFromChestDistance;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CraftFromChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CustomColorPicker,
                _ => option,
            }
            : this.Data.CustomColorPicker;
        set => this.ModData[$"{BetterChests.ModUniqueId}/CustomColorPicker"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.FilterItems,
                _ => option,
            }
            : this.Data.FilterItems;
        set => this.ModData[$"{BetterChests.ModUniqueId}/FilterItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.FilterItemsList.Concat(value.Split(',')))
            : this.Data.FilterItemsList;
        set => this.ModData[$"{BetterChests.ModUniqueId}/FilterItemsList"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public ItemMatcher ItemMatcher { get; } = new(true);

    /// <inheritdoc />
    public abstract List<Item> Items { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.OpenHeldChest,
                _ => option,
            }
            : this.Data.OpenHeldChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/OpenHeldChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public string QualifiedItemId { get; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChest,
                _ => option,
            }
            : this.Data.ResizeChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ResizeChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity switch
            {
                0 => this.Data.ResizeChestCapacity,
                _ => capacity,
            }
            : this.Data.ResizeChestCapacity;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ResizeChestCapacity"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChestMenu,
                _ => option,
            }
            : this.Data.ResizeChestMenu;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ResizeChestMenu"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows switch
            {
                0 => this.Data.ResizeChestMenuRows,
                _ => rows,
            }
            : this.Data.ResizeChestMenuRows;
        set => this.ModData[$"{BetterChests.ModUniqueId}/ResizeChestMenuRows"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.SearchItems,
                _ => option,
            }
            : this.Data.SearchItems;
        set => this.ModData[$"{BetterChests.ModUniqueId}/SearchItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.StashToChest,
                _ => range,
            }
            : this.Data.StashToChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.StashToChestDisableLocations.Concat(value.Split(',')))
            : this.Data.StashToChestDisableLocations;
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.StashToChestDistance,
                _ => distance,
            }
            : this.Data.StashToChestDistance;
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestPriority", out var value) && int.TryParse(value, out var priority)
            ? priority switch
            {
                0 => this.Data.StashToChestPriority,
                _ => priority,
            }
            : this.Data.StashToChestPriority;
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChestPriority"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.StashToChestStacks,
                _ => option,
            }
            : this.Data.StashToChestStacks;
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChestStacks"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.UnloadChest,
                _ => option,
            }
            : this.Data.UnloadChest;
        set => this.ModData[$"{BetterChests.ModUniqueId}/UnloadChest"] = FormatHelper.GetOptionString(value);
    }

    private IStorageData Data { get; }

    /// <inheritdoc />
    public Item AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            item.Stack = existingItem.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Items.Count < this.Capacity)
        {
            this.Items.Add(item);
            return null;
        }

        return item;
    }

    /// <inheritdoc />
    public void ClearNulls()
    {
        for (var index = this.Items.Count - 1; index >= 0; index--)
        {
            if (this.Items[index] is null)
            {
                this.Items.RemoveAt(index);
            }
        }
    }

    /// <inheritdoc/>
    public abstract void ShowMenu();

    /// <inheritdoc />
    public Item StackItems(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            if (existingItem.getRemainingStackSpace() > 0)
            {
                item.Stack = existingItem.addToStack(item);
            }

            if (item.Stack <= 0)
            {
                return null;
            }

            if (this.Items.Count < this.Capacity)
            {
                this.Items.Add(item);
                return null;
            }
        }

        return item;
    }

    /// <inheritdoc />
    public Item StashItem(Item item)
    {
        item.resetState();
        this.ClearNulls();

        if (this.ItemMatcher.Any() && this.ItemMatcher.Matches(item) && !this.FilterItemsList.SetEquals(this.Data.FilterItemsList))
        {
            item = this.AddItem(item);
            if (item is null)
            {
                if (this is StorageChest storageChest)
                {
                    storageChest.Chest.shakeTimer = 100;
                }

                return null;
            }
        }

        if (this.StashToChestStacks != FeatureOption.Disabled)
        {
            item = this.StackItems(item);
            if (item is null)
            {
                if (this is StorageChest storageChest)
                {
                    storageChest.Chest.shakeTimer = 100;
                }

                return null;
            }
        }

        return item;
    }

    /// <summary>
    /// Initializes the ItemMatcher with FilterItemList.
    /// </summary>
    protected void InitFilterItems()
    {
        foreach (var item in this.FilterItemsList)
        {
            this.ItemMatcher.Add(item);
        }
    }
}