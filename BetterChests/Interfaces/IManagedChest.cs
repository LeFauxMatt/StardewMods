namespace StardewMods.BetterChests.Interfaces;

using System.Collections.Generic;
using StardewMods.FuryCore.Helpers;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal interface IManagedChest : IChestData
{
    /// <summary>
    ///     Gets the actual instance of the <see cref="Chest" /> being managed.
    /// </summary>
    public Chest Chest { get; }

    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is uniquely assigned to each type of chest.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    ///     Gets the inventory of the Chest being managed.
    /// </summary>
    public IList<Item> Items { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the Chest.
    /// </summary>
    public string QualifiedItemId { get; }

    /// <summary>
    ///     Attempt to stash an item into the chest based on categorization and existing items.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be stashed completely, or null if it could.</returns>
    public Item StashItem(Item item);
}