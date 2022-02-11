namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.BetterChests.Models.Config;
using StardewMods.BetterChests.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedObjects : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<IDictionary<IGameObject, IManagedStorage>> _cachedObjects = new(() => new Dictionary<IGameObject, IManagedStorage>());
    private readonly Lazy<IGameObjects> _gameObjects;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedObjects" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ManagedObjects(IConfigModel config, IModServices services)
    {
        this.Config = config;
        this._assetHandler = services.Lazy<AssetHandler>();
        this._gameObjects = services.Lazy<IGameObjects>();
    }

    /// <summary>
    ///     Gets all storages in player inventory.
    /// </summary>
    public IEnumerable<KeyValuePair<InventoryItem, IManagedStorage>> InventoryStorages
    {
        get
        {
            foreach (var (inventoryItem, gameObject) in this.GameObjects.InventoryItems)
            {
                if (gameObject is not IStorageContainer storageContainer)
                {
                    continue;
                }

                if (!this.CachedObjects.TryGetValue(gameObject, out var managedStorage))
                {
                    var name = gameObject.Context switch
                    {
                        Chest chest => this.Assets.GetStorageName(chest),
                        SObject obj => this.Assets.GetStorageName(obj),
                        JunimoHut => "Junimo Hut",
                        FarmHouse => "Fridge",
                        IslandFarmHouse => "Fridge",
                        _ => null,
                    };

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var storageData = this.GetData(name);
                        managedStorage = new ManagedStorage(storageContainer, storageData, name);
                        this.CachedObjects.Add(gameObject, managedStorage);
                    }
                }

                if (managedStorage is not null)
                {
                    yield return new(inventoryItem, managedStorage);
                }
            }
        }
    }

    /// <summary>
    ///     Gets all storages placed in a game location.
    /// </summary>
    public IEnumerable<KeyValuePair<LocationObject, IManagedStorage>> LocationStorages
    {
        get
        {
            foreach (var (locationObject, gameObject) in this.GameObjects.LocationObjects)
            {
                if (gameObject is not IStorageContainer storageContainer)
                {
                    continue;
                }

                if (!this.CachedObjects.TryGetValue(gameObject, out var managedStorage))
                {
                    var name = gameObject.Context switch
                    {
                        Chest chest => this.Assets.GetStorageName(chest),
                        SObject obj => this.Assets.GetStorageName(obj),
                        JunimoHut => "Junimo Hut",
                        FarmHouse => "Fridge",
                        IslandFarmHouse => "Fridge",
                        _ => null,
                    };

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var storageData = this.GetData(name);
                        managedStorage = new ManagedStorage(storageContainer, storageData, name);
                        this.CachedObjects.Add(gameObject, managedStorage);
                    }
                }

                if (managedStorage is not null)
                {
                    yield return new(locationObject, managedStorage);
                }
            }
        }
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IDictionary<IGameObject, IManagedStorage> CachedObjects
    {
        get => this._cachedObjects.Value;
    }

    private IDictionary<string, IStorageData> ChestConfigs { get; } = new Dictionary<string, IStorageData>();

    private IConfigModel Config { get; }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    /// <summary>
    ///     Attempts to find a ManagedStorage that matches a storage context instance.
    /// </summary>
    /// <param name="context">The context object to find.</param>
    /// <param name="managedStorage">The <see cref="IManagedStorage" /> to return if it matches the context object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedStorage" /> could be found.</returns>
    public bool TryGetManagedStorage(object context, out IManagedStorage managedStorage)
    {
        if (context is null)
        {
            managedStorage = null;
            return false;
        }

        if (this.GameObjects.TryGetGameObject(context, out var gameObject) && gameObject is IStorageContainer storageContainer)
        {
            if (!this.CachedObjects.TryGetValue(gameObject, out managedStorage))
            {
                var name = gameObject.Context switch
                {
                    Chest chest => this.Assets.GetStorageName(chest),
                    SObject { ParentSheetIndex: 165 } obj => this.Assets.GetStorageName(obj),
                    JunimoHut => "Junimo Hut",
                    FarmHouse => "Fridge",
                    IslandFarmHouse => "Fridge",
                    ShippingBin => "Shipping Bin",
                    _ => null,
                };

                if (!string.IsNullOrWhiteSpace(name))
                {
                    var storageData = this.GetData(name);
                    managedStorage = new ManagedStorage(storageContainer, storageData, name);
                    this.CachedObjects.Add(gameObject, managedStorage);
                }
            }

            return managedStorage is not null;
        }

        foreach (var (_, playerStorage) in this.InventoryStorages)
        {
            if (ReferenceEquals(playerStorage.Context, context))
            {
                managedStorage = playerStorage;
                return true;
            }
        }

        foreach (var (_, placedStorage) in this.LocationStorages)
        {
            if (ReferenceEquals(placedStorage.Context, context))
            {
                managedStorage = placedStorage;
                return true;
            }
        }

        managedStorage = null;
        return false;
    }

    private IStorageData GetData(string name)
    {
        if (!this.ChestConfigs.TryGetValue(name, out var config))
        {
            if (!this.Assets.ChestData.TryGetValue(name, out var chestData))
            {
                chestData = new StorageData();
                this.Assets.AddChestData(name, chestData);
            }

            config = this.ChestConfigs[name] = new StorageModel(chestData, this.Config.DefaultChest);
        }

        return config;
    }
}