namespace StardewMods.BetterChests.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Extensions;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Provides access to all supported storages in the game.
/// </summary>
internal sealed class Storages
{
#nullable disable
    private static Storages instance;
#nullable enable

    private readonly ModConfig config;

    private EventHandler<IStorageTypeRequestedEventArgs>? storageTypeRequested;

    private Storages(ModConfig config)
    {
        this.config = config;
    }

    /// <summary>
    ///     Event for when a storage type is assigned to a storage object.
    /// </summary>
    public static event EventHandler<IStorageTypeRequestedEventArgs>? StorageTypeRequested
    {
        add => Storages.instance.storageTypeRequested += value;
        remove => Storages.instance.storageTypeRequested -= value;
    }

    /// <summary>
    ///     Gets storages from all locations and farmer inventory in the game.
    /// </summary>
    public static IEnumerable<StorageNode> All
    {
        get
        {
            var excluded = new HashSet<object>();
            var storages = new List<StorageNode>();

            // Iterate Inventory
            foreach (var storage in Storages.FromPlayer(Game1.player, excluded))
            {
                storages.Add(storage);
                yield return storage;
            }

            // Iterate Locations
            foreach (var location in CommonHelpers.AllLocations)
            {
                foreach (var storage in Storages.FromLocation(location, excluded))
                {
                    storages.Add(storage);
                    yield return storage;
                }
            }

            // Sub Storage
            foreach (var storage in storages)
            {
                if (storage is not { Data: Storage storageObject })
                {
                    continue;
                }

                foreach (var subStorage in Storages.FromStorage(storageObject, excluded))
                {
                    yield return subStorage;
                }
            }
        }
    }

    /// <summary>
    ///     Gets the current storage item from the farmer's inventory.
    /// </summary>
    public static StorageNode? CurrentItem =>
        Game1.player.CurrentItem is not null && Storages.TryGetOne(Game1.player.CurrentItem, out var storage)
            ? storage
            : null;

    /// <summary>
    ///     Gets all placed storages in the current location.
    /// </summary>
    public static IEnumerable<StorageNode> CurrentLocation => Storages.FromLocation(Game1.currentLocation);

    /// <summary>
    ///     Gets storages in the farmer's inventory.
    /// </summary>
    public static IEnumerable<StorageNode> Inventory => Storages.FromPlayer(Game1.player);

    private static ModConfig Config => Storages.instance.config;

    /// <summary>
    ///     Gets all storages placed in a particular location.
    /// </summary>
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
        foreach (var storage in Integrations.FromLocation(location, excluded))
        {
            yield return Storages.GetStorageType(storage);
        }

