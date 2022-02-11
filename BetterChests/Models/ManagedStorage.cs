namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class ManagedStorage : IManagedStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedStorage" /> class.
    /// </summary>
    /// <param name="container">The storage container.</param>
    /// <param name="data">The <see cref="IStorageData" /> associated with this object.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this chest type.</param>
    public ManagedStorage(IStorageContainer container, IStorageData data, string qualifiedItemId)
    {
        this.Container = container;
        this.Data = data;
        this.QualifiedItemId = qualifiedItemId;

        // Initialize ItemMatcher
        foreach (var item in this.FilterItemsList)
        {
            this.ItemMatcher.Add(item);
        }
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CarryChest,
                _ => option,
            }
            : this.Data.CarryChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CarryChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ChestMenuTabs,
                _ => option,
            }
            : this.Data.ChestMenuTabs;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ChestMenuTabs"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : this.Data.ChestMenuTabSet;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ChestMenuTabSet"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CollectItems,
                _ => option,
            }
            : this.Data.CollectItems;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CollectItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc cref="IGameObject.Context" />
    public object Context
    {
        get => this.Container.Context;
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.CraftFromChest,
                _ => range,
            }
            : this.Data.CraftFromChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CraftFromChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.CraftFromChestDisableLocations.Concat(value.Split(',')))
            : this.Data.CraftFromChestDisableLocations;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CraftFromChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.CraftFromChestDistance,
                _ => distance,
            }
            : this.Data.CraftFromChestDistance;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CraftFromChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.CustomColorPicker,
                _ => option,
            }
            : this.Data.CustomColorPicker;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/CustomColorPicker"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.FilterItems,
                _ => option,
            }
            : this.Data.FilterItems;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/FilterItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.FilterItemsList.Concat(value.Split(',')))
            : this.Data.FilterItemsList;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/FilterItemsList"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public ItemMatcher ItemMatcher { get; } = new(true);

    /// <inheritdoc cref="IStorageContainer.Items" />
    public IList<Item> Items
    {
        get => this.Container.Items;
    }

    /// <inheritdoc cref="IGameObject.ModData" />
    public ModDataDictionary ModData
    {
        get => this.Container.ModData;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.OpenHeldChest,
                _ => option,
            }
            : this.Data.OpenHeldChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/OpenHeldChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public string QualifiedItemId { get; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChest,
                _ => option,
            }
            : this.Data.ResizeChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ResizeChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity switch
            {
                0 => this.Data.ResizeChestCapacity,
                _ => capacity,
            }
            : this.Data.ResizeChestCapacity;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ResizeChestCapacity"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.ResizeChestMenu,
                _ => option,
            }
            : this.Data.ResizeChestMenu;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ResizeChestMenu"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows switch
            {
                0 => this.Data.ResizeChestMenuRows,
                _ => rows,
            }
            : this.Data.ResizeChestMenuRows;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/ResizeChestMenuRows"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.SearchItems,
                _ => option,
            }
            : this.Data.SearchItems;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/SearchItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.StashToChest,
                _ => range,
            }
            : this.Data.StashToChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/StashToChest"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.StashToChestDisableLocations.Concat(value.Split(',')))
            : this.Data.StashToChestDisableLocations;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/StashToChestDisableLocations"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.StashToChestDistance,
                _ => distance,
            }
            : this.Data.StashToChestDistance;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/StashToChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestPriority", out var value) && int.TryParse(value, out var priority)
            ? priority switch
            {
                0 => this.Data.StashToChestPriority,
                _ => priority,
            }
            : this.Data.StashToChestPriority;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/StashToChestPriority"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.StashToChestStacks,
                _ => option,
            }
            : this.Data.StashToChestStacks;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/StashToChestStacks"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Container.ModData.TryGetValue($"{BetterChests.ModUniqueId}/UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option)
            ? option switch
            {
                FeatureOption.Default => this.Data.UnloadChest,
                _ => option,
            }
            : this.Data.UnloadChest;
        set => this.Container.ModData[$"{BetterChests.ModUniqueId}/UnloadChest"] = FormatHelper.GetOptionString(value);
    }

    private IStorageContainer Container { get; }

    private IStorageData Data { get; }

    /// <inheritdoc />
    public Item AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Container.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            item.Stack = existingItem.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Container.Items.Count < this.ResizeChestCapacity)
        {
            this.Container.Items.Add(item);
            return null;
        }

        return item;
    }

    /// <inheritdoc />
    public void ClearNulls()
    {
        for (var index = this.Container.Items.Count - 1; index >= 0; index--)
        {
            if (this.Container.Items[index] is null)
            {
                this.Container.Items.RemoveAt(index);
            }
        }
    }

    /// <inheritdoc />
    public Item StackItems(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Container.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            if (existingItem.getRemainingStackSpace() > 0)
            {
                item.Stack = existingItem.addToStack(item);
            }

            if (item.Stack <= 0)
            {
                return null;
            }

            if (this.Container.Items.Count < this.ResizeChestCapacity)
            {
                this.Container.Items.Add(item);
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
                if (this.Context is Chest chest)
                {
                    chest.shakeTimer = 100;
                }

                return null;
            }
        }

        if (this.StashToChestStacks != FeatureOption.Disabled)
        {
            item = this.StackItems(item);
            if (item is null)
            {
                if (this.Context is Chest chest)
                {
                    chest.shakeTimer = 100;
                }

                return null;
            }
        }

        return item;
    }
}