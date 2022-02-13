namespace StardewMods.BetterChests.Interfaces.ManagedObjects;

using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;

/// <inheritdoc cref="IStorageContainer" />
internal interface IManagedStorage : IManagedObject, IStorageContainer
{
    /// <summary>
    ///     Attempt to stash an item into the storage based on categorization and existing items.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be stashed completely, or null if it could.</returns>
    public Item StashItem(Item item);
}