namespace StardewMods.BetterChests.UI;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Extensions;
using StardewMods.Common.Helpers;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Represents an Inventory Menu.
/// </summary>
internal class DisplayedItems
{
    private ClickableTextureComponent? _downArrow;
    private EventHandler? _itemsRefreshed;
    private int _offset;
    private ClickableTextureComponent? _upArrow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisplayedItems" /> class.
    /// </summary>
    /// <param name="menu">The <see cref="InventoryMenu" /> to attach to.</param>
    public DisplayedItems(InventoryMenu menu)
    {
        this.Menu = menu;
        this.Items = this.Menu.actualInventory
                         .Take(menu.capacity)
                         .ToList();
        this.Columns = this.Menu.capacity / this.Menu.rows;
        this.HighlightMethod = this.Menu.highlightMethod;
        this.Menu.highlightMethod = this.Highlight;

        // Reposition Arrows
        var topSlot = this.Columns - 1;
        var bottomSlot = this.Menu.capacity - 1;
        this.UpArrow.bounds.X = this.Menu.xPositionOnScreen + this.Menu.width + 8;
        this.UpArrow.bounds.Y = this.Menu.inventory[topSlot].bounds.Center.Y - 6 * Game1.pixelZoom;
        this.DownArrow.bounds.X = this.UpArrow.bounds.X;
        this.DownArrow.bounds.Y = this.Menu.inventory[bottomSlot].bounds.Center.Y - 6 * Game1.pixelZoom;

        // Assign Neighbor Ids
        this.UpArrow.leftNeighborID = this.Menu.inventory[topSlot].myID;
        this.Menu.inventory[topSlot].rightNeighborID = this.UpArrow.myID;
        this.DownArrow.leftNeighborID = this.Menu.inventory[bottomSlot].myID;
        this.Menu.inventory[bottomSlot].rightNeighborID = this.DownArrow.myID;
        this.UpArrow.downNeighborID = this.DownArrow.myID;
        this.DownArrow.upNeighborID = this.UpArrow.myID;
    }

    /// <summary>
    ///     Raised after the displayed items is refreshed.
    /// </summary>
    public event EventHandler ItemsRefreshed
    {
        add => this._itemsRefreshed += value;
        remove => this._itemsRefreshed -= value;
    }

    /// <summary>
    ///     Gets the items displayed in the inventory menu.
    /// </summary>
    public IList<Item> Items { get; private set; }

    /// <summary>
    ///     Gets the inventory menu.
    /// </summary>
    public InventoryMenu Menu { get; }

    /// <summary>
    ///     Gets or sets the current offset of items in the menu.
    /// </summary>
    public int Offset
    {
        get => this._offset;
        set
        {
            if (value < 0 || value * this.Columns + this.Menu.capacity > this.ActualInventory.Count.RoundUp(12))
            {
                return;
            }

            this._offset = value;
            this.RefreshItems();
        }
    }

    private IList<Item> ActualInventory
    {
        get => this.Filters.Any()
            ? this.Menu.actualInventory.Where(item => this.Filters.All(matcher => matcher.Matches(item))).ToList()
            : this.Menu.actualInventory;
    }

    private int Columns { get; }

    private ClickableTextureComponent DownArrow
    {
        get => this._downArrow ??= new(
            new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(421, 472, 11, 12),
            Game1.pixelZoom)
        {
            myID = 5318008,
        };
    }

    private List<ItemMatcher> Filters { get; } = new();

    private List<ItemMatcher> Highlighters { get; } = new();

    private InventoryMenu.highlightThisItem HighlightMethod { get; }

    private List<Func<IEnumerable<Item>, IEnumerable<Item>>> Transformers { get; } = new();

    private ClickableTextureComponent UpArrow
    {
        get => this._upArrow ??= new(
            new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(421, 459, 11, 12),
            Game1.pixelZoom)
        {
            myID = 5318009,
        };
    }

    /// <summary>
    ///     Adds a <see cref="ItemMatcher" /> to filter inventory.
    /// </summary>
    /// <param name="matcher">The <see cref="ItemMatcher" /> to add.</param>
    public void AddFilter(ItemMatcher matcher)
    {
        this.Filters.Add(matcher);
        matcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Adds a <see cref="ItemMatcher" /> to highlight inventory.
    /// </summary>
    /// <param name="matcher">The <see cref="ItemMatcher" /> to add.</param>
    public void AddHighlighter(ItemMatcher matcher)
    {
        this.Highlighters.Add(matcher);
        matcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Adds a function to transform the list of displayed items.
    /// </summary>
    /// <param name="transformer">The function to add.</param>
    public void AddTransformer(Func<IEnumerable<Item>, IEnumerable<Item>> transformer)
    {
        this.Transformers.Add(transformer);
    }

    /// <summary>
    ///     Draws UI elements to the screen.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch" /> to draw to.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (this.Offset > 0)
        {
            this.UpArrow.draw(spriteBatch);
        }

        if (this.Offset * this.Columns + this.Menu.capacity < this.ActualInventory.Count.RoundUp(12))
        {
            this.DownArrow.draw(spriteBatch);
        }
    }

    /// <summary>
    ///     Attempt to hover.
    /// </summary>
    /// <param name="x">The x-coord of the mouse.</param>
    /// <param name="y">The y-coord of the mouse.</param>
    public void Hover(int x, int y)
    {
        this.UpArrow.scale = this.UpArrow.containsPoint(x, y)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.UpArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.UpArrow.scale - 0.05f);
        this.DownArrow.scale = this.DownArrow.containsPoint(x, y)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.DownArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.DownArrow.scale - 0.05f);
    }

    /// <summary>
    ///     Attempt to left click.
    /// </summary>
    /// <param name="x">The x-coord of the mouse.</param>
    /// <param name="y">The y-coord of the mouse.</param>
    /// <returns>Returns true if an item was clicked.</returns>
    public bool LeftClick(int x, int y)
    {
        if (this.UpArrow.containsPoint(x, y))
        {
            this.Offset--;
            return true;
        }

        if (this.DownArrow.containsPoint(x, y))
        {
            this.Offset++;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Forces the displayed items to be refreshed.
    /// </summary>
    public void RefreshItems()
    {
        var items = this.ActualInventory.AsEnumerable();
        items = this.Transformers.Aggregate(items, (current, transformer) => transformer(current));
        items = items.Skip(this.Offset * this.Columns)
                     .Take(this.Menu.capacity);
        this.Items = items.ToList();
        for (var index = 0; index < this.Menu.inventory.Count; index++)
        {
            this.Menu.inventory[index].name = (index < this.Items.Count
                ? this.Menu.actualInventory.IndexOf(this.Items[index])
                : int.MaxValue).ToString();
        }

        this.Invoke();
    }

    private bool Highlight(Item item)
    {
        return this.HighlightMethod(item) && (!this.Highlighters.Any() || this.Highlighters.All(matcher => matcher.Matches(item)));
    }

    private void Invoke()
    {
        if (this._itemsRefreshed is null)
        {
            return;
        }

        foreach (var handler in this._itemsRefreshed.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private void OnCollectionChanged(object? source, NotifyCollectionChangedEventArgs? e)
    {
        this.Offset = 0;
        this.RefreshItems();
    }
}