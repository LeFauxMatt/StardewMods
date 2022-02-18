namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley.Menus;

/// <inheritdoc />
public class MenuItemsChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItemsChangedEventArgs" /> class.
    /// </summary>
    /// <param name="menu">The menu to add components to.</param>
    /// <param name="context">The storage object for items being displayed.</param>
    /// <param name="itemFilters">The item filters to apply.</param>
    /// <param name="itemHighlighters">The item highlighters to apply.</param>
    /// <param name="itemHighlightCache">The cache of which items meet highlight conditions.</param>
    /// <param name="forceRefresh">A method to reset all item caches.</param>
    public MenuItemsChangedEventArgs(ItemGrabMenu menu, IStorageContainer context, HashSet<ItemMatcher> itemFilters, HashSet<ItemMatcher> itemHighlighters, IDictionary<string, bool> itemHighlightCache, Action forceRefresh)
    {
        this.Menu = menu;
        this.Context = context;
        this.ItemFilters = itemFilters;
        this.ItemHighlightCache = itemHighlightCache;
        this.ItemHighlighters = itemHighlighters;
        this.ForceRefresh = forceRefresh;
    }

    /// <summary>
    ///     Gets the storage object for items being displayed.
    /// </summary>
    public IStorageContainer Context { get; }

    /// <summary>
    ///     Gets the Menu that items are being displayed on.
    /// </summary>
    public ItemGrabMenu Menu { get; }

    private Action ForceRefresh { get; }

    private HashSet<ItemMatcher> ItemFilters { get; }

    private IDictionary<string, bool> ItemHighlightCache { get; }

    private HashSet<ItemMatcher> ItemHighlighters { get; }

    /// <summary>
    ///     Add an item matcher which will filter what items are displayed.
    /// </summary>
    /// <param name="itemMatcher">Items where <see cref="ItemMatcher.Matches" /> returns true will be displayed.</param>
    public void AddFilter(ItemMatcher itemMatcher)
    {
        this.ItemFilters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.OnItemFilterChanged;
    }

    /// <summary>
    ///     Adds an item matcher which will determine what items are highlighted.
    /// </summary>
    /// <param name="itemMatcher">Items where <see cref="ItemMatcher.Matches" /> returns true will be highlighted.</param>
    public void AddHighlighter(ItemMatcher itemMatcher)
    {
        this.ItemHighlighters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.OnItemHighlighterChanged;
    }

    private void OnItemFilterChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.ForceRefresh();
    }

    private void OnItemHighlighterChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.ItemHighlightCache.Clear();
    }
}