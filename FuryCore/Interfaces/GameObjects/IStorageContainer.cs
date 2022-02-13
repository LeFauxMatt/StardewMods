namespace StardewMods.FuryCore.Interfaces.GameObjects;

using System.Collections.Generic;
using StardewValley;

/// <summary>
///     Represents a game object that can store items.
/// </summary>
public interface IStorageContainer : IGameObject
{
    /// <summary>
    ///     Gets the actual capacity of the object's storage.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    ///     Gets the items in the object's storage.
    /// </summary>
    IList<Item> Items { get; }

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
}