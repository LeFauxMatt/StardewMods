namespace StardewMods.BetterChests.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>
///     Represents an Inventory Menu.
/// </summary>
internal sealed class DisplayedItems
{
    private readonly int _columns;

    private readonly Lazy<ClickableTextureComponent> _downArrow = new(
        () => new(
            new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(421, 472, 11, 12),
            Game1.pixelZoom) { myID = 5318008 });

    private readonly List<IItemMatcher> _highlighters = new();
    private readonly InventoryMenu.highlightThisItem _highlightMethod;
    private readonly List<Item> _items = new();
    private readonly bool _topMenu;
    private readonly List<Func<IEnumerable<Item>, IEnumerable<Item>>> _transformers = new();

    private readonly Lazy<ClickableTextureComponent> _upArrow = new(
        () => new(
            new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(421, 459, 11, 12),
            Game1.pixelZoom) { myID = 5318009 });

    private EventHandler<List<Item>>? _itemsRefreshed;
    private int _offset;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DisplayedItems" /> class.
    /// </summary>
    /// <param name="menu">The <see cref="InventoryMenu" /> to attach to.</param>
    /// <param name="topMenu">Indicates if this is the top menu.</param>
    public DisplayedItems(InventoryMenu menu, bool topMenu)
    {
        this.Menu = menu;
        this._topMenu = topMenu;
        this._columns = this.Menu.capacity / this.Menu.rows;
        this._highlightMethod = this.Menu.highlightMethod;

        this.Menu.highlightMethod = this.Highlight;

        // Reposition Arrows
        var topSlot = this._columns - 1;
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

        this.RefreshItems();
    }

    /// <summary>
    ///     Raised after the displayed items is refreshed.
    /// </summary>
    public event EventHandler<List<Item>> ItemsRefreshed
    {
        add => this._itemsRefreshed += value;
        remove => this._itemsRefreshed -= value;
    }

    /// <summary>
    ///     Gets the items displayed in the inventory menu.
    /// </summary>
    public IList<Item> Items => this.ActualInventory.Any() ? this._items : Array.Empty<Item>();

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
            if (value < 0)
            {
                this._offset = 0;
                this.RefreshItems();
                return;
            }

            if (value * this._columns + this.Menu.capacity > this.ActualInventory.Count.RoundUp(12))
            {
                this._offset = (this.ActualInventory.Count.RoundUp(12) - this.Menu.capacity) / this._columns;
                this.RefreshItems();
                return;
            }

            this._offset = value;
            this.RefreshItems();
        }
    }

    private IList<Item> ActualInventory => this.Menu.actualInventory;

    private ClickableTextureComponent DownArrow => this._downArrow.Value;

    private ClickableTextureComponent UpArrow => this._upArrow.Value;

    /// <summary>
    ///     Adds a <see cref="ItemMatcher" /> to highlight inventory.
    /// </summary>
    /// <param name="matcher">The <see cref="ItemMatcher" /> to add.</param>
    public void AddHighlighter(IItemMatcher matcher)
    {
        this._highlighters.Add(matcher);
    }

    /// <summary>
    ///     Adds a function to transform the list of displayed items.
    /// </summary>
    /// <param name="transformer">The function to add.</param>
    public void AddTransformer(Func<IEnumerable<Item>, IEnumerable<Item>> transformer)
    {
        this._transformers.Add(transformer);

        if (this._topMenu)
        {
            BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        }
        else
        {
            BetterItemGrabMenu.RefreshInventory = true;
        }
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

        if (this.Offset * this._columns + this.Menu.capacity < this.ActualInventory.Count.RoundUp(12))
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
            --this.Offset;
            return true;
        }

        if (this.DownArrow.containsPoint(x, y))
        {
            ++this.Offset;
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
        items = this._transformers.Aggregate(items, (current, transformer) => transformer(current)).ToList();
        if (!items.Any())
        {
            this._items.Clear();
            this._items.AddRange(items.Skip(this.Offset * this._columns).Take(this.Menu.capacity));
        }
        else
        {
            do
            {
                this._items.Clear();
                this._items.AddRange(items.Skip(this.Offset * this._columns).Take(this.Menu.capacity));
            }
            while (!this._items.Any() && --this.Offset > 0);
        }

        for (var index = 0; index < this.Menu.inventory.Count; ++index)
        {
            this.Menu.inventory[index].name = (index < this._items.Count
                ? this.Menu.actualInventory.IndexOf(this._items[index])
                : int.MaxValue).ToString();
        }

        this._itemsRefreshed.InvokeAll(this, this._items);
    }

    private bool Highlight(Item item)
    {
        return this._highlightMethod(item)
            && (!this._highlighters.Any() || this._highlighters.All(matcher => matcher.Matches(item)));
    }
}