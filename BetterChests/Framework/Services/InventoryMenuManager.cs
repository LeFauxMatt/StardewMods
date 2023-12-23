namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IInventoryMenuManager" />
internal sealed class InventoryMenuManager : BaseService, IInventoryMenuManager
{
    private readonly HashSet<InventoryMenu.highlightThisItem> highlightMethods = [];
    private readonly HashSet<Func<IEnumerable<Item>, IEnumerable<Item>>> operations = [];
    private readonly WeakReference<InventoryMenu?> source = new(null);

    /// <summary>Initializes a new instance of the <see cref="InventoryMenuManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    public InventoryMenuManager(ILog log)
        : base(log) { }

    /// <summary>Gets or sets the method used to highlight an item in the inventory menu.</summary>
    public InventoryMenu.highlightThisItem OriginalHighlightMethod { get; set; } = InventoryMenu.highlightAllItems;

    /// <inheritdoc />
    public InventoryMenu? Menu
    {
        get => this.source.TryGetTarget(out var target) ? target : null;
        set => this.source.SetTarget(value);
    }

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
        var aggregateItems = this.operations.Aggregate(items, (current, operation) => operation(current)).ToList();

        // Validate the scrolled value
        var totalRows = (int)Math.Ceiling((double)aggregateItems.Count / this.Columns);
        var maxScroll = Math.Max(0, totalRows - this.Rows);

        return aggregateItems.Skip(Math.Min(this.Scrolled, maxScroll) * this.Columns).Take(this.Capacity);
    }

    /// <summary>Resets the state of the object by clearing the lists of highlight methods and operations.</summary>
    public void Reset()
    {
        this.highlightMethods.Clear();
        this.operations.Clear();
    }

    /// <summary>Highlights an item using the provided highlight methods.</summary>
    /// <param name="item">The item to highlight.</param>
    /// <returns>Returns true if the item is successfully highlighted, false otherwise.</returns>
    public bool HighlightMethod(Item item) =>
        this.OriginalHighlightMethod(item)
        && (!this.highlightMethods.Any() || this.highlightMethods.All(highlightMethod => highlightMethod(item)));
}