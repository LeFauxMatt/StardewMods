#nullable disable

namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.BetterChests.Models.Config;
using StardewMods.BetterChests.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedObjects : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<IDictionary<IGameObject, IManagedStorage>> _cachedObjects = new(() => new Dictionary<IGameObject, IManagedStorage>());
    private readonly Lazy<IGameObjects> _gameObjects;
    private readonly Lazy<ModIntegrations> _modIntegrations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedObjects" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ManagedObjects(IConfigModel config, IModServices services)
    {
        this.Config = config;
        this._assetHandler = services.Lazy<AssetHandler>();
        this._gameObjects = services.Lazy<IGameObjects>(gameObjects => gameObjects.GameObjectsRemoved += this.OnGameObjectsRemoved);
        this._modIntegrations = services.Lazy<ModIntegrations>();
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
                if (!this.Integrations.OverrideObject(locationObject, gameObject) && this.TryGetManagedStorage(gameObject, out var managedStorage))
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

    private ModIntegrations Integrations
    {
        get => this._modIntegrations.Value;
    }

    /// <summary>
    ///     Attempts to find the managed storage that matches a context.
    /// </summary>
    /// <param name="context">The context object to find.</param>
    /// <param name="managedStorage">The <see cref="IManagedStorage" /> to return if it matches the context object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedStorage" /> could be found.</returns>
    public bool TryGetManagedStorage(object context, out IManagedStorage managedStorage)
    {
        if (context is null || !this.GameObjects.TryGetGameObject(context, out var gameObject))
        {
            managedStorage = null;
            return false;
        }

        if (this.CachedObjects.TryGetValue(gameObject, out managedStorage))
        {
            return managedStorage is not null;
        }

        if (this.TryGetManagedStorage(gameObject, out managedStorage))
        {
            return managedStorage is not null;
        }

        managedStorage = this.InventoryStorages.FirstOrDefault(playerStorage => ReferenceEquals(playerStorage.Value.Context, context)).Value;
        managedStorage ??= this.LocationStorages.FirstOrDefault(placedStorage => ReferenceEquals(placedStorage.Value.Context, context)).Value;
        return managedStorage is not null;
    }

    /// <summary>
    ///     Attempts to find the managed storage that matches a game object.
    /// </summary>
    /// <param name="gameObject">The game object to find a managed storage for.</param>
    /// <param name="managedStorage">The <see cref="IManagedStorage" /> to return if it matches the game object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedStorage" /> could be found.</returns>
    [SuppressMessage("StyleCop", "SA1012", Justification = "Conflicts with SA1008")]
    public bool TryGetManagedStorage(IGameObject gameObject, out IManagedStorage managedStorage)
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
            SObject obj and ({ ParentSheetIndex: 165 } or Chest) => this.Assets.GetStorageName(obj),
            Building building => this.Assets.GetStorageName(building),
            GameLocation location => this.Assets.GetStorageName(location),
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

    private void OnGameObjectsRemoved(object sender, IGameObjectsRemovedEventArgs e)
    {
        foreach (var gameObject in e.Removed)
        {
            this.CachedObjects.Remove(gameObject);
        }
    }
}