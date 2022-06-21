namespace StardewMods.BetterChests.Helpers;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

internal static class StorageHelper
{
    public static IEnumerable<BaseStorage> All
    {
        get
        {
            var excluded = new HashSet<object>();
            var storages = new List<BaseStorage>();

            // Iterate Inventory
            foreach (var storage in StorageHelper.FromPlayer(Game1.player, excluded))
            {
                storages.Add(storage);
                yield return storage;
            }

            // Iterate Locations
            foreach (var location in StorageHelper.Locations)
            {
                foreach (var (storage, _, _) in StorageHelper.FromLocation(location, excluded))
                {
                    storages.Add(storage);
                    yield return storage;
                }
            }

            // Sub Storage
            foreach (var storage in storages.SelectMany(managedStorage => StorageHelper.FromStorage(managedStorage, excluded)))
            {
                yield return storage;
            }
        }
    }

    public static IEnumerable<BaseStorage> Inventory
    {
        get
        {
            var excluded = new HashSet<object>();
            foreach (var storage in StorageHelper.FromPlayer(Game1.player, excluded))
            {
                yield return storage;
            }
        }
    }

    public static IEnumerable<LocationStorage> World
    {
        get
        {
            var excluded = new HashSet<object>();
            foreach (var location in StorageHelper.Locations)
            {
                foreach (var storage in StorageHelper.FromLocation(location, excluded))
                {
                    yield return storage;
                }
            }
        }
    }

    public static Dictionary<string, StorageData> Types { get; } = new();

    private static IEnumerable<GameLocation> Locations
    {
        get => Context.IsMainPlayer ? Game1.locations : StorageHelper.Multiplayer!.GetActiveLocations();
    }

    private static IMultiplayerHelper? Multiplayer { get; set; }

    public static IEnumerable<LocationStorage> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(location))
        {
            yield break;
        }

        excluded.Add(location);

        // Special Locations
        switch (location)
        {
            case FarmHouse { fridge.Value: { } } farmHouse when !excluded.Contains(farmHouse) && !farmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(farmHouse);
                yield return new(new FridgeStorage(farmHouse), location, farmHouse.fridgePosition.ToVector2());
                break;
            case IslandFarmHouse { fridge.Value: { } } islandFarmHouse when !excluded.Contains(islandFarmHouse) && !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(islandFarmHouse);
                yield return new(new FridgeStorage(islandFarmHouse), location, islandFarmHouse.fridgePosition.ToVector2());
                break;
            case IslandWest islandWest when !excluded.Contains(islandWest):
                excluded.Add(islandWest);
                yield return new(new ShippingBinStorage(islandWest), location, islandWest.shippingBinPosition.ToVector2());
                break;
        }

        if (location is BuildableGameLocation buildableGameLocation)
        {
            // Buildings
            foreach (var building in buildableGameLocation.buildings)
            {
                // Special Buildings
                switch (building)
                {
                    case JunimoHut junimoHut when !excluded.Contains(junimoHut):
                        excluded.Add(junimoHut);
                        yield return new(new JunimoHutStorage(junimoHut), location, new(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2));
                        break;
                    case ShippingBin shippingBin when !excluded.Contains(shippingBin):
                        excluded.Add(shippingBin);
                        yield return new(new ShippingBinStorage(shippingBin), location, new(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2));
                        break;
                }

                // Indoors
                if (building.indoors.Value is not null)
                {
                    foreach (var subStorage in StorageHelper.FromLocation(building.indoors.Value, excluded).Where(subStorage => !excluded.Contains(subStorage.Storage.Context)))
                    {
                        excluded.Add(subStorage.Storage.Context);
                        yield return subStorage;
                    }
                }
            }
        }

        // Objects
        foreach (var (position, obj) in location.Objects.Pairs)
        {
            if (StorageHelper.TryGetOne(obj, out var subStorage) && !excluded.Contains(subStorage!.Context))
            {
                excluded.Add(subStorage.Context);
                yield return new(subStorage, location, position);
            }
        }
    }

    public static IEnumerable<BaseStorage> FromPlayer(Farmer player, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(player))
        {
            yield break;
        }

        excluded.Add(player);

        for (var index = 0; index < player.MaxItems; index++)
        {
            var item = player.Items[index];
            if (StorageHelper.TryGetOne(item, out var managedStorage) && !excluded.Contains(managedStorage!.Context))
            {
                excluded.Add(managedStorage.Context);
                yield return managedStorage;
            }
        }
    }

    public static void Init(IMultiplayerHelper multiplayer)
    {
        StorageHelper.Multiplayer = multiplayer;
    }

    public static bool TryGetOne(object context, [NotNullWhen(true)]out BaseStorage? storage)
    {
        switch (context)
        {
            case Chest chest:
                storage = new ChestStorage(chest);
                return true;
            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest } heldObj:
                storage = new ObjectStorage(heldObj);
                return true;
            case ShippingBin shippingBin:
                storage = new ShippingBinStorage(shippingBin);
                return true;
            case JunimoHut junimoHut:
                storage = new JunimoHutStorage(junimoHut);
                return true;
            case FarmHouse { fridge.Value: { } } farmHouse when !farmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(farmHouse);
                return true;
            case IslandFarmHouse { fridge.Value: { } } islandFarmHouse when !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(islandFarmHouse);
                return true;
            case IslandWest islandWest:
                storage = new ShippingBinStorage(islandWest);
                return true;
            default:
                storage = default;
                return false;
        }
    }

    private static IEnumerable<BaseStorage> FromStorage(BaseStorage storage, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(storage.Context))
        {
            yield break;
        }

        excluded.Add(storage.Context);
        var managedStorages = new List<BaseStorage>();

        foreach (var item in storage.Items.Where(item => item is not null && !excluded.Contains(item)))
        {
            if (StorageHelper.TryGetOne(item!, out var managedStorage) && !excluded.Contains(managedStorage!.Context))
            {
                excluded.Add(managedStorage.Context);
                managedStorages.Add(managedStorage);
                yield return managedStorage;
            }
        }

        // Sub Storage
        foreach (var subStorage in managedStorages.SelectMany(managedStorage => StorageHelper.FromStorage(managedStorage, excluded)))
        {
            yield return subStorage;
        }
    }

    public record LocationStorage(BaseStorage Storage, GameLocation Location, Vector2 Position);
}