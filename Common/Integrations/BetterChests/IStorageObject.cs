namespace StardewMods.Common.Integrations.BetterChests;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;

/// <summary>
///     Provides access to storage data and functions.
/// </summary>
public interface IStorageObject : IStorageData
{
    /// <summary>
    ///     Gets the actual capacity of the object's storage.
    /// </summary>
    public int ActualCapacity { get; }

    /// <summary>
    ///     Gets the context object.
    /// </summary>
    public object Context { get; }

    /// <summary>
    ///     Gets an ItemMatcher to represent the currently selected filters.
    /// </summary>
    public ItemMatcher FilterMatcher { get; }

    /// <summary>
    ///     Gets the items in the object's storage.
    /// </summary>
    public IList<Item?> Items { get; }

    /// <summary>
    ///     Gets the calculated capacity of the <see cref="InventoryMenu" />.
    /// </summary>
    public int MenuCapacity { get; }

    /// <summary>
    ///     Gets the extra vertical space needed for the <see cref="InventoryMenu" /> based on
    ///     <see cref="IStorageData.ResizeChestMenuRows" />.
    /// </summary>
    public int MenuExtraSpace { get; }

    /// <summary>
    ///     Gets the number of rows to display on the <see cref="InventoryMenu" /> based on
    ///     <see cref="IStorageData.ResizeChestMenuRows" />.
    /// </summary>
    public int MenuRows { get; }

    /// <summary>
    ///     Gets the ModData associated with the context object.
    /// </summary>
    public ModDataDictionary ModData { get; }

    /// <summary>
    ///     Gets the mutex required to lock this object.
    /// </summary>
    public NetMutex? Mutex { get; }

    /// <summary>
    ///     Gets the parent context where this storage is contained.
    /// </summary>
    public object? Parent { get; }

    /// <summary>
    ///     Gets the coordinate of this object.
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    ///     Attempts to add an item into the storage.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be added completely, or null if it could.</returns>
    public Item? AddItem(Item item);

    /// <summary>
    ///     Removes null items from the storage.
    /// </summary>
    public void ClearNulls();

    /// <summary>
    ///     Tests if a <see cref="Item" /> matches the <see cref="IStorageData.FilterItemsList" /> condition.
    /// </summary>
    /// <param name="item">The <see cref="Item" /> to test.</param>
    /// <returns>Returns true if the <see cref="Item" /> matches the filters.</returns>
    public bool FilterMatches(Item item);

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
    ///     Gets the item tag that will be used to sort in <see cref="IStorageData.OrganizeChest" />.
    /// </summary>
    /// <param name="item">The <see cref="Item" />.</param>
    /// <returns>Returns the <see cref="Item" /> tag based on the <see cref="IStorageData.OrganizeChestGroupBy" /> option.</returns>
    public string OrderBy(Item item);

    /// <summary>
    ///     Organizes items in a storage.
    /// </summary>
    /// <param name="descending">Sort in descending order.</param>
    public void OrganizeItems(bool descending = false);

    /// <summary>
    ///     Creates an <see cref="ItemGrabMenu" /> for this storage container.
    /// </summary>
    public void ShowMenu();

    /// <summary>
    ///     Stashes an item into storage based on categorization and stack settings.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <param name="existingStacks">Whether to stash into stackable items or based on categorization.</param>
    /// <returns>Returns the <see cref="Item" /> if not all could be stashed or null if successful.</returns>
    public Item? StashItem(Item item, bool existingStacks = false);

    /// <summary>
    ///     Gets the item tag that will be used to sub-sort in <see cref="IStorageData.OrganizeChest" />.
    /// </summary>
    /// <param name="item">The <see cref="Item" />.</param>
    /// <returns>Returns the <see cref="Item" /> tag based on the <see cref="IStorageData.OrganizeChestSortBy" /> option.</returns>
    public int ThenBy(Item item);
}