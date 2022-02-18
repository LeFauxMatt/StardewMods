namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="MenuItemsChanged" /> event.
/// </summary>
public interface IMenuItemsChangedEventArgs
{
    /// <summary>
    ///     Gets the storage object for items being displayed.
    /// </summary>
    public IStorageContainer Context { get; }

    /// <summary>
    ///     Gets the Menu that items are being displayed on.
    /// </summary>
    public ItemGrabMenu Menu { get; }

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
    ///     Sets the method which displayed items will be sorted by.
    /// </summary>
    /// <param name="sortMethod">The sort method.</param>
    public void SetSortMethod(Func<Item, int> sortMethod);
}