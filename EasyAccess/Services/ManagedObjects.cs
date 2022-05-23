#nullable disable

namespace StardewMods.EasyAccess.Services;

using System;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Interfaces.ManagedObjects;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedObjects : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<IDictionary<IGameObject, IManagedProducer>> _cachedObjects = new(() => new Dictionary<IGameObject, IManagedProducer>());
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
        this._gameObjects = services.Lazy<IGameObjects>(gameObjects => gameObjects.GameObjectsRemoved += this.OnGameObjectsRemoved);
    }

    /// <summary>
    ///     Gets all producers placed in a game location.
    /// </summary>
    public IEnumerable<KeyValuePair<LocationObject, IManagedProducer>> Producers
    {
        get
        {
            foreach (var (locationObject, gameObject) in this.GameObjects.LocationObjects)
            {
                if (this.TryGetManagedProducer(gameObject, out var managedProducer))
                {
                    yield return new(locationObject, managedProducer);
                }
            }
        }
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IDictionary<IGameObject, IManagedProducer> CachedObjects
    {
        get => this._cachedObjects.Value;
    }

    private IConfigModel Config { get; }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IDictionary<string, IProducerData> ProducerConfigs { get; } = new Dictionary<string, IProducerData>();

    /// <summary>
    ///     Attempts to find the managed producer that matches a game object.
    /// </summary>
    /// <param name="gameObject">The game object to find a managed producer for.</param>
    /// <param name="managedProducer">The <see cref="IManagedProducer" /> to return if it matches the game object.</param>
    /// <returns>Returns true if a matching <see cref="IManagedProducer" /> could be found.</returns>
    public bool TryGetManagedProducer(IGameObject gameObject, out IManagedProducer managedProducer)
    {
        if (this.CachedObjects.TryGetValue(gameObject, out managedProducer))
        {
            return managedProducer is not null;
        }

        if (gameObject is not IProducer producer)
        {
            return false;
        }

        var name = gameObject.Context switch
        {
            SObject obj => this.Assets.GetProducerName(obj),
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (!this.ProducerConfigs.TryGetValue(name, out var producerConfig))
        {
            if (!this.Assets.ProducerData.TryGetValue(name, out var producerData))
            {
                producerData = new ProducerData();
                this.Assets.AddProducerData(name, producerData);
            }

            producerConfig = this.ProducerConfigs[name] = new ProducerModel(producerData, this.Config.DefaultProducer);
        }

        managedProducer = new ManagedProducer(producer, producerConfig, name);
        this.CachedObjects.Add(gameObject, managedProducer);
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