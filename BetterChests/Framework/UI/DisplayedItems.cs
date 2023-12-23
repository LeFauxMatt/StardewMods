namespace StardewMods.BetterChests.Framework.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Extensions;
using StardewValley.Menus;

/// <summary>Represents an Inventory Menu.</summary>
internal sealed class DisplayedItems
{
    private readonly int columns;

    private readonly Lazy<ClickableTextureComponent> downArrow = new(
        () => new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = 5318008 });

    private readonly List<ItemMatcher> highlighters = new();
    private readonly InventoryMenu.highlightThisItem highlightMethod;
    private readonly List<Item> items = new();
    private readonly bool topMenu;
    private readonly List<Func<IEnumerable<Item>, IEnumerable<Item>>> transformers = new();

    private readonly Lazy<ClickableTextureComponent> upArrow = new(
        () => new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = 5318009 });

    private EventHandler<List<Item>>? itemsRefreshed;
    private int offset;

    /// <summary>Initializes a new instance of the <see cref="DisplayedItems" /> class.</summary>
    /// <param name="menu">The <see cref="InventoryMenu" /> to attach to.</param>
    /// <param name="topMenu">Indicates if this is the top menu.</param>
    public DisplayedItems(InventoryMenu menu, bool topMenu)
    {
        this.Menu = menu;
        this.topMenu = topMenu;
        this.columns = this.Menu.capacity / this.Menu.rows;
        this.highlightMethod = this.Menu.highlightMethod;

        this.Menu.highlightMethod = this.Highlight;

        // Reposition Arrows
        var topSlot = this.columns - 1;
        var bottomSlot = this.Menu.capacity - 1;
        this.UpArrow.bounds.X = this.Menu.xPositionOnScreen + this.Menu.width + 8;
        this.UpArrow.bounds.Y = this.Menu.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
        this.DownArrow.bounds.X = this.UpArrow.bounds.X;
        this.DownArrow.bounds.Y = this.Menu.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

        // Assign Neighbor Ids
        this.UpArrow.leftNeighborID = this.Menu.inventory[topSlot].myID;
        this.Menu.inventory[topSlot].rightNeighborID = this.UpArrow.myID;
        this.DownArrow.leftNeighborID = this.Menu.inventory[bottomSlot].myID;
        this.Menu.inventory[bottomSlot].rightNeighborID = this.DownArrow.myID;
        this.UpArrow.downNeighborID = this.DownArrow.myID;
        this.DownArrow.upNeighborID = this.UpArrow.myID;

        this.RefreshItems();
    }

    /// <summary>Gets the items displayed in the inventory menu.</summary>
    public IList<Item> Items => this.ActualInventory.Any() ? this.items : Array.Empty<Item>();

    /// <summary>Gets the inventory menu.</summary>
    public InventoryMenu Menu { get; }

    /// <summary>Gets or sets the current offset of items in the menu.</summary>
    public int Offset
    {
        get => this.offset;
        set
        {
            if (value < 0)
            {
                this.offset = 0;
                this.RefreshItems();
                return;
            }

            if ((value * this.columns) + this.Menu.capacity > this.ActualInventory.Count.RoundUp(12))
            {
                this.offset = (this.ActualInventory.Count.RoundUp(12) - this.Menu.capacity) / this.columns;
                this.RefreshItems();
                return;
            }

            this.offset = value;
            this.RefreshItems();
        }
    }

    private IList<Item> ActualInventory => this.Menu.actualInventory;

    private ClickableTextureComponent DownArrow => this.downArrow.Value;

    private ClickableTextureComponent UpArrow => this.upArrow.Value;

    /// <summary>Raised after the displayed items is refreshed.</summary>
    public event EventHandler<List<Item>> ItemsRefreshed
    {
        add => this.itemsRefreshed += value;
        remove => this.itemsRefreshed -= value;
    }

    /// <summary>Adds a <see cref="ItemMatcher" /> to highlight inventory.</summary>
    /// <param name="matcher">The <see cref="ItemMatcher" /> to add.</param>
    public void AddHighlighter(ItemMatcher matcher) => this.highlighters.Add(matcher);

    /// <summary>Draws UI elements to the screen.</summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch" /> to draw to.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (this.Offset > 0)
        {
            this.UpArrow.draw(spriteBatch);
        }

        if ((this.Offset * this.columns) + this.Menu.capacity < this.ActualInventory.Count.RoundUp(12))
        {
            this.DownArrow.draw(spriteBatch);
        }
    }

    /// <summary>Attempt to hover.</summary>
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

    /// <summary>Attempt to left click.</summary>
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

    /// <summary>Forces the displayed items to be refreshed.</summary>
    public void RefreshItems()
    {
        var actualInventory = this.ActualInventory.AsEnumerable();
        actualInventory =
            this.transformers.Aggregate(actualInventory, (current, transformer) => transformer(current)).ToList();

        if (!actualInventory.Any())
        {
            this.items.Clear();
            this.items.AddRange(actualInventory.Skip(this.Offset * this.columns).Take(this.Menu.capacity));
        }
        else
        {
            do
            {
                this.items.Clear();
                this.items.AddRange(actualInventory.Skip(this.Offset * this.columns).Take(this.Menu.capacity));
            } while (!this.items.Any() && --this.Offset > 0);
        }

        for (var index = 0; index < this.Menu.inventory.Count; ++index)
        {
            this.Menu.inventory[index].name =
                (index < this.items.Count ? this.Menu.actualInventory.IndexOf(this.items[index]) : int.MaxValue)
                .ToString(CultureInfo.InvariantCulture);
        }

        this.itemsRefreshed.InvokeAll(this, this.items);
    }

    private bool Highlight(Item item) =>
        this.highlightMethod(item)
        && (!this.highlighters.Any() || this.highlighters.All(matcher => matcher.MatchesFilter(item)));
}