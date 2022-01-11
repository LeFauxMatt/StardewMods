namespace FuryCore.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
public class ItemsHighlightedEventArgs : EventArgs
{
    public ItemsHighlightedEventArgs(Chest chest)
    {
        this.Chest = chest;
    }

    public Chest Chest { get; }

    private IDictionary<Item, bool> ItemHighlightCache { get; } = new Dictionary<Item, bool>();

    private IList<Func<Item, bool>> ItemHighlighters { get; } = new List<Func<Item, bool>>();

    public void AddHighlighter(Func<Item, bool> highlightItems)
    {
        this.ItemHighlighters.Add(highlightItems);
    }

    public void ForceRefresh()
    {
        this.ItemHighlightCache.Clear();
    }

    public bool HighlightMethod(Item item)
    {
        if (!this.ItemHighlightCache.TryGetValue(item, out var highlight))
        {
            highlight = this.ItemHighlighters.All(itemHighlighter => itemHighlighter.Invoke(item));
            this.ItemHighlightCache.Add(item, highlight);
        }

        return highlight;
    }
}