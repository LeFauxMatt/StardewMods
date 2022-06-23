namespace StardewMods.BetterChests.Models;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Storages;
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
        if (obj is BaseStorage storage)
        {
            storage.ClearNulls();
        }
    }

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who)
    {
        return (obj as BaseStorage)?.ActualCapacity ?? Chest.capacity;
    }

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who)
    {
        return (obj as BaseStorage)?.Items ?? default;
    }

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who)
    {
        return null;
    }

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who)
    {
        return obj switch
        {
            ChestStorage { Chest: { } chest } => chest.GetMutex(),
            _ => default,
        };
    }

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who)
    {
        // Location storage position
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item)
    {
        return (obj as BaseStorage)?.FilterMatches(item) ?? false;
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