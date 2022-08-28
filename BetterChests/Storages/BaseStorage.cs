namespace StardewMods.BetterChests.Storages;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="StardewMods.Common.Integrations.BetterChests.IStorageObject" />
internal abstract class BaseStorage : StorageNodeData, IStorageObject
{
    private static readonly IStorageData DefaultStorage = new StorageData();

    private readonly HashSet<string> _cachedFilterList = new();
    private readonly IItemMatcher _filterMatcher = new ItemMatcher(true);
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

    /// <inheritdoc />
    public virtual int ActualCapacity =>
        this.ResizeChestCapacity switch
        {
            < 0 => int.MaxValue,
            > 0 => this.ResizeChestCapacity,
            0 => Chest.capacity,
        };

    /// <inheritdoc />
    public object Context { get; }

    /// <inheritdoc />
    public IItemMatcher FilterMatcher
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

    /// <inheritdoc />
    public abstract IList<Item?> Items { get; }

    /// <inheritdoc />
    public int MenuCapacity => this.MenuRows * 12;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public virtual NetMutex? Mutex => default;

    /// <inheritdoc />
    public Vector2 Position { get; }

    /// <inheritdoc />
    public object? Source { get; }

    /// <inheritdoc />
    public virtual Item? AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(
                     existingItem => existingItem is not null && existingItem.canStackWith(item)))
        {
            item.Stack = existingItem!.addToStack(item);
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool Equals(IStorageObject? x, IStorageObject? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        return x.GetType() == y.GetType() && x.Context.Equals(y.Context);
    }

    /// <inheritdoc />
    public bool FilterMatches(Item? item)
    {
        if (item is null)
        {
            return false;
        }

        return !this.FilterItemsList.Any() || this.FilterMatcher.Matches(item);
    }

    /// <inheritdoc />
    public int GetHashCode(IStorageObject obj)
    {
        return obj.Context.GetHashCode();
    }

    /// <inheritdoc />
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
        var oldId = Game1.activeClickableMenu.currentlySnappedComponent != null
            ? Game1.activeClickableMenu.currentlySnappedComponent.myID
            : -1;
        this.ShowMenu();
        ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = tmp;
        if (oldId == -1)
        {
            return;
        }

        Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldId);
        Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void OrganizeItems(bool descending = false)
    {
        if (this.OrganizeChestGroupBy == GroupBy.Default && this.OrganizeChestSortBy == SortBy.Default)
        {
            ItemGrabMenu.organizeItemsInList(this.Items);
            return;
        }

        var items = this.Items.OfType<Item>()
                        .OrderBy(
                            item => this.OrganizeChestGroupBy switch
                            {
                                GroupBy.Category => item.GetContextTagsExt()
                                                        .FirstOrDefault(tag => tag.StartsWith("category_"))
                                                 ?? string.Empty,
                                GroupBy.Color => item.GetContextTagsExt()
                                                     .FirstOrDefault(tag => tag.StartsWith("color_"))
                                              ?? string.Empty,
                                GroupBy.Name => item.DisplayName,
                                GroupBy.Default or _ => string.Empty,
                            })
                        .ThenBy(
                            item => this.OrganizeChestSortBy switch
                            {
                                SortBy.Quality when item is SObject obj => obj.Quality,
                                SortBy.Quantity => item.Stack,
                                SortBy.Type => item.Category,
                                SortBy.Default or _ => 0,
                            })
                        .ToList();
        if (descending)
        {
            items.Reverse();
        }

        this.Items.Clear();
        foreach (var item in items)
        {
            this.Items.Add(item);
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Item? StashItem(Item item, bool existingStacks = false)
    {
        var condition1 = existingStacks && this.Items.Any(otherItem => otherItem?.canStackWith(item) == true);
        var condition2 = this.FilterItemsList.Any()
                      && !this.FilterItemsList.All(filter => filter.StartsWith("!"))
                      && this.FilterMatches(item);
        var stack = item.Stack;
        var tmp = condition1 || condition2 ? this.AddItem(item) : item;
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

        switch (this.Source)
        {
            case GameLocation location:
                sb.Append($", Location: {location.Name}");
                sb.Append($", Position: ({this.Position.X.ToString(CultureInfo.InvariantCulture)}");
                sb.Append($", {this.Position.Y.ToString(CultureInfo.InvariantCulture)})");
                break;
            case Farmer farmer:
                sb.Append($", Inventory: {farmer.Name}");
                break;
        }

        sb.Append(" }");
        return sb.ToString();
    }

    private void OnCollectionChanged(object? source, NotifyCollectionChangedEventArgs? e)
    {
        this.Data.FilterItemsList = new(this._filterMatcher);
    }
}