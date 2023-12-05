namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>Provides access to all supported storages in the game.</summary>
internal sealed class StorageService
{
#nullable disable
    private static StorageService instance;
#nullable enable

    private readonly ModConfig config;

    private EventHandler<IStorageTypeRequestedEventArgs>? storageTypeRequested;

    /// <summary>Initializes a new instance of the <see cref="StorageService" /> class.</summary>
    /// <param name="config">Mod config data.</param>
    public StorageService(ModConfig config)
    {
        StorageService.instance = this;
        this.config = config;
    }

    /// <summary>Gets storages from all locations and farmer inventory in the game.</summary>
    public static IEnumerable<StorageNode> All
    {
        get
        {
            var excluded = new HashSet<object>();
            var storages = new List<StorageNode>();

            // Iterate Inventory
            foreach (var storage in StorageService.FromPlayer(Game1.player, excluded))
            {
                storages.Add(storage);
                yield return storage;
            }

            // Iterate Locations
            var locations = new List<GameLocation>();
            Utility.ForEachLocation(
                location =>
                {
                    locations.Add(location);
                    return true;
                });

            foreach (var storage in locations.SelectMany(location => StorageService.FromLocation(location, excluded)))
            {
                storages.Add(storage);
                yield return storage;
            }

            // Sub Storage
            foreach (var storage in storages)
            {
                if (storage is not
                    {
                        Data: Storage storageObject,
                    })
                {
                    continue;
                }

                foreach (var subStorage in StorageService.FromStorage(storageObject, excluded))
                {
                    yield return subStorage;
                }
            }
        }
    }

    /// <summary>Gets the current storage item from the farmer's inventory.</summary>
    public static StorageNode? CurrentItem =>
        Game1.player.CurrentItem is not null && StorageService.TryGetOne(Game1.player.CurrentItem, out var storage)
            ? storage
            : null;

    /// <summary>Gets all placed storages in the current location.</summary>
    public static IEnumerable<StorageNode> CurrentLocation => StorageService.FromLocation(Game1.currentLocation);

    /// <summary>Gets storages in the farmer's inventory.</summary>
    public static IEnumerable<StorageNode> Inventory => StorageService.FromPlayer(Game1.player);

    private static ModConfig Config => StorageService.instance.config;

    /// <summary>Event for when a storage type is assigned to a storage object.</summary>
    public static event EventHandler<IStorageTypeRequestedEventArgs>? StorageTypeRequested
    {
        add => StorageService.instance.storageTypeRequested += value;
        remove => StorageService.instance.storageTypeRequested -= value;
    }

