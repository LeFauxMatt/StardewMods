namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class HeldStorage : IInventoryProvider
{
    /// <inheritdoc/>
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc/>
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc/>
    public void CleanInventory(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is BaseStorage storage)
        {
            storage.ClearNulls();
        }
    }

    /// <inheritdoc/>
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who)
    {
        return obj is IStorageData { ResizeChestCapacity: not 0 } storage
            ? storage.ResizeChestCapacity switch
            {
                < 0 => int.MaxValue,
                > 0 => storage.ResizeChestCapacity,
                0 => Chest.capacity,
            }
            : Chest.capacity;
    }

    /// <inheritdoc/>
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who)
    {
        return obj is IStorageData storage ? storage.Items : default;
    }

    /// <inheritdoc/>
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who)
    {
        return null;
    }

    /// <inheritdoc/>
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who)
    {
        return obj switch
        {
            ChestStorage { Chest: { } chest } => chest.GetMutex(),
            _ => default,
        };
    }

    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item)
    {
        if (obj is BaseStorage storage)
        {
            
        }

        return obj is KeyValuePair<IGameObjectType, IManagedStorage> pair && pair.Value.ItemMatcher.Matches(item);
    }

    /// <inheritdoc />
    public bool IsMutexRequired(object obj, GameLocation? location, Farmer? who)
    {
        return obj is BaseStorage;
    }

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who)
    {
        return obj is BaseStorage;
    }
}