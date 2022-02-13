namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
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
                if (this.TryGetManagedStorage(gameObject, out var managedStorage))
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
                if (this.TryGetManagedStorage(gameObject, out var managedStorage))
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
    ///     Attempts to find the ManagedStorage that matches a context.
    /// </summary>
    /// <param name="context">The context object to find.</param>
    /// <param name="managedStorage">The <see cref="IManagedStorage" /> to return if it matches the context object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedStorage" /> could be found.</returns>
    public bool FindManagedStorage(object context, out IManagedStorage managedStorage)
    {
        if (context is null)
        {
            managedStorage = null;
            return false;
        }

        if (this.GameObjects.TryGetGameObject(context, out var gameObject) && this.TryGetManagedStorage(gameObject, out managedStorage))
        {
            return true;
        }

        managedStorage = this.InventoryStorages.FirstOrDefault(playerStorage => ReferenceEquals(playerStorage.Value.Context, context)).Value;
        if (managedStorage is not null)
        {
            return true;
        }

        managedStorage = this.LocationStorages.FirstOrDefault(placedStorage => ReferenceEquals(placedStorage.Value.Context, context)).Value;
        if (managedStorage is not null)
        {
            return true;
        }

        managedStorage = null;
        return false;
    }

    private bool TryGetManagedStorage(IGameObject gameObject, out IManagedStorage managedStorage)
    {
        if (this.CachedObjects.TryGetValue(gameObject, out managedStorage))
        {
            return managedStorage is not null;
        }

        if (gameObject is not IStorageContainer storageContainer)
        {
            return false;
        }

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

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (!this.ChestConfigs.TryGetValue(name, out var storageConfig))
        {
            if (!this.Assets.ChestData.TryGetValue(name, out var storageData))
            {
                storageData = new StorageData();
                this.Assets.AddChestData(name, storageData);
            }

            storageConfig = this.ChestConfigs[name] = new StorageModel(storageData, this.Config.DefaultChest);
        }

        managedStorage = new ManagedStorage(storageContainer, storageConfig, name);
        this.CachedObjects.Add(gameObject, managedStorage);
        return true;
    }
}