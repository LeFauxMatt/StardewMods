namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;

/// <inheritdoc />
internal class StorageModData : IStorageData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageModData" /> class.
    /// </summary>
    /// <param name="storage">The base storage object.</param>
    public StorageModData(IStorageObject storage)
    {
        this.Storage = storage;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/AutoOrganize", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/AutoOrganize"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CarryChest", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CarryChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption CarryChestSlow
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CarryChestSlow", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CarryChestSlow"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ChestMenuTabs", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ChestMenuTabs"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => new(this.ModData.TryGetValue("furyx639.BetterChests/ChestMenuTabSet", out var value) && !string.IsNullOrWhiteSpace(value) ? value.Split(',') : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/ChestMenuTabSet");
            }

            this.ModData["furyx639.BetterChests/ChestMenuTabSet"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CollectItems", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CollectItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChest", out var value) && FeatureOptionRangeExtensions.TryParse(value, true, out var range)
            ? range
            : FeatureOptionRange.Default;
        set => this.ModData["furyx639.BetterChests/CraftFromChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => new(this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value) ? value.Split(',') : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/CraftFromChestDisableLocations");
            }

            this.ModData["furyx639.BetterChests/CraftFromChestDisableLocations"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CraftFromChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.ModData["furyx639.BetterChests/CraftFromChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/CustomColorPicker", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/CustomColorPicker"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/FilterItems", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/FilterItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => new(this.ModData.TryGetValue("furyx639.BetterChests/FilterItemsList", out var value) && !string.IsNullOrWhiteSpace(value) ? value.Split(',') : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/FilterItemsList");
            }

            this.ModData["furyx639.BetterChests/FilterItemsList"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/OpenHeldChest", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/OpenHeldChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChest", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChestGroupBy", out var value) && GroupByExtensions.TryParse(value, true, out var groupBy)
            ? groupBy
            : GroupBy.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChestGroupBy"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/OrganizeChestSortBy", out var value) && SortByExtensions.TryParse(value, true, out var sortBy)
            ? sortBy
            : SortBy.Default;
        set => this.ModData["furyx639.BetterChests/OrganizeChestSortBy"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ResizeChest", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ResizeChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ResizeChestCapacity", out var value) && int.TryParse(value, out var capacity)
            ? capacity
            : 0;
        set => this.ModData["furyx639.BetterChests/ResizeChestCapacity"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ResizeChestMenu", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/ResizeChestMenu"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/ResizeChestMenuRows", out var value) && int.TryParse(value, out var rows)
            ? rows
            : 0;
        set => this.ModData["furyx639.BetterChests/ResizeChestMenuRows"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/SearchItems", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/SearchItems"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/StashToChest", out var value) && FeatureOptionRangeExtensions.TryParse(value, true, out var range)
            ? range
            : FeatureOptionRange.Default;
        set => this.ModData["furyx639.BetterChests/StashToChest"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => new(this.ModData.TryGetValue("furyx639.BetterChests/StashToChestDisableLocations", out var value) && !string.IsNullOrWhiteSpace(value) ? value.Split(',') : Array.Empty<string>());
        set
        {
            if (!value.Any())
            {
                this.ModData.Remove("furyx639.BetterChests/StashToChestDisableLocations");
            }

            this.ModData["furyx639.BetterChests/StashToChestDisableLocations"] = string.Join(",", value);
        }
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/StashToChestDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.ModData["furyx639.BetterChests/StashToChestDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/StashToChestPriority", out var value) && int.TryParse(value, out var priority)
            ? priority
            : 0;
        set => this.ModData["furyx639.BetterChests/StashToChestPriority"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/StashToChestStacks", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/StashToChestStacks"] = value.ToStringFast();
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.ModData.TryGetValue("furyx639.BetterChests/UnloadChest", out var value) && FeatureOptionExtensions.TryParse(value, true, out var option)
            ? option
            : FeatureOption.Default;
        set => this.ModData["furyx639.BetterChests/UnloadChest"] = value.ToStringFast();
    }

    private ModDataDictionary ModData
    {
        get => this.Storage.ModData;
    }

    private IStorageObject Storage { get; }
}