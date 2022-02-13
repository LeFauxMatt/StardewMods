namespace StardewMods.BetterChests.Models.ManagedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects.Storages;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc cref="IManagedStorage" />
internal class ManagedStorage : StorageContainer, IManagedStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedStorage" /> class.
    /// </summary>
    /// <param name="container">The storage container.</param>
    /// <param name="data">The <see cref="IStorageData" /> for this type of storage.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this storage type.</param>
    public ManagedStorage(IStorageContainer container, IStorageData data, string qualifiedItemId)
        : base(container)
    {
        this.Data = data;
        this.QualifiedItemId = qualifiedItemId;

        // Initialize ItemMatcher
        foreach (var item in this.FilterItemsList)
        {
            this.ItemMatcher.Add(item);
        }
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.ResizeChest == FeatureOption.Enabled
            ? this.ResizeChestCapacity switch
            {
                -1 => int.MaxValue,
                <= 0 => Chest.capacity,
                _ => this.ResizeChestCapacity,
            }
            : Chest.capacity;
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
    public Item StashItem(Item item)
    {
        item.resetState();
        this.ClearNulls();

        // Add item if categorization exists and matches item
        if (this.ItemMatcher.Any() && this.ItemMatcher.Matches(item) && !this.FilterItemsList.SetEquals(this.Data.FilterItemsList))
        {
            item = this.AddItem(item);
        }

        // Add item if stacking is enabled and is stackable with any existing item
        if (item is not null && this.StashToChestStacks != FeatureOption.Disabled && this.Items.Any(existingItem => existingItem.canStackWith(item)))
        {
            item = this.AddItem(item);
        }

        if (item is null && this.Context is SObject obj)
        {
            obj.shakeTimer = 100;
        }

        return item;
    }
}