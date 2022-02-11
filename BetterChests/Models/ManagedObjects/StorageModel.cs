namespace StardewMods.BetterChests.Models.ManagedObjects;

using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;

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
    public FeatureOption CarryChest
    {
        get
        {
            if (this.Data.CarryChest != FeatureOption.Default)
            {
                return this.Data.CarryChest;
            }

            return this.DefaultStorage.CarryChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get
        {
            if (this.Data.ChestMenuTabs != FeatureOption.Default)
            {
                return this.Data.ChestMenuTabs;
            }

            return this.DefaultStorage.ChestMenuTabs != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
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
        get
        {
            if (this.Data.CollectItems != FeatureOption.Default)
            {
                return this.Data.CollectItems;
            }

            return this.DefaultStorage.CollectItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get
        {
            if (this.Data.CraftFromChest != FeatureOptionRange.Default)
            {
                return this.Data.CraftFromChest;
            }

            return this.DefaultStorage.CraftFromChest == FeatureOptionRange.Default
                ? FeatureOptionRange.Location
                : this.DefaultStorage.CraftFromChest;
        }
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
        get
        {
            if (this.Data.CustomColorPicker != FeatureOption.Default)
            {
                return this.Data.CustomColorPicker;
            }

            return this.DefaultStorage.CustomColorPicker != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get
        {
            if (this.Data.FilterItems != FeatureOption.Default)
            {
                return this.Data.FilterItems;
            }

            return this.DefaultStorage.FilterItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
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
        get
        {
            if (this.Data.OpenHeldChest != FeatureOption.Default)
            {
                return this.Data.OpenHeldChest;
            }

            return this.DefaultStorage.OpenHeldChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get
        {
            if (this.Data.ResizeChest != FeatureOption.Default)
            {
                return this.Data.ResizeChest;
            }

            return this.DefaultStorage.ResizeChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
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
        get
        {
            if (this.Data.ResizeChestMenu != FeatureOption.Default)
            {
                return this.Data.ResizeChestMenu;
            }

            return this.DefaultStorage.ResizeChestMenu != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
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
        get
        {
            if (this.Data.SearchItems != FeatureOption.Default)
            {
                return this.Data.SearchItems;
            }

            return this.DefaultStorage.SearchItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get
        {
            if (this.Data.StashToChest != FeatureOptionRange.Default)
            {
                return this.Data.StashToChest;
            }

            return this.DefaultStorage.StashToChest == FeatureOptionRange.Default
                ? FeatureOptionRange.Location
                : this.DefaultStorage.CraftFromChest;
        }
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
        get
        {
            if (this.Data.StashToChestStacks != FeatureOption.Default)
            {
                return this.Data.StashToChestStacks;
            }

            return this.DefaultStorage.StashToChestStacks != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.StashToChestStacks = value;
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get
        {
            if (this.Data.UnloadChest != FeatureOption.Default)
            {
                return this.Data.UnloadChest;
            }

            return this.DefaultStorage.UnloadChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.UnloadChest = value;
    }

    private IStorageData Data { get; }

    private IStorageData DefaultStorage { get; }
}