namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageProvider : IInventoryProvider
{
    /// <inheritdoc />
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc />
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc />
    public void CleanInventory(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is StorageWrapper
            {
                Storage:
                { } storage,
            })
        {
            storage.ClearNulls();
        }
    }

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who)
    {
        return obj is StorageWrapper
        {
            Storage:
            { } storage,
        }
            ? storage.ActualCapacity
            : Chest.capacity;
    }

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who)
    {
        return obj is StorageWrapper
        {
            Storage:
            { } storage,
        }
            ? storage.Items
            : default;
    }

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who)
    {
        return null;
    }

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who)
    {
        return obj is StorageWrapper
        {
            Storage:
            { } storage,
        }
            ? storage.Mutex
            : default;
    }

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who)
    {
        return obj is StorageWrapper
        {
            Storage:
            { } storage,
        }
            ? storage.Position
            : default;
    }

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item)
    {
        return obj is StorageWrapper
               {
                   Storage:
                   { } storage,
               }
            && storage.FilterMatches(item);
    }

    /// <inheritdoc />
    public bool IsMutexRequired(object obj, GameLocation? location, Farmer? who)
    {
        return true;
    }

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who)
    {
        return obj is StorageWrapper;
    }
}