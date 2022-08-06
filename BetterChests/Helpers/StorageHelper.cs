namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <summary>
///     Provides access to all supported storages in the game.
/// </summary>
internal class StorageHelper
{
    private static readonly Lazy<Dictionary<object, IStorageObject>> ReferenceContextLazy =
        new(StorageHelper.InitReferenceContext);

    private static StorageHelper? Instance;

    private readonly ModConfig _config;

    private StorageHelper(ModConfig config, Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        this._config = config;
        this.StorageTypes = storageTypes;

        // Chest
        if (!this._config.VanillaStorages.TryGetValue("Chest", out var storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Chest", storageData);
        }

        this.StorageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 130,
            },
            storageData);

        // Fridge
        if (!this._config.VanillaStorages.TryGetValue("Fridge", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Fridge", storageData);
        }

        this.StorageTypes.Add(context => context is FarmHouse or IslandFarmHouse, storageData);

        // Junimo Chest
        if (!this._config.VanillaStorages.TryGetValue("Junimo Chest", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Junimo Chest", storageData);
        }

        this.StorageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            },
            storageData);

        // Junimo Hut
        if (!this._config.VanillaStorages.TryGetValue("Junimo Hut", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Junimo Hut", storageData);
        }

        this.StorageTypes.Add(context => context is JunimoHut, storageData);

        // Mini-Fridge
        if (!this._config.VanillaStorages.TryGetValue("Mini-Fridge", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Mini-Fridge", storageData);
        }

        this.StorageTypes.Add(context => context is Chest { fridge.Value: true }, storageData);

        // Mini-Shipping Bin
        if (!this._config.VanillaStorages.TryGetValue("Mini-Shipping Bin", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Mini-Shipping Bin", storageData);
        }

        this.StorageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin,
            },
            storageData);

        // Shipping Bin
        if (!this._config.VanillaStorages.TryGetValue("Shipping Bin", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Shipping Bin", storageData);
        }

        this.StorageTypes.Add(context => context is ShippingBin or Farm or IslandWest, storageData);

        // Stone Chest
        if (!this._config.VanillaStorages.TryGetValue("Stone Chest", out storageData))
        {
            storageData = new();
            this._config.VanillaStorages.Add("Stone Chest", storageData);
        }

        this.StorageTypes.Add(
            context => context is Chest
            {
                playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None, ParentSheetIndex: 232,
            },
            storageData);
    }

    /// <summary>
    ///     Gets storages from all locations and farmer inventory in the game.
    /// </summary>
    public static IEnumerable<IStorageObject> All
    {
        get
        {
            IEnumerable<IStorageObject> GetAll()
            {
                var excluded = new HashSet<object>();
                var storages = new List<IStorageObject>();

                // Inventory Mod Integrations
                foreach (var storage in IntegrationHelper.FromPlayer(Game1.player, excluded))
                {
                    storages.Add(storage);
                    yield return storage;
                }

                // Iterate Inventory
                foreach (var storage in StorageHelper.FromPlayer(Game1.player, excluded))
                {
                    storages.Add(storage);
                    yield return storage;
                }

                // Iterate Locations
                foreach (var location in LocationHelper.AllLocations)
                {
                    // Mod Integrations
                    foreach (var storage in IntegrationHelper.FromLocation(location, excluded))
                    {
                        storages.Add(storage);
                        yield return storage;
                    }

                    foreach (var storage in StorageHelper.Instance!.FromLocation(location, excluded))
                    {
                        storages.Add(storage);
                        yield return storage;
                    }
                }

                // Sub Storage
                foreach (var storage in storages.SelectMany(
                             managedStorage => StorageHelper.FromStorage(managedStorage, excluded)))
                {
                    yield return storage;
                }
            }

            foreach (var storage in GetAll())
            {
                foreach (var (predicate, type) in StorageHelper.Instance!.StorageTypes)
                {
                    if (!predicate(storage.Context))
                    {
                        continue;
                    }

                    storage.Type = type;
                    break;
                }

                yield return storage;
            }
        }
    }

    /// <summary>
    ///     Gets all placed storages in the current location.
    /// </summary>
    public static IEnumerable<IStorageObject> CurrentLocation
    {
        get
        {
            var excluded = new HashSet<object>();
            foreach (var storage in StorageHelper.Instance!.FromLocation(Game1.currentLocation, excluded))
            {
                foreach (var (predicate, type) in StorageHelper.Instance.StorageTypes)
                {
                    if (!predicate(storage.Context))
                    {
                        continue;
                    }

                    storage.Type = type;
                    break;
                }

                yield return storage;
            }
        }
    }

    /// <summary>
    ///     Gets storages in the farmer's inventory.
    /// </summary>
    public static IEnumerable<IStorageObject> Inventory
    {
        get
        {
            var excluded = new HashSet<object>();
            foreach (var storage in StorageHelper.FromPlayer(Game1.player, excluded))
            {
                foreach (var (predicate, type) in StorageHelper.Instance!.StorageTypes)
                {
                    if (!predicate(storage.Context))
                    {
                        continue;
                    }

                    storage.Type = type;
                    break;
                }

                yield return storage;
            }
        }
    }

    /// <summary>
    ///     Gets the types of storages in the game.
    /// </summary>
    public static Dictionary<string, StorageData> Types { get; } = new();

    /// <summary>
    ///     Gets all placed storages in the world.
    /// </summary>
    public static IEnumerable<IStorageObject> World
    {
        get
        {
            var excluded = new HashSet<object>();
            foreach (var location in LocationHelper.AllLocations)
            {
                foreach (var storage in StorageHelper.Instance!.FromLocation(location, excluded))
                {
                    foreach (var (predicate, type) in StorageHelper.Instance.StorageTypes)
                    {
                        if (!predicate(storage.Context))
                        {
                            continue;
                        }

                        storage.Type = type;
                        break;
                    }

                    yield return storage;
                }
            }
        }
    }

    private static Dictionary<object, IStorageObject> ReferenceContext => StorageHelper.ReferenceContextLazy.Value;

    private Dictionary<Func<object, bool>, IStorageData> StorageTypes { get; }

    /// <summary>
    ///     Gets all storages placed in a particular farmer's inventory.
    /// </summary>
    /// <param name="player">The farmer to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all held storages in the farmer's inventory.</returns>
    public static IEnumerable<IStorageObject> FromPlayer(Farmer player, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(player))
        {
            yield break;
        }

        excluded.Add(player);

        // Mod Integrations
        foreach (var storage in IntegrationHelper.FromPlayer(player, excluded))
        {
            yield return storage;
        }

        var position = player.getTileLocation();
        for (var index = 0; index < player.MaxItems; index++)
        {
            var item = player.Items[index];
            if (!StorageHelper.TryGetOne(item, player, position, out var storage) || excluded.Contains(storage.Context))
            {
                continue;
            }

            excluded.Add(storage.Context);
            yield return storage;
        }
    }

