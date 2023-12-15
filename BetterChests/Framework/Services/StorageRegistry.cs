namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Buildings;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class StorageRegistry
{
    private readonly StorageFactory storageFactory;

    /// <summary>Initializes a new instance of the <see cref="StorageRegistry" /> class.</summary>
    /// <param name="storageFactory">The storage factory to use for creating storage objects.</param>
    public StorageRegistry(StorageFactory storageFactory) => this.storageFactory = storageFactory;

    /// <summary>
    ///     Retrieves all storage items that satisfy the specified predicate, if provided. If no predicate is provided,
    ///     returns all storage items.
    /// </summary>
    /// <param name="predicate">Optional. A function that defines the conditions of the storage items to search for.</param>
    /// <returns>An enumerable collection of IStorage items that satisfy the predicate, if provided.</returns>
    public IEnumerable<IStorage> GetAll(Func<IStorage, bool>? predicate = default)
    {
        var foundStorages = new HashSet<IStorage>();
        var storageQueue = new Queue<IStorage>();

        foreach (var storage in this.GetAllFromPlayers(foundStorages, storageQueue, predicate))
        {
            yield return storage;
        }

        foreach (var storage in this.GetAllFromLocations(foundStorages, storageQueue, predicate))
        {
            yield return storage;
        }

        foreach (var storage in this.GetAllFromStorages(foundStorages, storageQueue, predicate))
        {
            yield return storage;
        }
    }

    /// <summary>Retrieves all storages from the specified storage that match the optional predicate.</summary>
    /// <param name="storage">The storage where the storage items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the storages.</param>
    /// <returns>An enumerable collection of storages that match the predicate.</returns>
    public IEnumerable<IStorage> GetAllFromStorage(IStorage storage, Func<IStorage, bool>? predicate = default)
    {
        foreach (var item in storage.Items)
        {
            if (!this.storageFactory.TryGetStorage(item, storage, out var storageItem))
            {
                continue;
            }

            if (predicate is null || predicate(storageItem))
            {
                yield return storageItem;
            }
        }
    }

    /// <summary>Retrieves all storages from the specified game location that match the optional predicate.</summary>
    /// <param name="location">The game location where the storage items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the storages.</param>
    /// <returns>An enumerable collection of storages that match the predicate.</returns>
    public IEnumerable<IStorage> GetAllFromLocation(GameLocation location, Func<IStorage, bool>? predicate = default)
    {
        // Search for storages from objects in the location
        foreach (var obj in location.Objects.Values)
        {
            if (!this.storageFactory.TryGetStorage(obj, out var storage))
            {
                continue;
            }

            if (predicate is null || predicate(storage))
            {
                yield return storage;
            }
        }

        // Search for storages from buildings in the location
        foreach (var building in location.buildings)
        {
            foreach (var chest in building.buildingChests)
            {
                if (!this.storageFactory.TryGetStorage(chest, building, out var storage))
                {
                    continue;
                }

                if (predicate is null || predicate(storage))
                {
                    yield return storage;
                }
            }
        }

        // Get storage for fridge in location
        if (location.GetFridge() is
            { } fridge)
        {
            if (!this.storageFactory.TryGetStorage(fridge, location, out var storage))
            {
                yield break;
            }

            if (predicate is null || predicate(storage))
            {
                yield return storage;
            }
        }
    }

    /// <summary>Retrieves all storage items from the specified player matching the optional predicate.</summary>
    /// <param name="farmer">The player whose storage items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the storages.</param>
    /// <returns>An enumerable collection of storages that match the predicate.</returns>
    public IEnumerable<IStorage> GetAllFromPlayer(Farmer farmer, Func<IStorage, bool>? predicate = default)
    {
        foreach (var item in farmer.Items)
        {
            if (!this.storageFactory.TryGetStorage(item, farmer, out var storage))
            {
                continue;
            }

            if (predicate is null || predicate(storage))
            {
                yield return storage;
            }
        }
    }

    private IEnumerable<IStorage> GetAllFromPlayers(ISet<IStorage> foundStorages,
        Queue<IStorage> storageQueue,
        Func<IStorage, bool>? predicate = default)
    {
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var storage in this.GetAllFromPlayer(farmer, predicate))
            {
                if (!foundStorages.Add(storage))
                {
                    continue;
                }

                storageQueue.Enqueue(storage);
                yield return storage;
            }
        }
    }

    private IEnumerable<IStorage> GetAllFromLocations(ISet<IStorage> foundStorages,
        Queue<IStorage> storageQueue,
        Func<IStorage, bool>? predicate = default)
    {
        var foundLocations = new HashSet<GameLocation>();
        var locationQueue = new Queue<GameLocation>();

        foreach (var location in Game1.locations)
        {
            locationQueue.Enqueue(location);
        }

        while (locationQueue.TryDequeue(out var location))
        {
            if (!foundLocations.Add(location))
            {
                continue;
            }

            foreach (var storage in this.GetAllFromLocation(location, predicate))
            {
                if (!foundStorages.Add(storage))
                {
                    continue;
                }

                storageQueue.Enqueue(storage);
                yield return storage;
            }

            foreach (var building in location.buildings)
            {
                if (building.GetIndoorsType() == IndoorsType.Instanced)
                {
                    locationQueue.Enqueue(building.GetIndoors());
                }
            }
        }
    }

    private IEnumerable<IStorage> GetAllFromStorages(ISet<IStorage> foundStorages,
        Queue<IStorage> storageQueue,
        Func<IStorage, bool>? predicate = default)
    {
        while (storageQueue.TryDequeue(out var storage))
        {
            foreach (var childStorage in this.GetAllFromStorage(storage, predicate))
            {
                if (!foundStorages.Add(childStorage))
                {
                    continue;
                }

                storageQueue.Enqueue(childStorage);
                yield return childStorage;
            }
        }
    }
}
