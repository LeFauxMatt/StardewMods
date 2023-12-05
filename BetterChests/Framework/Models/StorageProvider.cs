namespace StardewMods.BetterChests.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class StorageProvider : IInventoryProvider
{
    /// <inheritdoc />
    public bool CanExtractItems(object obj, GameLocation? location, Farmer? who) => true;

    /// <inheritdoc />
    public bool CanInsertItems(object obj, GameLocation? location, Farmer? who) => true;

    /// <inheritdoc />
    public void CleanInventory(object obj, GameLocation? location, Farmer? who)
    {
        if (obj is StorageNode
            {
                Data: Storage storageObject,
            })
        {
            storageObject.ClearNulls();
        }
    }

    /// <inheritdoc />
    public int GetActualCapacity(object obj, GameLocation? location, Farmer? who) =>
        obj is StorageNode
        {
            Data: Storage storageObject,
        }
            ? storageObject.ActualCapacity
            : Chest.capacity;

    /// <inheritdoc />
    public IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who) =>
        obj is StorageNode
        {
            Data: Storage storageObject,
        }
            ? storageObject.Inventory
            : default;

    /// <inheritdoc />
    public Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who) => null;

    /// <inheritdoc />
    public NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who) =>
        obj is StorageNode
        {
            Data: Storage storageObject,
        }
            ? storageObject.Mutex
            : default;

    /// <inheritdoc />
    public Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who) =>
        obj is StorageNode
        {
            Data: Storage storageObject,
        }
            ? storageObject.Position
            : default;

    /// <inheritdoc />
    public bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) =>
        obj is StorageNode storage && storage.FilterMatches(item);

    /// <inheritdoc />
    public bool IsMutexRequired(object obj, GameLocation? location, Farmer? who) => true;

    /// <inheritdoc />
    public bool IsValid(object obj, GameLocation? location, Farmer? who) => obj is StorageNode;
}
