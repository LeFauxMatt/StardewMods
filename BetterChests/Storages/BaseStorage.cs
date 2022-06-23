namespace StardewMods.BetterChests.Storages;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal abstract class BaseStorage : IStorageData
{
    private readonly HashSet<string> _filterItemsList = new();
    private readonly ItemMatcher _filterMatcher = new(true);
    private int _capacity;
    private int _extraMenuSpace;
    private int _menuCapacity;
    private int _menuRows;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseStorage" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="location">The location of the source object.</param>
    /// <param name="position">The position of the source object.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    protected BaseStorage(object context, GameLocation? location, Vector2? position, IStorageData defaultChest)
    {
        this.Context = context;
        this.Location = location ?? Game1.currentLocation;
        this.Position = position ?? Vector2.Zero;
        this.DefaultChest = defaultChest;
        this.Data = new StorageModData(this);
        this.Type = new StorageData();
    }

    /// <summary>
    ///     Gets the actual capacity of the object's storage.
    /// </summary>
    public virtual int ActualCapacity
    {
        get => this.ResizeChestCapacity switch
        {
            < 0 => int.MaxValue,
            > 0 => this.ResizeChestCapacity,
            0 => Chest.capacity,
        };
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Data.AutoOrganize switch
        {
            _ when this.Type.AutoOrganize == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.AutoOrganize switch
            {
                FeatureOption.Default => this.DefaultChest.AutoOrganize,
                _ => this.Type.AutoOrganize,
            },
            _ => this.Data.AutoOrganize,
        };
        set => this.Data.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Data.CarryChest switch
        {
            _ when this.Type.CarryChest == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.CarryChest switch
            {
                FeatureOption.Default => this.DefaultChest.CarryChest,
                _ => this.Type.CarryChest,
            },
            _ => this.Data.CarryChest,
        };
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChestSlow
    {
        get => this.Data.CarryChestSlow switch
        {
            _ when this.Type.CarryChestSlow == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.CarryChestSlow switch
            {
                FeatureOption.Default => this.DefaultChest.CarryChest,
                _ => this.Type.CarryChestSlow,
            },
            _ => this.Data.CarryChestSlow,
        };
        set => this.Data.CarryChestSlow = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.ChestMenuTabs switch
        {
            _ when this.Type.ChestMenuTabs == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.ChestMenuTabs switch
            {
                FeatureOption.Default => this.DefaultChest.ChestMenuTabs,
                _ => this.Type.ChestMenuTabs,
            },
            _ => this.Data.ChestMenuTabs,
        };
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string>? ChestMenuTabSet
    {
        get => this.Data.ChestMenuTabSet is not null && this.Data.ChestMenuTabSet.Any()
            ? this.Data.ChestMenuTabSet
            : this.Type.ChestMenuTabSet is not null && this.Type.ChestMenuTabSet.Any()
                ? this.Type.ChestMenuTabSet
                : this.DefaultChest.ChestMenuTabSet;
        set => this.Data.ChestMenuTabSet = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Data.CollectItems switch
        {
            _ when this.Type.CollectItems == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.CollectItems switch
            {
                FeatureOption.Default => this.DefaultChest.CollectItems,
                _ => this.Type.CollectItems,
            },
            _ => this.Data.CollectItems,
        };
        set => this.Data.CollectItems = value;
    }

    /// <summary>
    ///     Gets the context object.
    /// </summary>
    public object Context { get; }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.CraftFromChest switch
        {
            _ when this.Type.CraftFromChest == FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            FeatureOptionRange.Default => this.Type.CraftFromChest switch
            {
                FeatureOptionRange.Default => this.DefaultChest.CraftFromChest,
                _ => this.Type.CraftFromChest,
            },
            _ => this.Data.CraftFromChest,
        };
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc />
    public HashSet<string>? CraftFromChestDisableLocations
    {
        get => this.Data.CraftFromChestDisableLocations is not null && this.Data.CraftFromChestDisableLocations.Any()
            ? this.Data.CraftFromChestDisableLocations
            : this.Type.CraftFromChestDisableLocations is not null && this.Type.CraftFromChestDisableLocations.Any()
                ? this.Type.CraftFromChestDisableLocations
                : this.DefaultChest.CraftFromChestDisableLocations;
        set => this.Data.CraftFromChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Data.CraftFromChestDistance != 0
            ? this.Data.CraftFromChestDistance
            : this.Type.CraftFromChestDistance != 0
                ? this.Type.CraftFromChestDistance
                : this.DefaultChest.CraftFromChestDistance;
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public FeatureOption CustomColorPicker
    {
        get => this.Data.CustomColorPicker switch
        {
            _ when this.Type.CustomColorPicker == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.CustomColorPicker switch
            {
                FeatureOption.Default => this.DefaultChest.CustomColorPicker,
                _ => this.Type.CustomColorPicker,
            },
            _ => this.Data.CustomColorPicker,
        };
        set => this.Data.CustomColorPicker = value;
    }

    /// <summary>
    ///     Gets data individually assigned to the storage object.
    /// </summary>
    public IStorageData Data { get; }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.Data.FilterItems switch
        {
            _ when this.Type.FilterItems == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.FilterItems switch
            {
                FeatureOption.Default => this.DefaultChest.FilterItems,
                _ => this.Type.FilterItems,
            },
            _ => this.Data.FilterItems,
        };
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc />
    public HashSet<string>? FilterItemsList
    {
        get => this.Data.FilterItemsList is not null && this.Data.FilterItemsList.Any()
            ? this.Data.FilterItemsList
            : this.Type.FilterItemsList is not null && this.Type.FilterItemsList.Any()
                ? this.Type.FilterItemsList
                : this.DefaultChest.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <summary>
    ///     Gets the items in the object's storage.
    /// </summary>
    public abstract IList<Item?> Items { get; }

    /// <summary>
    ///     Gets the location where this storage is placed.
    /// </summary>
    public GameLocation Location { get; }

    /// <summary>
    ///     Gets the calculated capacity of the <see cref="InventoryMenu" />.
    /// </summary>
    public int MenuCapacity
    {
        get
        {
            if (this._capacity == this.ResizeChestCapacity)
            {
                return this._menuCapacity;
            }

            return this._menuCapacity = this.MenuRows * 12;
        }
    }

    /// <summary>
    ///     Gets the extra vertical space needed for the <see cref="InventoryMenu" /> based on
    ///     <see cref="IStorageData.ResizeChestMenuRows" />.
    /// </summary>
    public int MenuExtraSpace
    {
        get
        {
            if (this._capacity == this.ResizeChestCapacity)
            {
                return this._extraMenuSpace;
            }

            return this._extraMenuSpace = (this.MenuRows - 3) * Game1.tileSize;
        }
    }

    /// <summary>
    ///     Gets the number of rows to display on the <see cref="InventoryMenu" /> based on
    ///     <see cref="IStorageData.ResizeChestMenuRows" />.
    /// </summary>
    public int MenuRows
    {
        get
        {
            if (this._capacity == this.ResizeChestCapacity)
            {
                return this._menuRows;
            }

            this._capacity = this.ResizeChestCapacity;
            return this._menuRows = this.ResizeChestCapacity switch
            {
                0 or Chest.capacity => 0,
                < 0 or >= 72 => this.ResizeChestMenuRows,
                < 72 => (int)Math.Min(this.ResizeChestMenuRows, Math.Ceiling(this.ResizeChestCapacity / 12f)),
            };
        }
    }

    /// <summary>
    ///     Gets the ModData associated with the context object.
    /// </summary>
    public abstract ModDataDictionary ModData { get; }

    /// <summary>
    ///     Gets the mutex required to lock this object.
    /// </summary>
    public virtual NetMutex? Mutex
    {
        get => default;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Data.OpenHeldChest switch
        {
            _ when this.Type.OpenHeldChest == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.OpenHeldChest switch
            {
                FeatureOption.Default => this.DefaultChest.OpenHeldChest,
                _ => this.Type.OpenHeldChest,
            },
            _ => this.Data.OpenHeldChest,
        };
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.Data.OrganizeChest switch
        {
            _ when this.Type.OrganizeChest == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.OrganizeChest switch
            {
                FeatureOption.Default => this.DefaultChest.OrganizeChest,
                _ => this.Type.OrganizeChest,
            },
            _ => this.Data.OrganizeChest,
        };
        set => this.Data.OrganizeChest = value;
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get => this.Data.OrganizeChestGroupBy switch
        {
            GroupBy.Default => this.Type.OrganizeChestGroupBy switch
            {
                GroupBy.Default => this.DefaultChest.OrganizeChestGroupBy,
                _ => this.Type.OrganizeChestGroupBy,
            },
            _ => this.Data.OrganizeChestGroupBy,
        };
        set => this.Data.OrganizeChestGroupBy = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the storage is sorted in descending order.
    /// </summary>
    public bool OrganizeChestOrderByDescending
    {
        get => this.ModData.ContainsKey("furyx639.BetterChests/OrganizeChestSortByDescending");
        set
        {
            if (value)
            {
                this.ModData["furyx639.BetterChests/OrganizeChestSortByDescending"] = "true";
                return;
            }

            this.ModData.Remove("furyx639.BetterChests/OrganizeChestSortByDescending");
        }
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get => this.Data.OrganizeChestSortBy switch
        {
            SortBy.Default => this.Type.OrganizeChestSortBy switch
            {
                SortBy.Default => this.DefaultChest.OrganizeChestSortBy,
                _ => this.Type.OrganizeChestSortBy,
            },
            _ => this.Data.OrganizeChestSortBy,
        };
        set => this.Data.OrganizeChestSortBy = value;
    }

    /// <summary>
    ///     Gets the coordinate of this object.
    /// </summary>
    public Vector2 Position { get; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.Data.ResizeChest switch
        {
            _ when this.Type.ResizeChest == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.ResizeChest switch
            {
                FeatureOption.Default => this.DefaultChest.ResizeChest,
                _ => this.Type.ResizeChest,
            },
            _ => this.Data.ResizeChest,
        };
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.Data.ResizeChestCapacity != 0
            ? this.Data.ResizeChestCapacity
            : this.Type.ResizeChestCapacity != 0
                ? this.Type.ResizeChestCapacity
                : this.DefaultChest.ResizeChestCapacity;
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc />
    public FeatureOption ResizeChestMenu
    {
        get => this.Data.ResizeChestMenu switch
        {
            _ when this.Type.ResizeChestMenu == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.ResizeChestMenu switch
            {
                FeatureOption.Default => this.DefaultChest.ResizeChestMenu,
                _ => this.Type.ResizeChestMenu,
            },
            _ => this.Data.ResizeChestMenu,
        };
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc />
    public int ResizeChestMenuRows
    {
        get => this.Data.ResizeChestMenuRows != 0
            ? this.Data.ResizeChestMenuRows
            : this.Type.ResizeChestMenuRows != 0
                ? this.Type.ResizeChestMenuRows
                : this.DefaultChest.ResizeChestMenuRows;
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Data.SearchItems switch
        {
            _ when this.Type.SearchItems == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.SearchItems switch
            {
                FeatureOption.Default => this.DefaultChest.SearchItems,
                _ => this.Type.SearchItems,
            },
            _ => this.Data.SearchItems,
        };
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.Data.StashToChest switch
        {
            _ when this.Type.StashToChest == FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            FeatureOptionRange.Default => this.Type.StashToChest switch
            {
                FeatureOptionRange.Default => this.DefaultChest.StashToChest,
                _ => this.Type.StashToChest,
            },
            _ => this.Data.StashToChest,
        };
        set => this.Data.StashToChest = value;
    }

    /// <inheritdoc />
    public HashSet<string>? StashToChestDisableLocations
    {
        get => this.Data.StashToChestDisableLocations is not null && this.Data.StashToChestDisableLocations.Any()
            ? this.Data.StashToChestDisableLocations
            : this.Type.StashToChestDisableLocations is not null && this.Type.StashToChestDisableLocations.Any()
                ? this.Type.StashToChestDisableLocations
                : this.DefaultChest.StashToChestDisableLocations;
        set => this.Data.StashToChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Data.StashToChestDistance != 0
            ? this.Data.StashToChestDistance
            : this.Type.StashToChestDistance != 0
                ? this.Type.StashToChestDistance
                : this.DefaultChest.StashToChestDistance;
        set => this.Data.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.Data.StashToChestPriority != 0
            ? this.Data.StashToChestPriority
            : this.Type.StashToChestPriority != 0
                ? this.Type.StashToChestPriority
                : this.DefaultChest.StashToChestPriority;
        set => this.Data.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.Data.StashToChestStacks switch
        {
            _ when this.Type.StashToChestStacks == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.StashToChestStacks switch
            {
                FeatureOption.Default => this.DefaultChest.StashToChestStacks,
                _ => this.Type.StashToChestStacks,
            },
            _ => this.Data.StashToChestStacks,
        };
        set => this.Data.StashToChestStacks = value;
    }

    /// <summary>
    ///     Gets the storage data for this type of storage.
    /// </summary>
    public IStorageData Type { get; }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Data.UnloadChest switch
        {
            _ when this.Type.UnloadChest == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => this.Type.UnloadChest switch
            {
                FeatureOption.Default => this.DefaultChest.UnloadChest,
                _ => this.Type.UnloadChest,
            },
            _ => this.Data.UnloadChest,
        };
        set => this.Data.UnloadChest = value;
    }

    private IStorageData DefaultChest { get; }

    /// <summary>
    ///     Attempts to add an item into the storage.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be added completely, or null if it could.</returns>
    public virtual Item? AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem is not null && existingItem.canStackWith(item)))
        {
            item.Stack = existingItem!.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Items.Count < this.ActualCapacity)
        {
            this.Items.Add(item);
            return null;
        }

        return item;
    }

    /// <summary>
    ///     Removes null items from the storage.
    /// </summary>
    public virtual void ClearNulls()
    {
        for (var index = this.Items.Count - 1; index >= 0; index--)
        {
            if (this.Items[index] is null)
            {
                this.Items.RemoveAt(index);
            }
        }
    }

    /// <summary>
    ///     Tests if a <see cref="Item" /> matches the <see cref="IStorageData.FilterItemsList" /> condition.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> to test.</param>
    /// <returns>Returns true if the <see cref="Item" /> matches the filters.</returns>
    public bool FilterMatches(Item item)
    {
        if (this.FilterItems == FeatureOption.Disabled || this.FilterItemsList?.Any() != true)
        {
            return true;
        }

        if (!this.FilterItemsList.SetEquals(this._filterItemsList))
        {
            this._filterItemsList.Clear();
            this._filterMatcher.Clear();
            foreach (var filter in this.FilterItemsList)
            {
                this._filterItemsList.Add(filter);
                this._filterMatcher.Add(filter);
            }
        }

        return this._filterMatcher.Matches(item);
    }

    /// <summary>
    ///     Grabs an item from a player into this storage container.
    /// </summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player whose inventory to grab the item from.</param>
    public virtual void GrabInventoryItem(Item item, Farmer who)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        var tmp = this.AddItem(item);
        if (tmp == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
        }

        this.ClearNulls();
        var oldId = Game1.activeClickableMenu.currentlySnappedComponent != null ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
        this.ShowMenu();
        ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = tmp;
        if (oldId != -1)
        {
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldId);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }
    }

    /// <summary>
    ///     Grab an item from this storage container.
    /// </summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player grabbing the item.</param>
    public virtual void GrabStorageItem(Item item, Farmer who)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            this.Items.Remove(item);
            this.ClearNulls();
            this.ShowMenu();
        }
    }

    /// <summary>
    ///     Gets the item tag that will be used to sort in <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="item">The <see cref="Item" />.</param>
    /// <returns>Returns the <see cref="Item" /> tag based on the <see cref="IStorageData.OrganizeChestGroupBy" /> option.</returns>
    public string OrderBy(Item item)
    {
        return this.OrganizeChestGroupBy switch
        {
            GroupBy.Category => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("category_")) ?? string.Empty,
            GroupBy.Color => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("color_")) ?? string.Empty,
            GroupBy.Name => item.DisplayName,
            GroupBy.Default or _ => string.Empty,
        };
    }

    /// <summary>
    ///     Creates an <see cref="ItemGrabMenu" /> for this storage container.
    /// </summary>
    public virtual void ShowMenu()
    {
        Game1.activeClickableMenu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            InventoryMenu.highlightAllItems,
            this.GrabInventoryItem,
            null,
            this.GrabStorageItem,
            false,
            true,
            true,
            true,
            true,
            1,
            null,
            -1,
            this.Context);
    }

    /// <summary>
    ///     Stashes an item into storage based on categorization and stack settings.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <param name="existingStacks">Whether to stash into stackable items or based on categorization.</param>
    /// <returns>Returns the <see cref="Item" /> if not all could be stashed or null if successful.</returns>
    public Item? StashItem(Item item, bool existingStacks = false)
    {
        if (existingStacks)
        {
            if (this.StashToChestStacks == FeatureOption.Disabled)
            {
                return item;
            }

            return this.Items.Any(otherItem => otherItem?.canStackWith(item) == true) ? this.AddItem(item) : item;
        }

        if (this.FilterItemsList?.All(filter => filter.StartsWith("!")) == true)
        {
            return item;
        }

        return this.FilterMatches(item) ? this.AddItem(item) : item;
    }

    /// <summary>
    ///     Gets the item tag that will be used to sub-sort in <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="item">The <see cref="Item" />.</param>
    /// <returns>Returns the <see cref="Item" /> tag based on the <see cref="IStorageData.OrganizeChestSortBy" /> option.</returns>
    public int ThenBy(Item item)
    {
        return this.OrganizeChestSortBy switch
        {
            SortBy.Quality when item is SObject obj => obj.Quality,
            SortBy.Quantity => item.Stack,
            SortBy.Type => item.Category,
            SortBy.Default or _ => 0,
        };
    }
}