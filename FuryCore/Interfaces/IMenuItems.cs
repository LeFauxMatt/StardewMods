namespace StardewMods.FuryCore.Interfaces;

using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Allows displayed items to be handled separately from actual items.
/// </summary>
public interface IMenuItems
{
    /// <summary>
    ///     Gets the actual inventory of the Chest/Menu.
    /// </summary>
    public IList<Item> ActualInventory { get; }

    /// <summary>
    ///     Gets the source storage caontiner that actual items are associated with.
    /// </summary>
    public IStorageContainer Context { get; }

    /// <summary>
    ///     Gets the currently displayed items of the Menu.
    /// </summary>
    public IEnumerable<Item> ItemsDisplayed { get; }

    /// <summary>
    ///     Gets the current menu that is displaying items.
    /// </summary>
    public ItemGrabMenu Menu { get; }

    /// <summary>
    ///     Gets or sets the number of slots the currently displayed items are offset by.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    ///     Gets the total number of rows in the menu.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    ///     Clears internal cache of filtered/highlighted items.
    /// </summary>
    public void ForceRefresh();
}