    /// <summary>Gets all storages placed in a particular location.</summary>
    /// <param name="location">The location to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all placed storages at the location.</returns>
    public static IEnumerable<StorageNode> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(location))
        {
            yield break;
        }

        excluded.Add(location);

        // Mod Integrations
        foreach (var storage in IntegrationService.FromLocation(location, excluded))
        {
            yield return StorageService.GetStorageType(storage);
        }

        // Get Fridge
        var fridge = location.GetFridge();
        if (fridge is not null && !excluded.Contains(fridge))
        {
            excluded.Add(fridge);
            var fridgePosition = location.GetFridgePosition() ?? Point.Zero;
            yield return StorageService.GetStorageType(new FridgeStorage(location, fridgePosition.ToVector2()));
        }

        // Get Shipping Bin
        if (location is IslandWest islandWest)
        {
            excluded.Add(islandWest);
            yield return StorageService.GetStorageType(
                new ShippingBinStorage(islandWest, islandWest.shippingBinPosition.ToVector2()));
        }

        if (location.IsBuildableLocation())
        {
            // Buildings
            foreach (var building in location.buildings)
            {
                // Special Buildings
                switch (building)
                {
                    case JunimoHut junimoHut when !excluded.Contains(junimoHut):
                        excluded.Add(junimoHut);
                        yield return StorageService.GetStorageType(
                            new JunimoHutStorage(
                                junimoHut,
                                location,
                                new(
                                    (int)(building.tileX.Value + (building.tilesWide.Value / 2f)),
                                    (int)(building.tileY.Value + (building.tilesHigh.Value / 2f)))));

                        break;
                    case ShippingBin shippingBin when !excluded.Contains(shippingBin):
                        excluded.Add(shippingBin);
                        yield return StorageService.GetStorageType(
                            new ShippingBinStorage(
                                shippingBin,
                                location,
                                new(
                                    (int)(building.tileX.Value + (building.tilesWide.Value / 2f)),
                                    (int)(building.tileY.Value + (building.tilesHigh.Value / 2f)))));

                        break;
                }
            }
        }

        // Objects
        foreach (var (position, obj) in location.Objects.Pairs)
        {
            if (position.X < 0
                || position.Y < 0
                || !StorageService.TryGetOne(obj, location, position, out var subStorage)
                || excluded.Contains(subStorage.Context))
            {
                continue;
            }

            excluded.Add(subStorage.Context);
            yield return StorageService.GetStorageType(subStorage);
        }
    }

    /// <summary>Gets all storages placed in a particular farmer's inventory.</summary>
    /// <param name="player">The farmer to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <param name="limit">Limit the number of items from the farmer's inventory.</param>
    /// <returns>An enumerable of all held storages in the farmer's inventory.</returns>
    public static IEnumerable<StorageNode> FromPlayer(Farmer player, ISet<object>? excluded = null, int? limit = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(player))
        {
            yield break;
        }

        excluded.Add(player);

        // Mod Integrations
        foreach (var storage in IntegrationService.FromPlayer(player, excluded))
        {
            yield return StorageService.GetStorageType(storage);
        }

        limit ??= player.MaxItems;
        var position = player.Tile;
        for (var index = 0; index < limit; ++index)
        {
            var item = player.Items[index];
            if (!StorageService.TryGetOne(item, player, position, out var storage)
                || excluded.Contains(storage.Context))
            {
                continue;
            }

            excluded.Add(storage.Context);
            yield return StorageService.GetStorageType(storage);
        }
    }

    /// <summary>Attempt to gets a placed storage at a specific position.</summary>
    /// <param name="location">The location to get the storage from.</param>
    /// <param name="pos">The position to get the storage from.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found at the location and position..</returns>
    public static bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out StorageNode? storage)
    {
        if (!location.Objects.TryGetValue(pos, out var obj)
            || !StorageService.TryGetOne(obj, location, pos, out var storageObject))
        {
            storage = default;
            return false;
        }

        storage = StorageService.GetStorageType(storageObject);
        return true;
    }

    /// <summary>Attempts to retrieve a storage based on a context object.</summary>
    /// <param name="context">The context object.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found for the context object.</returns>
    public static bool TryGetOne(object? context, [NotNullWhen(true)] out StorageNode? storage)
    {
        switch (context)
        {
            case StorageNode storageNode:
                storage = storageNode;
                return true;
            case Storage baseStorage:
                storage = StorageService.GetStorageType(baseStorage);
                return true;
        }

        if (!IntegrationService.TryGetOne(context, out var storageObject)
            && !StorageService.TryGetOne(context, default, default, out storageObject))
        {
            storage = default;
            return false;
        }

        storage = StorageService.GetStorageType(storageObject);
        return true;
    }

    private static IEnumerable<StorageNode> FromStorage(Storage storage, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(storage.Context))
        {
            return Array.Empty<StorageNode>();
        }

        excluded.Add(storage.Context);

        var storages = new List<Storage>();
        foreach (var item in storage.Inventory.Where(item => item is not null && !excluded.Contains(item)))
        {
            if (!StorageService.TryGetOne(item, storage.Source, storage.Position, out var storageObject)
                || excluded.Contains(storageObject.Context))
            {
                continue;
            }

            excluded.Add(storageObject.Context);
            storages.Add(storageObject);
        }

        return StorageService.GetStorageTypes(storages)
            .Concat(storages.SelectMany(subStorage => StorageService.FromStorage(subStorage, excluded)));
    }

    private static StorageNode GetStorageType(Storage storage)
    {
        var storageTypes = new List<IStorageData>();
        var storageTypeRequestedEventArgs = new StorageTypeRequestedEventArgs(storage.Context, storageTypes);
        StorageService.instance.storageTypeRequested.InvokeAll(StorageService.instance, storageTypeRequestedEventArgs);
        var storageType = storageTypes.FirstOrDefault();
        return new(
            storage,
            storageType is not null ? new StorageNode(storageType, StorageService.Config) : StorageService.Config);
    }

    private static IEnumerable<StorageNode> GetStorageTypes(IEnumerable<Storage> storages) =>
        storages.Select(StorageService.GetStorageType);

    private static bool TryGetOne(
        object? context,
        object? parent,
        Vector2 position,
        [NotNullWhen(true)] out Storage? storage)
    {
        switch (context)
        {
            case Storage storageObject:
                storage = storageObject;
                return true;
            case Farm farm:
                var farmShippingBin = farm.buildings.OfType<ShippingBin>().FirstOrDefault();
                storage = farmShippingBin is not null
                    ? new ShippingBinStorage(
                        farm,
                        new(
                            farmShippingBin.tileX.Value + (int)(farmShippingBin.tilesWide.Value / 2f),
                            farmShippingBin.tileY.Value + (int)(farmShippingBin.tilesHigh.Value / 2f)))
                    : default;

                return storage is not null;
            case FarmHouse
            {
                fridge.Value: not null,
            } farmHouse when !farmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(farmHouse, position);
                return true;
            case IslandFarmHouse
            {
                fridge.Value: not null,
            } islandFarmHouse when !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(islandFarmHouse, position);
                return true;
            case SObject
            {
                ParentSheetIndex: 165,
                heldObject.Value: Chest,
            } heldObj:
                storage = new ObjectStorage(heldObj, parent, position);
                return true;
            case Chest
            {
                SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin,
            } shippingChest:
                storage = new ShippingBinStorage(shippingChest, parent, position);
                return true;
            case Chest
            {
                playerChest.Value: true,
            } chest:
                storage = new ChestStorage(chest, parent, position);
                return true;
            case JunimoHut junimoHut:
                storage = new JunimoHutStorage(junimoHut, parent, position);
                return true;
            case IslandWest islandWest:
                storage = new ShippingBinStorage(islandWest, position);
                return true;
            default:
                storage = default;
                return false;
        }
    }
}
