namespace FuryCore.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Models;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class ItemsDisplayedEventArgs : EventArgs
{
    private int _offset;
    private bool _refreshInventory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsDisplayedEventArgs"/> class.
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="menu"></param>
    public ItemsDisplayedEventArgs(IEnumerable<Item> inventory, ItemGrabMenu menu)
    {
        this.Inventory = inventory.ToList();
        this.Menu = menu;
        this.Columns = this.Menu.ItemsToGrabMenu.capacity / this.Menu.ItemsToGrabMenu.rows;
    }

    public List<Item> Inventory { get; }

    public ItemGrabMenu Menu { get; }

    /// <summary>
    ///     Gets or sets the number of slots the currently displayed items are offset by.
    /// </summary>
    public int Offset
    {
        get
        {
            this.Range.Maximum = Math.Max(0, (this.DisplayedItems.Count - this.Menu.ItemsToGrabMenu.capacity).RoundUp(this.Columns) / this.Columns);
            return this.Range.Clamp(this._offset);
        }

        set
        {
            this.Range.Maximum = Math.Max(0, (this.DisplayedItems.Count - this.Menu.ItemsToGrabMenu.capacity).RoundUp(this.Columns) / this.Columns);
            this._offset = this.Range.Clamp(value);
            this._refreshInventory = true;
        }
    }

    public IEnumerable<Item> Items
    {
        get
        {
            if (this.DisplayedItems is null)
            {
                this.DisplayedItems = this.Inventory.Where(this.FilterMethod).ToList();
                this.Indexes = this.DisplayedItems.Select(item => this.Inventory.IndexOf(item)).ToList();
            }

            if (this._refreshInventory)
            {
                foreach (var (slot, index) in this.Menu.ItemsToGrabMenu.inventory.Select((slot, index) => (slot, index + (this.Offset * this.Columns))))
                {
                    slot.name = (index < this.Indexes.Count ? this.Indexes[index] : int.MaxValue).ToString();
                }

                this._refreshInventory = false;
            }

            return this.DisplayedItems.Skip(this.Offset * this.Columns);
        }
    }

    private int Columns { get; }

    private IList<Item> DisplayedItems { get; set; }

    private IList<int> Indexes { get; set; }

    private IDictionary<Item, bool> ItemFilterCache { get; } = new Dictionary<Item, bool>();

    private IList<Func<Item, bool>> ItemFilters { get; } = new List<Func<Item, bool>>();

    private Range<int> Range { get; } = new();

    public void AddFilter(Func<Item, bool> filter)
    {
        this.ItemFilters.Add(filter);
        this.ForceRefresh();
    }

    /// <summary>
    ///     Forces displayed inventory to refresh.
    /// </summary>
    public void ForceRefresh()
    {
        this.ItemFilterCache.Clear();
        this.DisplayedItems = null;
        this._refreshInventory = true;
    }

    private bool FilterMethod(Item item)
    {
        if (!this.ItemFilterCache.TryGetValue(item, out var filtered))
        {
            filtered = this.ItemFilters.All(itemHighlighter => itemHighlighter.Invoke(item));
            this.ItemFilterCache.Add(item, filtered);
        }

        return filtered;
    }
}