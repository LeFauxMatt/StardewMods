namespace StardewMods.BetterChests.Interfaces;

using System.Collections.Generic;
using StardewMods.FuryCore.Helpers;
using StardewValley;

/// <inheritdoc />
internal interface IManagedStorage : IStorageData
{
    /// <summary>
    ///     Gets the actual capacity of the source object.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    ///     Gets the source object associated with the storage being managed.
    /// </summary>
    public object Context { get; }

    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is uniquely assigned to each type of chest.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    ///     Gets the inventory of the Chest being managed.
    /// </summary>
    public IList<Item> Items { get; }

    /// <summary>
    ///     Gets the ModData associated with the source object.
    /// </summary>
    public ModDataDictionary ModData { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the Chest.
    /// </summary>
    public string QualifiedItemId { get; }

    /// <summary>
    ///     Attempts to add an item into the storage.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be added completely, or null if it could.</returns>
    public Item AddItem(Item item);

    /// <summary>
    ///     Removes null items from the storage.
    /// </summary>
    public void ClearNulls();

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