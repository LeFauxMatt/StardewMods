#nullable disable

namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.CustomEvents.IMenuItemsChangedEventArgs" />
internal class MenuItemsChangedEventArgs : EventArgs, IMenuItemsChangedEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItemsChangedEventArgs" /> class.
    /// </summary>
    /// <param name="menu">The menu to add components to.</param>
    /// <param name="context">The storage object for items being displayed.</param>
    /// <param name="menuItems">The <see cref="MenuItems" /> instance.</param>
    public MenuItemsChangedEventArgs(ItemGrabMenu menu, IStorageContainer context, MenuItems menuItems)
    {
        this.Menu = menu;
        this.Context = context;
        this.MenuItems = menuItems;
    }

    /// <inheritdoc />
    public IStorageContainer Context { get; }

    /// <inheritdoc />
    public ItemGrabMenu Menu { get; }

    private MenuItems MenuItems { get; }

    /// <inheritdoc />
    public void AddFilter(ItemMatcher itemMatcher)
    {
        this.MenuItems.ItemFilters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.MenuItems.OnItemFilterChanged;
    }

    /// <inheritdoc />
    public void AddHighlighter(ItemMatcher itemMatcher)
    {
        this.MenuItems.ItemHighlighters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.MenuItems.OnItemHighlighterChanged;
    }

    /// <inheritdoc />
    public void SetSortMethod(Func<Item, int> sortMethod)
    {
        this.MenuItems.SortMethod = sortMethod;
    }
}