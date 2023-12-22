namespace StardewMods.BetterChests.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <summary>An instance of a game object that can store items.</summary>
internal interface IContainer : IItemFilter
{
    /// <summary>The key used to store the locked slot information.</summary>
    private const string LockedSlotKey = "furyx639.BetterChests/LockedSlot";

    /// <summary>Gets options for the type of storage.</summary>
    IStorage StorageType { get; }

    /// <summary>Gets options for the storage instance.</summary>
    IStorage Options { get; }

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
    public void ForEachItem(Func<Item, bool> action)
    {
        for (var index = this.Items.Count - 1; index >= 0; --index)
        {
            if (!action(this.Items[index]))
            {
                break;
            }
        }
    }

    /// <summary>Transfers an item to a different storage.</summary>
    /// <param name="item">The item to transfer.</param>
    /// <param name="containerTo">The storage to transfer the item to.</param>
    /// <param name="remaining">Contains the remaining item after addition, if any.</param>
    /// <returns>Returns true if the transfer was successful; otherwise, false.</returns>
    public bool Transfer(Item item, IContainer containerTo, out Item? remaining)
    {
        if (!this.Items.Contains(item))
        {
            remaining = null;
            return false;
        }

        if (this.Options.SlotLock == FeatureOption.Enabled && item.modData.ContainsKey(IContainer.LockedSlotKey))
        {
            remaining = null;
            return false;
        }

        if (!containerTo.TryAdd(item, out remaining))
        {
            return false;
        }

        if (remaining is null)
        {
            this.Items.Remove(item);
        }

        return true;
    }

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
