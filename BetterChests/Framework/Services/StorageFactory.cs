namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewValley.Buildings;
using StardewValley.Objects;

/// <summary>Represents a factory class for creating storage instances.</summary>
internal sealed class StorageFactory
{
    /// <summary>Tries to get a storage object based on the provided item.</summary>
    /// <param name="item">The item to check for storage.</param>
    /// <param name="parentStorage">The parent storage object.</param>
    /// <param name="storage">
    ///     When this method returns, contains the storage object corresponding to the item if it exists;
    ///     otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if a storage object is found; otherwise, <c>false</c>.</returns>
    public bool TryGetStorage(Item item, IStorage parentStorage, [NotNullWhen(true)] out IStorage? storage)
    {
        if (item is not SObject obj || !this.TryGetStorage(obj, out var childStorage))
        {
            storage = null;
            return false;
        }

        storage = new StoredStorage(parentStorage, childStorage);
        return true;
    }

    /// <summary>Tries to get a storage from the given item belonging to a player.</summary>
    /// <param name="item">The item to get a storage from.</param>
    /// <param name="farmer">The farmer inventory of the item.</param>
    /// <param name="storage">
    ///     When this method returns, contains the storage object if the item was retrieved successfully;
    ///     otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns><c>true</c> if a storage object is found; otherwise, <c>false</c>.</returns>
    public bool TryGetStorage(Item item, Farmer farmer, [NotNullWhen(true)] out IStorage? storage)
    {
        if (item is not SObject obj || !this.TryGetStorage(obj, out var childStorage))
        {
            storage = null;
            return false;
        }

        storage = new FarmerStorage(farmer, childStorage);
        return true;
    }

    /// <summary>Tries to retrieve the storage for a building chest.</summary>
    /// <param name="chest">The chest to retrieve the storage for.</param>
    /// <param name="building">The building to retrieve the storage from.</param>
    /// <param name="storage">
    ///     When this method returns, contains the storage for the specified chest within the building, if
    ///     the storage is available; otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if a storage object is found; otherwise, <c>false</c>.</returns>
    public bool TryGetStorage(Chest chest, Building building, [NotNullWhen(true)] out IStorage? storage)
    {
        if (!this.TryGetStorage(chest, out var childStorage))
        {
            storage = null;
            return false;
        }

        storage = new BuildingStorage(building, childStorage);
        return true;
    }

    /// <summary>Tries to get the fridge storage from a game location.</summary>
    /// <param name="chest">The chest object.</param>
    /// <param name="location">The game location.</param>
    /// <param name="storage">Output parameter for the storage object.</param>
    /// <returns>True if the storage was found and retrieved successfully, otherwise false.</returns>
    public bool TryGetStorage(Chest chest, GameLocation location, [NotNullWhen(true)] out IStorage? storage)
    {
        if (!this.TryGetStorage(chest, out var childStorage))
        {
            storage = null;
            return false;
        }

        storage = new FridgeStorage(location, childStorage);
        return true;
    }

    /// <summary>Tries to get storage from the specified object and location.</summary>
    /// <param name="obj">The object to get storage from.</param>
    /// <param name="storage">When this method returns, contains the storage if found; otherwise, null.</param>
    /// <returns><c>true</c> if a storage object is found; otherwise, <c>false</c>.</returns>
    public bool TryGetStorage(SObject obj, [NotNullWhen(true)] out IStorage? storage)
    {
        switch (obj)
        {
            case Chest chest:
                storage = new ChestStorage(chest);
                return true;
            case StorageFurniture storageFurniture:
                storage = new FurnitureStorage(storageFurniture);
                return true;
            case
            {
                heldObject.Value:
                { } heldObject
            } when this.TryGetStorage(heldObject, out var childStorage):
                storage = new HeldStorage(obj, childStorage);
                return true;
            default:
                storage = null;
                return false;
        }
    }
}
