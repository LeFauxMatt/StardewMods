namespace StardewMods.BetterChests.Interfaces;

using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;

/// <inheritdoc cref="StardewMods.BetterChests.Interfaces.IStorageData" />
internal interface IManagedStorage : IStorageContainer, IStorageData
{
    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is uniquely assigned to each type of chest.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the Chest.
    /// </summary>
    public string QualifiedItemId { get; }

    /// <summary>
    ///     Attempts to stack add an item into the storage based on existing items.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be stacked completely, or null if it could.</returns>
    public Item StackItems(Item item);

    /// <summary>
    ///     Attempt to stash an item into the storage based on categorization and existing items.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be stashed completely, or null if it could.</returns>
    public Item StashItem(Item item);
}