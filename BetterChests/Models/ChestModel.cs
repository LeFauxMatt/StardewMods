namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;

/// <inheritdoc />
internal class ChestModel : IChestData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChestModel"/> class.
    /// </summary>
    /// <param name="chestData">ChestData representing this chest type.</param>
    /// <param name="defaultChest">ChestData representing the default options.</param>
    public ChestModel(IChestData chestData, IChestData defaultChest)
    {
        this.Data = chestData;
        this.DefaultChest = defaultChest;
    }

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get
        {
            if (this.Data.CarryChest != FeatureOption.Default)
            {
                return this.Data.CarryChest;
            }

            return this.DefaultChest.CarryChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get
        {
            if (this.Data.ChestMenuTabs != FeatureOption.Default)
            {
                return this.Data.ChestMenuTabs;
            }

            return this.DefaultChest.ChestMenuTabs != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get
        {
            if (this.Data.CollectItems != FeatureOption.Default)
            {
                return this.Data.CollectItems;
            }

            return this.DefaultChest.CollectItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get
        {
            if (this.Data.CraftFromChest != FeatureOptionRange.Default)
            {
                return this.Data.CraftFromChest;
            }

            return this.DefaultChest.CraftFromChest == FeatureOptionRange.Default
                ? FeatureOptionRange.Location
                : this.DefaultChest.CraftFromChest;
        }
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get
        {
            if (this.Data.CustomColorPicker != FeatureOption.Default)
            {
                return this.Data.CustomColorPicker;
            }

            return this.DefaultChest.CustomColorPicker != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get
        {
            if (this.Data.FilterItems != FeatureOption.Default)
            {
                return this.Data.FilterItems;
            }

            return this.DefaultChest.FilterItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get
        {
            if (this.Data.OpenHeldChest != FeatureOption.Default)
            {
                return this.Data.OpenHeldChest;
            }

            return this.DefaultChest.OpenHeldChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get
        {
            if (this.Data.ResizeChest != FeatureOption.Default)
            {
                return this.Data.ResizeChest;
            }

            return this.DefaultChest.ResizeChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get
        {
            if (this.Data.ResizeChestMenu != FeatureOption.Default)
            {
                return this.Data.ResizeChestMenu;
            }

            return this.DefaultChest.ResizeChestMenu != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get
        {
            if (this.Data.SearchItems != FeatureOption.Default)
            {
                return this.Data.SearchItems;
            }

            return this.DefaultChest.SearchItems != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get
        {
            if (this.Data.StashToChest != FeatureOptionRange.Default)
            {
                return this.Data.StashToChest;
            }

            return this.DefaultChest.StashToChest == FeatureOptionRange.Default
                ? FeatureOptionRange.Location
                : this.DefaultChest.CraftFromChest;
        }
        set => this.Data.StashToChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption UnloadChest
    {
        get
        {
            if (this.Data.UnloadChest != FeatureOption.Default)
            {
                return this.Data.UnloadChest;
            }

            return this.DefaultChest.UnloadChest != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.UnloadChest = value;
    }

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get
        {
            if (this.Data.CraftFromChestDistance != 0)
            {
                return this.Data.CraftFromChestDistance;
            }

            return this.DefaultChest.CraftFromChestDistance == 0
                ? -1
                : this.DefaultChest.CraftFromChestDistance;
        }
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public HashSet<string> ChestMenuTabSet
    {
        get => this.Data.ChestMenuTabSet.Any()
            ? this.Data.ChestMenuTabSet
            : this.DefaultChest.ChestMenuTabSet;
        set => this.Data.ChestMenuTabSet = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList.Any()
            ? this.Data.FilterItemsList
            : this.DefaultChest.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get
        {
            if (this.Data.ResizeChestCapacity != 0)
            {
                return this.Data.ResizeChestCapacity;
            }

            return this.DefaultChest.ResizeChestCapacity == 0
                ? 60
                : this.DefaultChest.ResizeChestCapacity;
        }
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get
        {
            if (this.Data.ResizeChestMenuRows != 0)
            {
                return this.Data.ResizeChestMenuRows;
            }

            return this.DefaultChest.ResizeChestMenuRows == 0
                ? 5
                : this.DefaultChest.ResizeChestMenuRows;
        }
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get
        {
            if (this.Data.StashToChestDistance != 0)
            {
                return this.Data.StashToChestDistance;
            }

            return this.DefaultChest.StashToChestDistance == 0
                ? -1
                : this.DefaultChest.StashToChestDistance;
        }
        set => this.Data.StashToChestDistance = value;
    }

    /// <inheritdoc/>
    public FeatureOption StashToChestStacks
    {
        get
        {
            if (this.Data.StashToChestStacks != FeatureOption.Default)
            {
                return this.Data.StashToChestStacks;
            }

            return this.DefaultChest.StashToChestStacks != FeatureOption.Disabled
                ? FeatureOption.Enabled
                : FeatureOption.Disabled;
        }
        set => this.Data.StashToChestStacks = value;
    }

    private IChestData DefaultChest { get; }

    private IChestData Data { get; }
}