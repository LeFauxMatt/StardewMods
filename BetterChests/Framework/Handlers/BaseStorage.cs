namespace StardewMods.BetterChests.Framework.Handlers;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="IStorageData" />
internal abstract class BaseStorage : StorageNode, IComparable<BaseStorage>
{
    private static readonly IStorageData DefaultStorage = new StorageData();

    private readonly HashSet<string> _cachedFilterList = new();
    private readonly ItemMatcher _filterMatcher = new(true);
    private int _capacity;
    private int _menuRows;
    private int _rows;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseStorage" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    protected BaseStorage(object context, object? source, Vector2 position)
        : base(BaseStorage.DefaultStorage, BaseStorage.DefaultStorage)
    {
        this.Context = context;
        this.Source = source;
        this.Position = position;
        this.Data = new StorageModData(this);
        this._filterMatcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Gets the actual capacity of the object's storage.
    /// </summary>
    public virtual int ActualCapacity =>
        this.ResizeChestCapacity switch
        {
            < 0 => int.MaxValue,
            > 0 => this.ResizeChestCapacity,
            0 => Chest.capacity,
        };

    /// <summary>
    ///     Gets the context object.
    /// </summary>
    public object Context { get; }

    /// <summary>
    ///     Gets an ItemMatcher to represent the currently selected filters.
    /// </summary>
    public ItemMatcher FilterMatcher
    {
        get
        {
            if (this._cachedFilterList.SetEquals(this.FilterItemsList))
            {
                return this._filterMatcher;
            }

            this._filterMatcher.CollectionChanged -= this.OnCollectionChanged;
            this._cachedFilterList.Clear();
            this._filterMatcher.Clear();
            foreach (var filter in this.FilterItemsList)
            {
                this._cachedFilterList.Add(filter);
                this._filterMatcher.Add(filter);
            }

            this._filterMatcher.CollectionChanged += this.OnCollectionChanged;

            return this._filterMatcher;
        }
    }

    /// <summary>
    ///     Gets the items in the object's storage.
    /// </summary>
    public abstract IList<Item?> Items { get; }

    /// <summary>
    ///     Gets the context object's current location.
    /// </summary>
    public virtual GameLocation Location
    {
        get
        {
            var source = this.Source;
            while (source is BaseStorage parent && !ReferenceEquals(this, parent))
            {
                source = parent.Source;
            }

            return source switch
            {
                GameLocation gameLocation => gameLocation,
                Character character => character.currentLocation,
                _ => Game1.currentLocation,
            };
        }
    }

    /// <summary>
    ///     Gets the calculated capacity of the <see cref="InventoryMenu" />.
    /// </summary>
    public int MenuCapacity => this.MenuRows * 12;

    /// <summary>
    ///     Gets the number of rows to display on the <see cref="InventoryMenu" /> based on
    ///     <see cref="IStorageData.ResizeChestMenuRows" />.
    /// </summary>
    public int MenuRows
    {
        get
        {
            if (this._menuRows > 0
             && this._capacity == this.ResizeChestCapacity
             && this._rows == this.ResizeChestMenuRows)
            {
                return this._menuRows;
            }

            this._capacity = this.ResizeChestCapacity;
            this._rows = this.ResizeChestMenuRows;
            return this._menuRows = (int)Math.Min(
                this.ActualCapacity switch
                {
                    0 or Chest.capacity => 3,
                    _ when this.ResizeChestMenuRows <= 0 => 3,
                    < 0 or >= 72 => this.ResizeChestMenuRows,
                    < 72 => this.ResizeChestMenuRows,
                },
                Math.Ceiling(this.ActualCapacity / 12f));
        }
    }

    /// <summary>
    ///     Gets the ModData associated with the context object.
    /// </summary>
    public abstract ModDataDictionary ModData { get; }

    /// <summary>
    ///     Gets the mutex required to lock this object.
    /// </summary>
    public virtual NetMutex? Mutex => default;

    /// <summary>
    ///     Gets the coordinate of this object.
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    ///     Gets the source context where this storage is contained.
    /// </summary>
    public object? Source { get; }

    /// <summary>
    ///     Attempts to add an item into the storage.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be added completely, or null if it could.</returns>
    public virtual Item? AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items)
        {
            if (existingItem is null || !item.canStackWith(existingItem))
            {
                continue;
            }

            item.Stack = existingItem.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Items.Count >= this.ActualCapacity)
        {
            return item;
        }

