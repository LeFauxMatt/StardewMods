namespace StardewMods.EasyAccess.Services;

using System;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Interfaces.ManagedObjects;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
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
        this._gameObjects = services.Lazy<IGameObjects>();
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
                if (gameObject is not IProducer producer)
                {
                    continue;
                }

                if (!this.CachedObjects.TryGetValue(gameObject, out var managedProducer))
                {
                    var name = gameObject.Context switch
                    {
                        SObject obj => obj.Name,
                        _ => null,
                    };

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var producerData = this.GetData(name);
                        managedProducer = new ManagedProducer(producer, producerData, name);
                        this.CachedObjects.Add(gameObject, managedProducer);
                    }
                }

                if (managedProducer is not null)
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

    private IProducerData GetData(string name)
    {
        if (!this.ProducerConfigs.TryGetValue(name, out var config))
        {
            if (!this.Assets.ProducerData.TryGetValue(name, out var producerData))
            {
                producerData = new ProducerData();
                this.Assets.AddProducerData(name, producerData);
            }

            config = this.ProducerConfigs[name] = new ProducerModel(producerData, this.Config.DefaultProducer);
        }

        return config;
    }
}