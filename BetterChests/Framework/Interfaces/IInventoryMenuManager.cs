namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewValley.Menus;

/// <summary>Manages the inventory menu by adding, removing, and filtering item filters.</summary>
internal interface IInventoryMenuManager
{
    /// <summary>Gets the capacity of the inventory menu.</summary>
    public int Capacity { get; }

    /// <summary>Gets the number of columns of the inventory menu.</summary>
    public int Columns { get; }

    /// <summary>Gets the number of rows of the inventory menu.</summary>
    public int Rows { get; }

    /// <summary>Gets the container associated with the inventory menu.</summary>
    public IContainer? Context { get; }

    /// <summary>Gets or sets the number of rows that the inventory menu will be scrolled by.</summary>
    public int Scrolled { get; set; }

    /// <summary>Adds a filter method to the inventory menu.</summary>
    /// <param name="highlightMethod">The filter method to add.</param>
    public void AddHighlightMethod(InventoryMenu.highlightThisItem highlightMethod);

    /// <summary>Adds the specified operation.</summary>
    /// <param name="operation">The operation to register.</param>
    public void AddOperation(Func<IEnumerable<Item>, IEnumerable<Item>> operation);
}
