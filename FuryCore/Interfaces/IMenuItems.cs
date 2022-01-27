namespace FuryCore.Interfaces;

using System.Collections.Generic;
using FuryCore.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Allows displayed items to be handled separately from actual items.
/// </summary>
public interface IMenuItems
{
    /// <summary>
    ///     Gets the source chest that actual items are associated with.
    /// </summary>
    public Chest Chest { get; }

    /// <summary>
    ///     Gets the current menu that is displaying items.
    /// </summary>
    public ItemGrabMenu Menu { get; }

    /// <summary>
    ///     Gets the actual inventory of the Chest/Menu.
    /// </summary>
    public IList<Item> ActualInventory { get; }

    /// <summary>
    ///     Gets the currently displayed items of the Menu.
    /// </summary>
    public IEnumerable<Item> ItemsDisplayed { get; }

    /// <summary>
    ///     Gets or sets the number of slots the currently displayed items are offset by.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    ///     Gets the total number of rows in the menu.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    ///     Add an item matcher which will filter what items are displayed.
    /// </summary>
    /// <param name="itemMatcher">Items where <see cref="ItemMatcher.Matches" /> returns true will be displayed.</param>
    public void AddFilter(ItemMatcher itemMatcher);

    /// <summary>
    ///     Adds an item matcher which will determine what items are highlighted.
    /// </summary>
    /// <param name="itemMatcher">Items where <see cref="ItemMatcher.Matches" /> returns true will be highlighted.</param>
    public void AddHighlighter(ItemMatcher itemMatcher);

    /// <summary>
    ///     Clears internal cache of filtered/highlighted items.
    /// </summary>
    public void ForceRefresh();
}