        this.Items.Add(item);
        return null;
    }

    /// <summary>
    ///     Removes null items from the storage.
    /// </summary>
    public virtual void ClearNulls()
    {
        for (var index = this.Items.Count - 1; index >= 0; --index)
        {
            if (this.Items[index] is null)
            {
                this.Items.RemoveAt(index);
            }
        }
    }

    /// <inheritdoc />
    public int CompareTo(BaseStorage? other)
    {
        if (ReferenceEquals(null, other))
        {
            return -1;
        }

        if (ReferenceEquals(this, other) || this.StashToChestPriority == other.StashToChestPriority)
        {
            return 0;
        }

        return -this.StashToChestPriority.CompareTo(other.StashToChestPriority);
    }

    /// <summary>
    ///     Tests if a <see cref="Item" /> matches the <see cref="IStorageData.FilterItemsList" /> condition.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> to test.</param>
    /// <returns>Returns true if the <see cref="Item" /> matches the filters.</returns>
    public bool FilterMatches(Item? item)
    {
        if (item is null)
        {
            return false;
        }

        return !this.FilterItemsList.Any() || this.FilterMatcher.Matches(item);
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
        var oldId = Game1.activeClickableMenu.currentlySnappedComponent?.myID ?? -1;
        this.ShowMenu();
        ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = tmp;
        if (oldId == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldId);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
    }

    /// <summary>
    ///     Grab an item from this storage container.
    /// </summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player grabbing the item.</param>
    public virtual void GrabStorageItem(Item item, Farmer who)
    {
        if (!who.couldInventoryAcceptThisItem(item))
        {
            return;
        }

        this.Items.Remove(item);
        this.ClearNulls();
        this.ShowMenu();
    }

    /// <summary>
    ///     Organizes items in a storage.
    /// </summary>
    /// <param name="descending">Sort in descending order.</param>
    public void OrganizeItems(bool descending = false)
    {
        if (this.OrganizeChestGroupBy == GroupBy.Default && this.OrganizeChestSortBy == SortBy.Default)
        {
            ItemGrabMenu.organizeItemsInList(this.Items);
            return;
        }

        var items = this.Items.ToArray();
        Array.Sort(
            items,
            (i1, i2) =>
            {
                if (ReferenceEquals(i2, null))
                {
                    return -1;
                }

                if (ReferenceEquals(i1, null))
                {
                    return 1;
                }

                if (ReferenceEquals(i1, i2))
                {
                    return 0;
                }

                var g1 = this.OrganizeChestGroupBy switch
                {
                    GroupBy.Category => i1.GetContextTagsExt().FirstOrDefault(tag => tag.StartsWith("category_"))
                                     ?? string.Empty,
                    GroupBy.Color => i1.GetContextTagsExt().FirstOrDefault(tag => tag.StartsWith("color_"))
                                  ?? string.Empty,
                    GroupBy.Name => i1.DisplayName,
                    GroupBy.Default or _ => string.Empty,
                };

                var g2 = this.OrganizeChestGroupBy switch
                {
                    GroupBy.Category => i2.GetContextTagsExt().FirstOrDefault(tag => tag.StartsWith("category_"))
                                     ?? string.Empty,
                    GroupBy.Color => i2.GetContextTagsExt().FirstOrDefault(tag => tag.StartsWith("color_"))
                                  ?? string.Empty,
                    GroupBy.Name => i2.DisplayName,
                    GroupBy.Default or _ => string.Empty,
                };

                if (!g1.Equals(g2))
                {
                    return string.Compare(g1, g2, StringComparison.OrdinalIgnoreCase);
                }

                var o1 = this.OrganizeChestSortBy switch
                {
                    SortBy.Quality when i1 is SObject obj => obj.Quality,
                    SortBy.Quantity => i1.Stack,
                    SortBy.Type => i1.Category,
                    SortBy.Default or _ => 0,
                };

                var o2 = this.OrganizeChestSortBy switch
                {
                    SortBy.Quality when i2 is SObject obj => obj.Quality,
                    SortBy.Quantity => i2.Stack,
                    SortBy.Type => i2.Category,
                    SortBy.Default or _ => 0,
                };

                return o1.CompareTo(o2);
            });

        if (descending)
        {
            Array.Reverse(items);
        }

        this.Items.Clear();
        foreach (var item in items)
        {
            this.Items.Add(item);
        }
    }

    /// <summary>
    ///     Creates an <see cref="ItemGrabMenu" /> for this storage container.
    /// </summary>
    public virtual void ShowMenu()
    {
        var menu = new ItemGrabMenu(
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

        if (Game1.options.SnappyMenus
         && Game1.activeClickableMenu is ItemGrabMenu { currentlySnappedComponent: { } currentlySnappedComponent })
        {
            menu.setCurrentlySnappedComponentTo(currentlySnappedComponent.myID);
            menu.snapCursorToCurrentSnappedComponent();
        }

        Game1.activeClickableMenu = menu;
    }

    /// <summary>
    ///     Stashes an item into storage based on categorization and stack settings.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <param name="existingStacks">Whether to stash into stackable items or based on categorization.</param>
    /// <returns>Returns the <see cref="Item" /> if not all could be stashed or null if successful.</returns>
    public Item? StashItem(Item item, bool existingStacks = false)
    {
        // Disallow stashing of any Chest.
        if (item is Chest or SObject { heldObject.Value: Chest })
        {
            return item;
        }

        var stack = item.Stack;
        var tmp = (existingStacks && this.Items.Any(otherItem => otherItem?.canStackWith(item) == true))
               || (this.FilterItemsList.Any()
                && !this.FilterItemsList.All(filter => filter.StartsWith("!"))
                && this.FilterMatches(item))
            ? this.AddItem(item)
            : item;
        if (tmp is null || stack != item.Stack)
        {
            Log.Trace(
                $"StashItem: {{ Item: {item.Name}, Quantity: {Math.Max(1, stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, To: {this}");
        }

        return tmp;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("{ ");
        switch (this)
        {
            case ChestStorage:
                sb.Append("Type: Chest");
                break;
            case FridgeStorage:
                sb.Append("Type: Fridge");
                break;
            case JunimoHutStorage:
                sb.Append("Type: JunimoHut");
                break;
            case ObjectStorage:
                sb.Append("Type: Object");
                break;
            case ShippingBinStorage:
                sb.Append("Type: ShippingBin");
                break;
        }

        if (!string.IsNullOrWhiteSpace(this.ChestLabel))
        {
            sb.Append($", Name: {this.ChestLabel}");
        }

        sb.Append($", Location: {this.Location.Name}");
        if (!this.Position.Equals(Vector2.Zero))
        {
            sb.Append($", Position: ({this.Position.X.ToString(CultureInfo.InvariantCulture)}");
            sb.Append($", {this.Position.Y.ToString(CultureInfo.InvariantCulture)})");
        }

        if (this.Source is Farmer farmer)
        {
            sb.Append($", Inventory: {farmer.Name}");
        }

        sb.Append(" }");
        return sb.ToString();
    }

    private void OnCollectionChanged(object? source, NotifyCollectionChangedEventArgs? e)
    {
        this.Data.FilterItemsList = new(this._filterMatcher);
    }
}