    /// <summary>
    ///     Initialized <see cref="StorageHelper" />.
    /// </summary>
    /// <param name="config">Mod config data.</param>
    /// <param name="storageTypes">A dictionary of all registered storage types.</param>
    /// <returns>Returns an instance of the <see cref="StorageHelper" /> class.</returns>
    public static StorageHelper Init(ModConfig config, Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        return StorageHelper.Instance ??= new(config, storageTypes);
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

        if (!IntegrationHelper.TryGetOne(context, out storage)
         && !StorageHelper.TryGetOne(context, default, default, out storage))
        {
            return false;
        }

        foreach (var (predicate, storageType) in StorageHelper.Instance!.StorageTypes)
        {
            if (!predicate(storage.Context))
            {
                continue;
            }

            storage.Type = storageType;
            return true;
        }

        return true;
    }

    /// <summary>
    ///     Gets all storages placed in a particular location.
    /// </summary>
    /// <param name="location">The location to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all placed storages at the location.</returns>
    public IEnumerable<IStorageObject> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(location))
        {
            yield break;
        }

        excluded.Add(location);

        // Mod Integrations
        foreach (var storage in IntegrationHelper.FromLocation(location, excluded))
        {
            yield return storage;
        }

        // Special Locations
        switch (location)
        {
            case FarmHouse { fridge.Value: { } fridge } farmHouse
                when !excluded.Contains(fridge) && !farmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(fridge);
                yield return new FridgeStorage(
                    farmHouse,
                    this._config.DefaultChest,
                    farmHouse.fridgePosition.ToVector2());
                break;
            case IslandFarmHouse { fridge.Value: { } fridge } islandFarmHouse when !excluded.Contains(fridge)
             && !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                excluded.Add(fridge);
                yield return new FridgeStorage(
                    islandFarmHouse,
                    this._config.DefaultChest,
                    islandFarmHouse.fridgePosition.ToVector2());
                break;
            case IslandWest islandWest:
                excluded.Add(islandWest);
                yield return new ShippingBinStorage(
                    islandWest,
                    this._config.DefaultChest,
                    islandWest.shippingBinPosition.ToVector2());
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
                            this._config.DefaultChest,
                            new(
                                building.tileX.Value + building.tilesWide.Value / 2,
                                building.tileY.Value + building.tilesHigh.Value / 2));
                        break;
                    case ShippingBin shippingBin when !excluded.Contains(shippingBin):
                        excluded.Add(shippingBin);
                        yield return new ShippingBinStorage(
                            shippingBin,
                            location,
                            this._config.DefaultChest,
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
            if (!StorageHelper.TryGetOne(obj, location, position, out var subStorage)
             || excluded.Contains(subStorage.Context))
            {
                continue;
            }

            excluded.Add(subStorage.Context);
            yield return subStorage;
        }
    }

    private static IEnumerable<IStorageObject> FromStorage(IStorageObject storage, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();
        if (excluded.Contains(storage.Context))
        {
            yield break;
        }

        excluded.Add(storage.Context);
        var managedStorages = new List<IStorageObject>();

        foreach (var item in storage.Items.Where(item => item is not null && !excluded.Contains(item)))
        {
            if (!StorageHelper.TryGetOne(item, storage.Parent, storage.Position, out var managedStorage)
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
                     managedStorage => StorageHelper.FromStorage(managedStorage, excluded)))
        {
            yield return subStorage;
        }
    }

    private static Dictionary<object, IStorageObject> InitReferenceContext()
    {
        var referenceContext = new Dictionary<object, IStorageObject>();
        foreach (var location in LocationHelper.AllLocations)
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
                                StorageHelper.Instance!._config.DefaultChest,
                                new(
                                    shippingBin.tileX.Value + shippingBin.tilesWide.Value / 2,
                                    shippingBin.tileY.Value + shippingBin.tilesHigh.Value / 2)));
                    }

                    break;

                // Fridge
                case FarmHouse { fridge.Value: { } fridge, fridgePosition: var fridgePosition } farmHouse
                    when !referenceContext.ContainsKey(fridge) && !fridgePosition.Equals(Point.Zero):
                    referenceContext.Add(
                        fridge,
                        new FridgeStorage(
                            farmHouse,
                            StorageHelper.Instance!._config.DefaultChest,
                            fridgePosition.ToVector2()));
                    break;

                // Island Fridge
                case IslandFarmHouse
                    {
                        fridge.Value: { } islandFridge, fridgePosition: var islandFridgePosition,
                    } islandFarmHouse when !referenceContext.ContainsKey(islandFridge)
                                        && !islandFridgePosition.Equals(Point.Zero):
                    referenceContext.Add(
                        islandFridge,
                        new FridgeStorage(
                            islandFarmHouse,
                            StorageHelper.Instance!._config.DefaultChest,
                            islandFridgePosition.ToVector2()));
                    break;
            }
        }

        return referenceContext;
    }

    private static bool TryGetOne(
        object? context,
        object? parent,
        Vector2 position,
        [NotNullWhen(true)] out IStorageObject? storage)
    {
        if (context is not null && StorageHelper.ReferenceContext.TryGetValue(context, out storage))
        {
            return true;
        }

        switch (context)
        {
            case IStorageObject storageObject:
                storage = storageObject;
                return true;
            case Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } shippingChest:
                storage = new ShippingBinStorage(
                    shippingChest,
                    parent,
                    StorageHelper.Instance!._config.DefaultChest,
                    position);
                return true;
            case Chest { playerChest.Value: true } chest:
                storage = new ChestStorage(chest, parent, StorageHelper.Instance!._config.DefaultChest, position);
                return true;
            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest } heldObj:
                storage = new ObjectStorage(heldObj, parent, StorageHelper.Instance!._config.DefaultChest, position);
                return true;
            case ShippingBin shippingBin:
                storage = new ShippingBinStorage(
                    shippingBin,
                    parent,
                    StorageHelper.Instance!._config.DefaultChest,
                    position);
                return true;
            case JunimoHut junimoHut:
                storage = new JunimoHutStorage(
                    junimoHut,
                    parent,
                    StorageHelper.Instance!._config.DefaultChest,
                    position);
                return true;
            case FarmHouse { fridge.Value: { } } farmHouse when !farmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(farmHouse, StorageHelper.Instance!._config.DefaultChest, position);
                return true;
            case IslandFarmHouse { fridge.Value: { } } islandFarmHouse
                when !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                storage = new FridgeStorage(islandFarmHouse, StorageHelper.Instance!._config.DefaultChest, position);
                return true;
            case IslandWest islandWest:
                storage = new ShippingBinStorage(islandWest, StorageHelper.Instance!._config.DefaultChest, position);
                return true;
            default:
                storage = default;
                return false;
        }
    }
}