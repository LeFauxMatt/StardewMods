namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.StorageHandlers;
using StardewMods.Common.Enums;
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
    private static Storages Instance;
#nullable enable

    private readonly ModConfig _config;
    private readonly Lazy<Dictionary<object, IStorageObject>> _referenceContext;
    private readonly Dictionary<Func<object, bool>, IStorageData> _storageTypes;

    private Storages(ModConfig config, Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        this._config = config;
        this._storageTypes = storageTypes;

        foreach (var (predicate, storageData) in Integrations.InitTypes(config.VanillaStorages))
        {
            this._storageTypes.Add(predicate, new StorageNodeData(storageData, config));
        }

        this.InitTypes(config.VanillaStorages, config);
        this._referenceContext = new(
            () =>
            {
                var referenceContext = new Dictionary<object, IStorageObject>();
                foreach (var location in CommonHelpers.AllLocations)
                {
                    switch (location)
                    {
                        // Shipping Bin for Chests Anywhere
                        case Farm farm when !referenceContext.ContainsKey(farm):
                            var shippingBin = farm.buildings.OfType<ShippingBin>().FirstOrDefault();
                            if (shippingBin is not null)
                            {
                                referenceContext.Add(
                                    farm,
                                    new ShippingBinStorage(
                                        farm,
                                        new(
                                            shippingBin.tileX.Value + shippingBin.tilesWide.Value / 2,
                                            shippingBin.tileY.Value + shippingBin.tilesHigh.Value / 2)));
                            }

                            break;

                        // Fridge
                        case FarmHouse { fridge.Value: { } fridge, fridgePosition: var fridgePosition } farmHouse
                            when !referenceContext.ContainsKey(fridge) && !fridgePosition.Equals(Point.Zero):
                            referenceContext.Add(fridge, new FridgeStorage(farmHouse, fridgePosition.ToVector2()));
                            break;

                        // Island Fridge
                        case IslandFarmHouse
                            {
                                fridge.Value: { } islandFridge, fridgePosition: var islandFridgePosition,
                            } islandFarmHouse when !referenceContext.ContainsKey(islandFridge)
                                                && !islandFridgePosition.Equals(Point.Zero):
                            referenceContext.Add(
                                islandFridge,
                                new FridgeStorage(islandFarmHouse, islandFridgePosition.ToVector2()));
                            break;
                    }
                }

                return referenceContext;
            });
    }

    /// <summary>
    ///     Gets storages from all locations and farmer inventory in the game.
    /// </summary>
    public static IEnumerable<IStorageObject> All
    {
        get
        {
            var excluded = new HashSet<object>();
            var storages = new List<IStorageObject>();

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
            foreach (var storage in storages.SelectMany(
                         managedStorage => Storages.FromStorage(managedStorage, excluded)))
            {
                yield return storage;
            }
        }
    }

    /// <summary>
    ///     Gets the current storage item from the farmer's inventory.
    /// </summary>
    public static IStorageObject? CurrentItem =>
        Game1.player.CurrentItem is not null && Storages.TryGetOne(Game1.player.CurrentItem, out var storage)
            ? storage.WithType(Storages.StorageTypes)
            : null;

    /// <summary>
    ///     Gets all placed storages in the current location.
    /// </summary>
    public static IEnumerable<IStorageObject> CurrentLocation => Storages.FromLocation(Game1.currentLocation);

    /// <summary>
    ///     Gets storages in the farmer's inventory.
    /// </summary>
    public static IEnumerable<IStorageObject> Inventory => Storages.FromPlayer(Game1.player);

    /// <summary>
    ///     Gets the types of storages in the game.
    /// </summary>
    public static Dictionary<string, IStorageData> Types { get; } = new();

    /// <summary>
    ///     Gets all placed storages in the world.
    /// </summary>
    public static IEnumerable<IStorageObject> World
    {
        get
        {
            var excluded = new HashSet<object>();
            return CommonHelpers.AllLocations.SelectMany(location => Storages.FromLocation(location, excluded));
        }
    }

    private static ModConfig Config => Storages.Instance._config;

    private static Dictionary<object, IStorageObject> ReferenceContext =>
        Storages.Instance._referenceContext.Value;

    private static Dictionary<Func<object, bool>, IStorageData> StorageTypes => Storages.Instance._storageTypes;

    /// <summary>
    ///     Gets all storages placed in a particular location.
    /// </summary>
    /// <param name="location">The location to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all placed storages at the location.</returns>
    public static IEnumerable<IStorageObject> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(location))
        {
            return Array.Empty<IStorageObject>();
        }

        excluded.Add(location);

        IEnumerable<IStorageObject> GetAll()
        {
            // Mod Integrations
            foreach (var storage in Integrations.FromLocation(location, excluded))
            {
                yield return storage;
            }

            // Special Locations
            switch (location)
            {
                case FarmHouse { fridge.Value: { } fridge } farmHouse
                    when !excluded.Contains(fridge) && !farmHouse.fridgePosition.Equals(Point.Zero):
                    excluded.Add(fridge);
                    yield return new FridgeStorage(farmHouse, farmHouse.fridgePosition.ToVector2());
                    break;
                case IslandFarmHouse { fridge.Value: { } fridge } islandFarmHouse when !excluded.Contains(fridge)
                 && !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                    excluded.Add(fridge);
                    yield return new FridgeStorage(islandFarmHouse, islandFarmHouse.fridgePosition.ToVector2());
                    break;
                case IslandWest islandWest:
                    excluded.Add(islandWest);
                    yield return new ShippingBinStorage(islandWest, islandWest.shippingBinPosition.ToVector2());
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
                            yield return new JunimoHutStorage(
                                junimoHut,
                                location,
                                new(
                                    building.tileX.Value + building.tilesWide.Value / 2,
                                    building.tileY.Value + building.tilesHigh.Value / 2));
                            break;
                        case ShippingBin shippingBin when !excluded.Contains(shippingBin):
                            excluded.Add(shippingBin);
                            yield return new ShippingBinStorage(
                                shippingBin,
                                location,
                                new(
                                    building.tileX.Value + building.tilesWide.Value / 2,
                                    building.tileY.Value + building.tilesHigh.Value / 2));
                            break;
                    }
                }
            }

            // Objects
            foreach (var (position, obj) in location.Objects.Pairs)
            {
                if (!Storages.TryGetOne(obj, location, position, out var subStorage)
                 || excluded.Contains(subStorage.Context))
                {
                    continue;
                }

                excluded.Add(subStorage.Context);
                yield return subStorage;
            }
        }

        return GetAll().WithTypes(Storages.StorageTypes);
    }

    /// <summary>
    ///     Gets all storages placed in a particular farmer's inventory.
    /// </summary>
    /// <param name="player">The farmer to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <param name="limit">Limit the number of items from the farmer's inventory.</param>
    /// <returns>An enumerable of all held storages in the farmer's inventory.</returns>
    public static IEnumerable<IStorageObject> FromPlayer(
        Farmer player,
        ISet<object>? excluded = null,
        int? limit = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(player))
        {
            return Array.Empty<IStorageObject>();
        }

        excluded.Add(player);

        IEnumerable<IStorageObject> GetAll()
        {
            // Mod Integrations
            foreach (var storage in Integrations.FromPlayer(player, excluded))
            {
                yield return storage;
            }

            limit ??= player.MaxItems;
            var position = player.getTileLocation();
            for (var index = 0; index < limit; ++index)
            {
                var item = player.Items[index];
                if (!Storages.TryGetOne(item, player, position, out var storage) || excluded.Contains(storage.Context))
                {
                    continue;
                }

                excluded.Add(storage.Context);
                yield return storage;
            }
        }

        return GetAll().WithTypes(Storages.StorageTypes);
    }

    /// <summary>
    ///     Initialized <see cref="Storages" />.
    /// </summary>
    /// <param name="config">Mod config data.</param>
    /// <param name="storageTypes">A dictionary of all registered storage types.</param>
    /// <returns>Returns an instance of the <see cref="Storages" /> class.</returns>
    public static Storages Init(ModConfig config, Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        return Storages.Instance ??= new(config, storageTypes);
    }

    /// <summary>
    ///     Attempt to gets a placed storage at a specific position.
    /// </summary>
    /// <param name="location">The location to get the storage from.</param>
    /// <param name="pos">The position to get the storage from.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found at the location and position..</returns>
    public static bool TryGetOne(GameLocation location, Vector2 pos, [NotNullWhen(true)] out IStorageObject? storage)
    {
        storage = Storages.FromLocation(location)
                          .FirstOrDefault(locationStorage => locationStorage.Position.Equals(pos));
        return storage is not null;
    }

    /// <summary>
    ///     Attempts to retrieve a storage based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found for the context object.</returns>
    public static bool TryGetOne(object? context, [NotNullWhen(true)] out IStorageObject? storage)
    {
        if (context is IStorageObject baseStorage)
        {
            storage = baseStorage;
            return true;
        }

        if (!Integrations.TryGetOne(context, out storage)
         && !Storages.TryGetOne(context, default, default, out storage))
        {
            return false;
        }

        if (storage is ShippingBinStorage && Integrations.TestConflicts(nameof(BetterShippingBin), out _))
        {
            storage = null;
            return false;
        }

        storage.WithType(Storages.StorageTypes);
        return true;
    }

    private static IEnumerable<IStorageObject> FromStorage(IStorageObject storage, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(storage.Context))
        {
            return Array.Empty<IStorageObject>();
        }

        excluded.Add(storage.Context);

        IEnumerable<IStorageObject> GetAll()
        {
            var managedStorages = new List<IStorageObject>();

            foreach (var item in storage.Items.Where(item => item is not null && !excluded.Contains(item)))
            {
                if (!Storages.TryGetOne(item, storage.Source, storage.Position, out var managedStorage)
                 || excluded.Contains(managedStorage.Context))
                {
                    continue;
                }

                excluded.Add(managedStorage.Context);
                managedStorages.Add(managedStorage);
                yield return managedStorage;
            }

            // Sub Storage
            foreach (var subStorage in managedStorages.SelectMany(
                         managedStorage => Storages.FromStorage(managedStorage, excluded)))
            {
                yield return subStorage;
            }
        }

        return GetAll().WithTypes(Storages.StorageTypes);
    }

    private static bool TryGetOne(
        object? context,
        object? parent,
        Vector2 position,
        [NotNullWhen(true)] out IStorageObject? storage)
    {
        if (context is not null && Storages.ReferenceContext.TryGetValue(context, out storage))
        {
            return true;
        }

        switch (context)
        {
            case IStorageObject storageObject:
                storage = storageObject;
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
            case FarmHouse { fridge.Value: { } } farmHouse when !farmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(farmHouse, position);
                return true;
            case IslandFarmHouse { fridge.Value: { } } islandFarmHouse
                when !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(islandFarmHouse, position);
                return true;
            case IslandWest islandWest:
                storage = new ShippingBinStorage(islandWest, position);
                return true;
            default:
                storage = default;
                return false;
        }
    }

    private void InitTypes(IDictionary<string, StorageData> vanillaStorages, IStorageData defaultStorage)
    {
        // Chest
        if (!vanillaStorages.TryGetValue("Chest", out var storageData))
        {
            storageData = new();
            vanillaStorages.Add("Chest", storageData);
        }

        this._storageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 130,
            },
            new StorageNodeData(storageData, defaultStorage));

        // Fridge
        if (!vanillaStorages.TryGetValue("Fridge", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Fridge", storageData);
        }

        this._storageTypes.Add(
            context => context is FarmHouse or IslandFarmHouse,
            new StorageNodeData(storageData, defaultStorage));

        // Junimo Chest
        if (!vanillaStorages.TryGetValue("Junimo Chest", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Junimo Chest", storageData);
        }

        this._storageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            },
            new StorageNodeData(storageData, defaultStorage));

        // Junimo Hut
        if (!vanillaStorages.TryGetValue("Junimo Hut", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Junimo Hut", storageData);
        }

        this._storageTypes.Add(context => context is JunimoHut, new StorageNodeData(storageData, defaultStorage));

        // Mini-Fridge
        if (!vanillaStorages.TryGetValue("Mini-Fridge", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Mini-Fridge", storageData);
        }

        this._storageTypes.Add(
            context => context is Chest { fridge.Value: true },
            new StorageNodeData(storageData, defaultStorage));

        // Mini-Shipping Bin
        if (!vanillaStorages.TryGetValue("Mini-Shipping Bin", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Mini-Shipping Bin", storageData);
        }

        this._storageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin,
            },
            new StorageNodeData(storageData, defaultStorage));

        // Shipping Bin
        if (!vanillaStorages.TryGetValue("Shipping Bin", out storageData))
        {
            storageData = new()
            {
                CustomColorPicker = FeatureOption.Disabled,
            };
            vanillaStorages.Add("Shipping Bin", storageData);
        }

        this._storageTypes.Add(
            context => context is ShippingBin or Farm or IslandWest,
            new StorageNodeData(storageData, defaultStorage));

        // Stone Chest
        if (!vanillaStorages.TryGetValue("Stone Chest", out storageData))
        {
            storageData = new();
            vanillaStorages.Add("Stone Chest", storageData);
        }

        this._storageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 232,
            },
            new StorageNodeData(storageData, defaultStorage));
    }
}