        // Special Locations
        switch (location)
        {
            case FarmHouse { fridge.Value: { } fridge } farmHouse
                when !excluded.Contains(fridge) && !farmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(fridge);
                yield return Storages.GetStorageType(
                    new FridgeStorage(farmHouse, farmHouse.fridgePosition.ToVector2()));
                break;
            case IslandFarmHouse { fridge.Value: { } fridge } islandFarmHouse when !excluded.Contains(fridge)
                && !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(fridge);
                yield return Storages.GetStorageType(
                    new FridgeStorage(islandFarmHouse, islandFarmHouse.fridgePosition.ToVector2()));
                break;
            case IslandWest islandWest:
                excluded.Add(islandWest);
                yield return Storages.GetStorageType(
                    new ShippingBinStorage(islandWest, islandWest.shippingBinPosition.ToVector2()));
                break;
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
                        yield return Storages.GetStorageType(
                            new JunimoHutStorage(
                                junimoHut,
                                location,
                                new(
                                    building.tileX.Value + (building.tilesWide.Value / 2),
                                    building.tileY.Value + (building.tilesHigh.Value / 2))));
                        break;
                    case ShippingBin shippingBin when !excluded.Contains(shippingBin):
                        excluded.Add(shippingBin);
                        yield return Storages.GetStorageType(
                            new ShippingBinStorage(
                                shippingBin,
                                location,
                                new(
                                    building.tileX.Value + (building.tilesWide.Value / 2),
                                    building.tileY.Value + (building.tilesHigh.Value / 2))));
                        break;
                }
            }
        }

        // Objects
        foreach (var (position, obj) in location.Objects.Pairs)
        {
            if (position.X < 0
                || position.Y < 0
                || !Storages.TryGetOne(obj, location, position, out var subStorage)
                || excluded.Contains(subStorage.Context))
            {
                continue;
            }

            excluded.Add(subStorage.Context);
            yield return Storages.GetStorageType(subStorage);
        }
    }

    /// <summary>
    ///     Gets all storages placed in a particular farmer's inventory.
    /// </summary>
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
        foreach (var storage in Integrations.FromPlayer(player, excluded))
        {
            yield return Storages.GetStorageType(storage);
        }

        limit ??= player.MaxItems;
        var position = player.Tile;
        for (var index = 0; index < limit; ++index)
        {
            var item = player.Items[index];
            if (!Storages.TryGetOne(item, player, position, out var storage) || excluded.Contains(storage.Context))
            {
                continue;
            }

            excluded.Add(storage.Context);
            yield return Storages.GetStorageType(storage);
        }
    }

    /// <summary>
    ///     Initialized <see cref="Storages" />.
    /// </summary>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="Storages" /> class.</returns>
    public static Storages Init(ModConfig config)
    {
        return Storages.instance ??= new(config);
    }

    /// <summary>
    ///     Attempt to gets a placed storage at a specific position.
    /// </summary>
    /// <param name="location">The location to get the storage from.</param>
    /// <param name="pos">The position to get the storage from.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found at the location and position..</returns>
    public static bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out StorageNode? storage)
    {
        if (!location.Objects.TryGetValue(pos, out var obj)
            || !Storages.TryGetOne(obj, location, pos, out var storageObject))
        {
            storage = default;
            return false;
        }

        storage = Storages.GetStorageType(storageObject);
        return true;
    }

    /// <summary>
    ///     Attempts to retrieve a storage based on a context object.
    /// </summary>
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
                storage = Storages.GetStorageType(baseStorage);
                return true;
        }

        if (!Integrations.TryGetOne(context, out var storageObject)
            && !Storages.TryGetOne(context, default, default, out storageObject))
        {
            storage = default;
            return false;
        }

        if (storageObject is ShippingBinStorage { Context: not Chest }
            && Integrations.TestConflicts(nameof(BetterShippingBin), out _))
        {
            storage = default;
            return false;
        }

        storage = Storages.GetStorageType(storageObject);
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
        foreach (var item in storage.Items.Where(item => item is not null && !excluded.Contains(item)))
        {
            if (!Storages.TryGetOne(item, storage.Source, storage.Position, out var storageObject)
                || excluded.Contains(storageObject.Context))
            {
                continue;
            }

            excluded.Add(storageObject.Context);
            storages.Add(storageObject);
        }

        return Storages.GetStorageTypes(storages)
            .Concat(storages.SelectMany(subStorage => Storages.FromStorage(subStorage, excluded)));
    }

    private static StorageNode GetStorageType(Storage storage)
    {
        var storageTypes = new List<IStorageData>();
        var storageTypeRequestedEventArgs = new StorageTypeRequestedEventArgs(storage.Context, storageTypes);
        Storages.instance.storageTypeRequested.InvokeAll(Storages.instance, storageTypeRequestedEventArgs);
        var storageType = storageTypes.FirstOrDefault();
        return new(storage, storageType is not null ? new StorageNode(storageType, Storages.Config) : Storages.Config);
    }

    private static IEnumerable<StorageNode> GetStorageTypes(IEnumerable<Storage> storages)
    {
        return storages.Select(Storages.GetStorageType);
    }

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
                            farmShippingBin.tileX.Value + (farmShippingBin.tilesWide.Value / 2),
                            farmShippingBin.tileY.Value + (farmShippingBin.tilesHigh.Value / 2)))
                    : default;
                return storage is not null;
            case FarmHouse { fridge.Value: { } } farmHouse when !farmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(farmHouse, position);
                return true;
            case IslandFarmHouse { fridge.Value: { } } islandFarmHouse
                when !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(islandFarmHouse, position);
                return true;
            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest } heldObj:
                storage = new ObjectStorage(heldObj, parent, position);
                return true;
            case Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } shippingChest:
                storage = new ShippingBinStorage(shippingChest, parent, position);
                return true;
            case Chest { playerChest.Value: true } chest:
                storage = new ChestStorage(chest, parent, position);
                return true;
            case ShippingBin or IslandWest when !Storages.Config.BetterShippingBin:
                storage = default;
                return false;
            case ShippingBin shippingBin:
                storage = new ShippingBinStorage(shippingBin, parent, position);
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