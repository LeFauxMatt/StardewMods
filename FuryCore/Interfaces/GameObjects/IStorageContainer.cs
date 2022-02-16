namespace StardewMods.FuryCore.Interfaces.GameObjects;

using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;

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

    /// <summary>
    ///     Grabs an item from a player into this storage container.
    /// </summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player whose inventory to grab the item from.</param>
    public void GrabInventoryItem(Item item, Farmer who);

    /// <summary>
    ///     Grab an item from this storage container.
    /// </summary>
    /// <param name="item">The item to grab.</param>
    /// <param name="who">The player grabbing the item.</param>
    public void GrabStorageItem(Item item, Farmer who);

    /// <summary>
    ///     Creates an <see cref="ItemGrabMenu" /> for this storage container.
    /// </summary>
    public void ShowMenu();
}