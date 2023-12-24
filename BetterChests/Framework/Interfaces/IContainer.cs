namespace StardewMods.BetterChests.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <summary>An instance of a game object that can store items.</summary>
internal interface IContainer : IItemFilter
{
    /// <summary>Gets the name of the storage.</summary>
    string DisplayName { get; }

    /// <summary>Gets the description of the storage.</summary>
    string Description { get; }

    /// <summary>Gets options for the storage instance.</summary>
    IStorageOptions Options { get; }

    /// <summary>Gets the collection of items.</summary>
    IInventory Items { get; }

    /// <summary>Gets the game location of an object.</summary>
    GameLocation Location { get; }

    /// <summary>Gets the tile location of an object.</summary>
    Vector2 TileLocation { get; }

    /// <summary>Gets the mod data dictionary.</summary>
    ModDataDictionary ModData { get; }

    /// <summary>Executes a given action for each item in the collection.</summary>
    /// <param name="action">The action to be executed for each item.</param>
    public void ForEachItem(Func<Item, bool> action);

    /// <summary>Opens an item grab menu for this container.</summary>
    public void ShowMenu();

    /// <summary>Transfers an item to a different storage.</summary>
    /// <param name="item">The item to transfer.</param>
    /// <param name="containerTo">The storage to transfer the item to.</param>
    /// <param name="remaining">Contains the remaining item after addition, if any.</param>
    /// <returns>Returns true if the transfer was successful; otherwise, false.</returns>
    public bool Transfer(Item item, IContainer containerTo, out Item? remaining);

    /// <summary>Tries to add an item to the storage.</summary>
    /// <param name="item">The item to give.</param>
    /// <param name="remaining">When this method returns, contains the remaining item after addition, if any.</param>
    /// <returns>True if the item was successfully given; otherwise, false.</returns>
    public bool TryAdd(Item item, out Item? remaining);
}

/// <inheritdoc />
/// <typeparam name="TSource">The source object type.</typeparam>
internal interface IContainer<TSource> : IContainer
    where TSource : class
{
    /// <summary>Gets a value indicating whether the source object is still alive.</summary>
    public bool IsAlive { get; }

    /// <summary>Gets a weak reference to the source object.</summary>
    public WeakReference<TSource> Source { get; }
}