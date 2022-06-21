#nullable disable

namespace StardewMods.BetterChests.Models.ManagedObjects;

using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;

/// <inheritdoc />
internal class StorageModel : IStorageData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageModel" /> class.
    /// </summary>
    /// <param name="storageData"><see cref="IStorageData" /> representing this storage type.</param>
    /// <param name="defaultStorage"><see cref="IStorageData" /> representing the default storage.</param>
    public StorageModel(IStorageData storageData, IStorageData defaultStorage)
    {
        this.Data = storageData;
        this.DefaultStorage = defaultStorage;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.DefaultStorage.AutoOrganize switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.AutoOrganize is not FeatureOption.Default => this.Data.AutoOrganize,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.AutoOrganize,
        };
        set => this.Data.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.DefaultStorage.CarryChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.CarryChest is not FeatureOption.Default => this.Data.CarryChest,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.CarryChest,
        };
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.DefaultStorage.ChestMenuTabs switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.ChestMenuTabs is not FeatureOption.Default => this.Data.ChestMenuTabs,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.ChestMenuTabs,
        };
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Data.ChestMenuTabSet.Any()
            ? this.Data.ChestMenuTabSet
            : this.DefaultStorage.ChestMenuTabSet;
        set => this.Data.ChestMenuTabSet = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.DefaultStorage.CollectItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.CollectItems is not FeatureOption.Default => this.Data.CollectItems,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.CollectItems,
        };
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.DefaultStorage.CraftFromChest switch
        {
            FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            _ when this.Data.CraftFromChest is not FeatureOptionRange.Default => this.Data.CraftFromChest,
            FeatureOptionRange.Default => FeatureOptionRange.Disabled,
            _ => this.DefaultStorage.CraftFromChest,
        };
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.Data.CraftFromChestDisableLocations.Any()
            ? this.Data.CraftFromChestDisableLocations
            : this.DefaultStorage.CraftFromChestDisableLocations;
        set => this.Data.CraftFromChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get
        {
            if (this.Data.CraftFromChestDistance != 0)
            {
                return this.Data.CraftFromChestDistance;
            }

            return this.DefaultStorage.CraftFromChestDistance == 0
                ? -1
                : this.DefaultStorage.CraftFromChestDistance;
        }
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.DefaultStorage.CustomColorPicker switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.CustomColorPicker is not FeatureOption.Default => this.Data.CustomColorPicker,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.CustomColorPicker,
        };
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.DefaultStorage.FilterItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.FilterItems is not FeatureOption.Default => this.Data.FilterItems,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.FilterItems,
        };
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList.Any()
            ? this.Data.FilterItemsList
            : this.DefaultStorage.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.DefaultStorage.OpenHeldChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.OpenHeldChest is not FeatureOption.Default => this.Data.OpenHeldChest,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.OpenHeldChest,
        };
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.DefaultStorage.OrganizeChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.OrganizeChest is not FeatureOption.Default => this.Data.OrganizeChest,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.OrganizeChest,
        };
        set => this.Data.OrganizeChest = value;
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get => this.Data.OrganizeChestGroupBy != GroupBy.Default ? this.Data.OrganizeChestGroupBy : this.DefaultStorage.OrganizeChestGroupBy;
        set => this.Data.OrganizeChestGroupBy = value;
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get => this.Data.OrganizeChestSortBy != SortBy.Default ? this.Data.OrganizeChestSortBy : this.DefaultStorage.OrganizeChestSortBy;
        set => this.Data.OrganizeChestSortBy = value;
    }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.DefaultStorage.ResizeChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.ResizeChest is not FeatureOption.Default => this.Data.ResizeChest,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.ResizeChest,
        };
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get
        {
            if (this.Data.ResizeChestCapacity != 0)
            {
                return this.Data.ResizeChestCapacity;
            }

            return this.DefaultStorage.ResizeChestCapacity == 0
                ? 60
                : this.DefaultStorage.ResizeChestCapacity;
        }
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.DefaultStorage.ResizeChestMenu switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.ResizeChestMenu is not FeatureOption.Default => this.Data.ResizeChestMenu,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.ResizeChestMenu,
        };
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get
        {
            if (this.Data.ResizeChestMenuRows != 0)
            {
                return this.Data.ResizeChestMenuRows;
            }

            return this.DefaultStorage.ResizeChestMenuRows == 0
                ? 5
                : this.DefaultStorage.ResizeChestMenuRows;
        }
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.DefaultStorage.SearchItems switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.SearchItems is not FeatureOption.Default => this.Data.SearchItems,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.SearchItems,
        };
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.DefaultStorage.StashToChest switch
        {
            FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            _ when this.Data.StashToChest is not FeatureOptionRange.Default => this.Data.StashToChest,
            FeatureOptionRange.Default => FeatureOptionRange.Disabled,
            _ => this.DefaultStorage.StashToChest,
        };
        set => this.Data.StashToChest = value;
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.Data.StashToChestDisableLocations.Any()
            ? this.Data.StashToChestDisableLocations
            : this.DefaultStorage.StashToChestDisableLocations;
        set => this.Data.StashToChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get
        {
            if (this.Data.StashToChestDistance != 0)
            {
                return this.Data.StashToChestDistance;
            }

            return this.DefaultStorage.StashToChestDistance == 0
                ? -1
                : this.DefaultStorage.StashToChestDistance;
        }
        set => this.Data.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.Data.StashToChestPriority != 0 ? this.Data.StashToChestPriority : this.DefaultStorage.StashToChestPriority;
        set => this.Data.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.DefaultStorage.StashToChestStacks switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.StashToChestStacks is not FeatureOption.Default => this.Data.StashToChestStacks,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.StashToChestStacks,
        };
        set => this.Data.StashToChestStacks = value;
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.DefaultStorage.UnloadChest switch
        {
            FeatureOption.Disabled => FeatureOption.Disabled,
            _ when this.Data.UnloadChest is not FeatureOption.Default => this.Data.UnloadChest,
            FeatureOption.Default => FeatureOption.Disabled,
            _ => this.DefaultStorage.UnloadChest,
        };
        set => this.Data.UnloadChest = value;
    }

    private IStorageData Data { get; }

    private IStorageData DefaultStorage { get; }
}