namespace StardewMods.BetterChests.Models.ManagedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects.Storages;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc cref="IManagedStorage" />
internal class ManagedStorage : BaseStorage, IManagedStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedStorage" /> class.
    /// </summary>
    /// <param name="container">The storage container.</param>
    /// <param name="data">The <see cref="IStorageData" /> for this type of storage.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this storage type.</param>
    public ManagedStorage(IStorageContainer container, IStorageData data, string qualifiedItemId)
        : base(container.Context)
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
    public FeatureOption AutoOrganize
    {
        get => this.Data.AutoOrganize switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/AutoOrganize", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.AutoOrganize,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/AutoOrganize"] = FormatHelper.GetOptionString(value);
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
        get => this.Data.CarryChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CarryChest", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.CarryChest,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/CarryChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.ChestMenuTabs switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ChestMenuTabs", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.ChestMenuTabs,
        };
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
        get => this.Data.CollectItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CollectItems", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.CollectItems,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/CollectItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.CraftFromChest switch
        {
            FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CraftFromChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range) && range is not FeatureOptionRange.Default => range,
            FeatureOptionRange.Default => FeatureOptionRange.Disabled,
            _ => this.Data.CraftFromChest,
        };
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
        get => this.Data.CustomColorPicker switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/CustomColorPicker", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.CustomColorPicker,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/CustomColorPicker"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.Data.FilterItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItems", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.FilterItems,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/FilterItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : this.Data.FilterItemsList;
        set
        {
            this.ModData[$"{BetterChests.ModUniqueId}/FilterItemsList"] = string.Join(",", value);
            this.ItemMatcher.Clear();
            foreach (var item in this.FilterItemsList)
            {
                this.ItemMatcher.Add(item);
            }
        }
    }

    /// <inheritdoc />
    public ItemMatcher ItemMatcher { get; } = new(true);

    /// <inheritdoc />
    public override IList<Item> Items
    {
        get => this.Container.Items;
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Container.ModData;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Data.OpenHeldChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OpenHeldChest", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.OpenHeldChest,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/OpenHeldChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.Data.OrganizeChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OrganizeChest", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.OrganizeChest,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/OrganizeChest"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OrganizeChestGroupBy", out var value) && Enum.TryParse(value, out GroupBy groupBy)
            ? groupBy switch
            {
                GroupBy.Default => this.Data.OrganizeChestGroupBy,
                _ => groupBy,
            }
            : this.Data.OrganizeChestGroupBy;
        set => this.ModData[$"{BetterChests.ModUniqueId}/OrganizeChestGroupBy"] = FormatHelper.GetGroupByString(value);
    }

    /// <inheritdoc />
    public bool OrganizeChestOrderByDescending
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OrganizeChestOrderByDescending", out var value) && bool.TryParse(value, out var orderByDesc) && orderByDesc;
        set => this.ModData[$"{BetterChests.ModUniqueId}/OrganizeChestOrderByDescending"] = value.ToString();
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get => this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/OrganizeChestSortBy", out var value) && Enum.TryParse(value, out SortBy sortBy)
            ? sortBy switch
            {
                SortBy.Default => this.Data.OrganizeChestSortBy,
                _ => sortBy,
            }
            : this.Data.OrganizeChestSortBy;
        set => this.ModData[$"{BetterChests.ModUniqueId}/OrganizeChestSortBy"] = FormatHelper.GetSortByString(value);
    }

    /// <inheritdoc />
    public string QualifiedItemId { get; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.Data.ResizeChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChest", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.ResizeChest,
        };
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
        get => this.Data.ResizeChestMenu switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/ResizeChestMenu", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.ResizeChestMenu,
        };
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
        get => this.Data.SearchItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/SearchItems", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.SearchItems,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/SearchItems"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.Data.StashToChest switch
        {
            FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChest", out var value) && Enum.TryParse(value, out FeatureOptionRange range) && range is not FeatureOptionRange.Default => range,
            FeatureOptionRange.Default => FeatureOptionRange.Disabled,
            _ => this.Data.StashToChest,
        };
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
        get => this.Data.StashToChestStacks switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/StashToChestStacks", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.StashToChestStacks,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/StashToChestStacks"] = FormatHelper.GetOptionString(value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Data.UnloadChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.ModData.TryGetValue($"{BetterChests.ModUniqueId}/UnloadChest", out var value) && Enum.TryParse(value, out FeatureOption option) && option is not FeatureOption.Default => option,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.Data.UnloadChest,
        };
        set => this.ModData[$"{BetterChests.ModUniqueId}/UnloadChest"] = FormatHelper.GetOptionString(value);
    }

    private IStorageContainer Container { get; }

    private IStorageData Data { get; }

    /// <inheritdoc />
    public override Item AddItem(Item item)
    {
        return this.Container.AddItem(item);
    }

    /// <inheritdoc />
    public override void ClearNulls()
    {
        this.Container.ClearNulls();
    }

    /// <inheritdoc />
    public override void GrabInventoryItem(Item item, Farmer who)
    {
        this.Container.GrabInventoryItem(item, who);
    }

    /// <inheritdoc />
    public override void GrabStorageItem(Item item, Farmer who)
    {
        this.Container.GrabStorageItem(item, who);
    }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        this.Container.ShowMenu();
    }

    /// <inheritdoc />
    public Item StashItem(Item item)
    {
        item.resetState();
        this.ClearNulls();

        // Add item if categorization exists and matches item
        if (!this.FilterItemsList.All(filterItem => filterItem.StartsWith("!")) && this.ItemMatcher.Any() && this.ItemMatcher.Matches(item))
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