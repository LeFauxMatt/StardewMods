namespace FuryCore.Interfaces;

using System.Collections.Generic;
using FuryCore.Helpers;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

public interface IMenuItems
{

    public Chest Chest { get; }

    public ItemGrabMenu Menu { get; }

    public IList<Item> ActualInventory { get; }

    public IEnumerable<Item> ItemsDisplayed { get; }

    /// <summary>
    ///     Gets or sets the number of slots the currently displayed items are offset by.
    /// </summary>
    public int Offset { get; set; }

    public void AddFilter(ItemMatcher itemMatcher);

    public void AddHighlighter(ItemMatcher itemMatcher);

    public void ForceRefresh();
}