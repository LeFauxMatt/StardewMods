namespace BetterChests.Models;

using System.Linq;
using BetterChests.Interfaces;
using FuryCore.Helpers;
using StardewValley;
using StardewValley.Objects;

internal record ManagedChest
{
    public ManagedChest(Chest chest, IChestConfigExtended config)
    {
        this.Chest = chest;
        this.Config = config;
        this.ItemMatcher = new(true);

        if (this.Chest.modData.TryGetValue("FilterItems", out var filterItems) && !string.IsNullOrWhiteSpace(filterItems))
        {
            this.ItemMatcher.StringValue = filterItems;
        }
    }

    public Chest Chest { get; }

    public IChestConfigExtended Config { get; }

    public ItemMatcher ItemMatcher { get; }

    public Item StashItem(Item item, bool fillStacks = false)
    {
        var stack = item.Stack;

        if ((this.Config.ItemMatcher.Any() || this.ItemMatcher.Any()) && this.Config.ItemMatcher.Matches(item) && this.ItemMatcher.Matches(item))
        {
            var tmp = this.Chest.addItem(item);
            if (tmp is null || tmp.Stack <= 0)
            {
                return null;
            }

            if (tmp.Stack != stack)
            {
                item.Stack = tmp.Stack;
            }
        }

        if (fillStacks)
        {
            foreach (var chestItem in this.Chest.items.Where(chestItem => chestItem.maximumStackSize() > 1 && chestItem.canStackWith(item)))
            {
                if (chestItem.getRemainingStackSpace() > 0)
                {
                    item.Stack = chestItem.addToStack(item);
                }

                if (item.Stack <= 0)
                {
                    return null;
                }
            }
        }

        return item;
    }
}