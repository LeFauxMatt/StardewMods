namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IInventoryMenuManager" />
internal sealed class InventoryMenuManager : BaseService, IInventoryMenuManager
{
    private readonly HashSet<InventoryMenu.highlightThisItem> highlightMethods = [];
    private readonly HashSet<Func<IEnumerable<Item>, IEnumerable<Item>>> operations = [];
    private readonly WeakReference<InventoryMenu?> source = new(null);
    private readonly ClickableTextureComponent upArrow;
    private readonly ClickableTextureComponent downArrow;
    private List<Item>? cachedItems;

    /// <summary>Initializes a new instance of the <see cref="InventoryMenuManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    public InventoryMenuManager(ILog log)
        : base(log)
    {
        this.upArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = 5318009 };

        this.downArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = 5318009 };
    }

    /// <summary>Gets or sets the method used to highlight an item in the inventory menu.</summary>
    public InventoryMenu.highlightThisItem OriginalHighlightMethod { get; set; } = InventoryMenu.highlightAllItems;

    /// <inheritdoc />
    public InventoryMenu? Menu => this.source.TryGetTarget(out var target) ? target : null;

    /// <inheritdoc />
    public IContainer? Container { get; set; }

    /// <inheritdoc />
    public int Capacity => this.Menu?.capacity switch { null => 36, > 70 => 70, _ => this.Menu.capacity };

    /// <inheritdoc />
    public int Rows => this.Menu?.rows ?? 3;

    /// <inheritdoc />
    public int Columns => this.Capacity / this.Rows;

    /// <inheritdoc />
    public int Scrolled { get; set; }

    /// <inheritdoc />
    public void AddHighlightMethod(InventoryMenu.highlightThisItem highlightMethod) =>
        this.highlightMethods.Add(highlightMethod);

    /// <inheritdoc />
    public void AddOperation(Func<IEnumerable<Item>, IEnumerable<Item>> operation) => this.operations.Add(operation);

    /// <summary>
    /// Applies a series of operations to a collection of items and returns a modified subset of items based on
    /// specified criteria.
    /// </summary>
    /// <param name="items">The collection of items to apply the operations to.</param>
    /// <returns>The modified subset of items based on the applied operations and specified criteria.</returns>
    public IEnumerable<Item> ApplyOperation(IEnumerable<Item> items)
    {
        // Apply added operations
        this.cachedItems = this.operations.Aggregate(items, (current, operation) => operation(current)).ToList();

        // Validate the scrolled value
        var totalRows = (int)Math.Ceiling((double)this.cachedItems.Count / this.Columns);
        var maxScroll = Math.Max(0, totalRows - this.Rows);
        this.Scrolled = Math.Max(0, Math.Min(this.Scrolled, maxScroll));
        return this.cachedItems.Skip(this.Scrolled * this.Columns).Take(this.Capacity);
    }

    /// <summary>Draws overlay components to the SpriteBatch.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the game object.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (this.Scrolled > 0)
        {
            this.upArrow.draw(spriteBatch);
        }

        if (this.cachedItems is not null)
        {
            var totalRows = (int)Math.Ceiling((double)this.cachedItems.Count / this.Columns);
            var maxScroll = Math.Max(0, totalRows - this.Rows);
            if (this.Scrolled < maxScroll)
            {
                this.downArrow.draw(spriteBatch);
            }
        }
    }

    /// <summary>Performs a hover action at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse click.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse click.</param>
    public void Hover(int mouseX, int mouseY)
    {
        this.upArrow.scale = this.upArrow.containsPoint(mouseX, mouseY)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.upArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.upArrow.scale - 0.05f);

        this.downArrow.scale = this.downArrow.containsPoint(mouseX, mouseY)
            ? Math.Min(Game1.pixelZoom * 1.1f, this.downArrow.scale + 0.05f)
            : Math.Max(Game1.pixelZoom, this.downArrow.scale - 0.05f);
    }

    /// <summary>Performs a left click at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse.</param>
    /// <returns>Returns true if an overlay item was clicked; otherwise, false.</returns>
    public bool LeftClick(int mouseX, int mouseY)
    {
        if (this.upArrow.containsPoint(mouseX, mouseY))
        {
            this.Scrolled--;
            return true;
        }

        if (this.downArrow.containsPoint(mouseX, mouseY))
        {
            this.Scrolled++;
            return true;
        }

        return false;
    }

    /// <summary>Resets the state of the menu, clearing all operations, cached items, and setting new arrow positions and neighbor IDs.</summary>
    /// <param name="parent">The parent ItemGrabMenu, if any.</param>
    /// <param name="current">The current InventoryMenu, if any.</param>
    public void Reset(ItemGrabMenu? parent, InventoryMenu? current)
    {
        this.source.SetTarget(current);
        this.highlightMethods.Clear();
        this.operations.Clear();
        this.cachedItems = null;

        if (parent is null || current is null)
        {
            this.Scrolled = 0;
            this.Container = null;
            return;
        }

        // Add arrows
        parent.allClickableComponents.Add(this.upArrow);
        parent.allClickableComponents.Add(this.downArrow);

        // Reposition arrows
        var topSlot = this.Columns - 1;
        var bottomSlot = this.Capacity - 1;
        this.upArrow.bounds.X = current.xPositionOnScreen + current.width + 8;
        this.upArrow.bounds.Y = current.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
        this.downArrow.bounds.X = current.xPositionOnScreen + current.width + 8;
        this.downArrow.bounds.Y = current.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

        // Assign Neighbor Ids
        this.upArrow.leftNeighborID = current.inventory[topSlot].myID;
        current.inventory[topSlot].rightNeighborID = this.upArrow.myID;
        this.downArrow.leftNeighborID = current.inventory[bottomSlot].myID;
        current.inventory[bottomSlot].rightNeighborID = this.downArrow.myID;
        this.upArrow.downNeighborID = this.downArrow.myID;
        this.downArrow.upNeighborID = this.upArrow.myID;
    }

    /// <summary>Highlights an item using the provided highlight methods.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>Returns true if the item is successfully highlighted, false otherwise.</returns>
    public bool HighlightMethod(Item item) =>
        this.OriginalHighlightMethod(item)
        && (!this.highlightMethods.Any() || this.highlightMethods.All(highlightMethod => highlightMethod(item)));
}