namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using StardewMods.FuryCore.Models.GameObjects.Producers;
using StardewMods.FuryCore.Models.GameObjects.Storages;
using StardewMods.FuryCore.Models.GameObjects.Terrains;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

/// <inheritdoc cref="IGameObjects" />
[FuryCoreService(true)]
internal class GameObjects : IGameObjects, IModService
{
    private readonly PerScreen<IDictionary<object, IGameObject>> _cachedObjects = new(() => new Dictionary<object, IGameObject>());
    private readonly PerScreen<IDictionary<object, object>> _contextMap = new(() => new Dictionary<object, object>());
    private readonly GameObjectsRemoved _gameObjectsRemoved;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjects" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public GameObjects(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this._gameObjectsRemoved = new(helper.Events, services);
    }

    /// <inheritdoc />
    public event EventHandler<IGameObjectsRemovedEventArgs> GameObjectsRemoved
    {
        add => this._gameObjectsRemoved.Add(value);
        remove => this._gameObjectsRemoved.Remove(value);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<InventoryItem, IGameObject>> InventoryItems
    {
        get
        {
            IList<object> exclude = new List<object>();
            foreach (var getInventoryItems in this.ExternalInventoryItems)
            {
                IEnumerable<(int, object)> inventoryItems = null;
                try
                {
                    inventoryItems = getInventoryItems.Invoke(Game1.player);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (inventoryItems is null)
                {
                    continue;
                }

                foreach (var (index, context) in inventoryItems)
                {
                    if (exclude.Contains(context) || !this.TryGetGameObject(context, true, out var gameObject))
                    {
                        continue;
                    }

                    exclude.Add(context);
                    yield return new(new(Game1.player, index), gameObject);
                }
            }

            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                var item = Game1.player.Items[index];
                if (item is null || exclude.Contains(item) || !this.TryGetGameObject(item, true, out var gameObject))
                {
                    continue;
                }

                exclude.Add(item);
                yield return new(new(Game1.player, index), gameObject);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<LocationObject, IGameObject>> LocationObjects
    {
        get
        {
            IList<object> exclude = new List<object>();
            foreach (var location in this.AccessibleLocations)
            {
                foreach (var getLocationObjects in this.ExternalLocationObjects)
                {
                    IEnumerable<(Vector2, object)> locationObjects = null;
                    try
                    {
                        locationObjects = getLocationObjects.Invoke(location);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (locationObjects is null)
                    {
                        continue;
                    }

                    foreach (var (position, context) in locationObjects)
                    {
                        if (exclude.Contains(context) || !this.TryGetGameObject(context, true, out var gameObject))
                        {
                            continue;
                        }

                        exclude.Add(context);
                        yield return new(new(location, position), gameObject);
                    }
                }
            }

            foreach (var location in this.AccessibleLocations)
            {
                switch (location)
                {
                    // Storages from BuildableGameLocation.buildings
                    case BuildableGameLocation buildableGameLocation:
                        foreach (var building in buildableGameLocation.buildings.Where(building => !exclude.Contains(building)))
                        {
                            if (!this.TryGetGameObject(building, true, out var buildingObject))
                            {
                                continue;
                            }

                            exclude.Add(building);
                            yield return new(new(location, new(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2)), buildingObject);
                        }

                        break;

                    // Storage from FarmHouse.fridge.Value
                    case FarmHouse farmHouse:
                        if (farmHouse.fridge.Value is null || exclude.Contains(farmHouse.fridge.Value) || farmHouse.fridgePosition.Equals(Point.Zero) || !this.TryGetGameObject(farmHouse, true, out var farmHouseObject))
                        {
                            break;
                        }

                        exclude.Add(farmHouse.fridge.Value);
                        yield return new(new(location, farmHouse.fridgePosition.ToVector2()), farmHouseObject);
                        break;

                    // Storage from IslandFarmHouse.fridge.Value
                    case IslandFarmHouse islandFarmHouse:
                        if (islandFarmHouse.fridge.Value is null || exclude.Contains(islandFarmHouse.fridge.Value) || islandFarmHouse.fridgePosition.Equals(Point.Zero) || !this.TryGetGameObject(islandFarmHouse, true, out var islandFarmHouseObject))
                        {
                            break;
                        }

                        exclude.Add(islandFarmHouse.fridge.Value);
                        yield return new(new(location, islandFarmHouse.fridgePosition.ToVector2()), islandFarmHouseObject);
                        break;

                    // Island Farm
                    case IslandWest islandWest:
                        if (exclude.Contains(islandWest) || !this.TryGetGameObject(islandWest, true, out var islandWestObject))
                        {
                            break;
                        }

                        exclude.Add(islandWest);
                        yield return new(new(location, islandWest.shippingBinPosition.ToVector2()), islandWestObject);
                        break;
                }

                // Storages from GameLocation.Objects
                foreach (var (position, obj) in location.Objects.Pairs)
                {
                    if (exclude.Contains(obj) || !this.TryGetGameObject(obj, true, out var gameObject))
                    {
                        continue;
                    }

                    exclude.Add(obj);
                    yield return new(new(location, position), gameObject);
                }

                // Large terrain features
                foreach (var largeTerrainFeature in location.largeTerrainFeatures.OfType<Bush>())
                {
                    if (exclude.Contains(largeTerrainFeature) || !this.TryGetGameObject(largeTerrainFeature, true, out var gameObject))
                    {
                        continue;
                    }

                    exclude.Add(largeTerrainFeature);
                    yield return new(new(location, largeTerrainFeature.tilePosition.Value), gameObject);
                }

                // Terrain features
                foreach (var (position, terrainFeature) in location.terrainFeatures.Pairs)
                {
                    if (terrainFeature is not HoeDirt or FruitTree || exclude.Contains(terrainFeature) || !this.TryGetGameObject(terrainFeature, true, out var gameObject))
                    {
                        continue;
                    }

                    exclude.Add(terrainFeature);
                    yield return new(new(location, position), gameObject);
                }
            }
        }
    }

    private IEnumerable<GameLocation> AccessibleLocations
    {
        get => Context.IsMainPlayer
            ? Game1.locations.Concat(
                from location in Game1.locations.OfType<BuildableGameLocation>()
                from building in location.buildings
                where building.indoors.Value is not null
                select building.indoors.Value)
            : this.Helper.Multiplayer.GetActiveLocations();
    }

    private IDictionary<object, IGameObject> CachedObjects
    {
        get => this._cachedObjects.Value;
    }

    private IDictionary<object, object> ContextMap
    {
        get => this._contextMap.Value;
    }

    private IList<Func<Farmer, IEnumerable<(int Index, object Context)>>> ExternalInventoryItems { get; } = new List<Func<Farmer, IEnumerable<(int Index, object Context)>>>();

    private IList<Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>>> ExternalLocationObjects { get; } = new List<Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>>>();

    private IModHelper Helper { get; }

    /// <inheritdoc />
    public void AddInventoryItemsGetter(Func<Farmer, IEnumerable<(int Index, object Context)>> getInventoryItems)
    {
        this.ExternalInventoryItems.Add(getInventoryItems);
    }

    /// <inheritdoc />
    public void AddLocationObjectsGetter(Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>> getLocationObjects)
    {
        this.ExternalLocationObjects.Add(getLocationObjects);
    }

    /// <inheritdoc />
    public bool TryGetGameObject(object context, out IGameObject gameObject)
    {
        if (this.TryGetGameObject(context, false, out gameObject))
        {
            return gameObject is not null;
        }

        gameObject ??= this.InventoryItems.FirstOrDefault(inventoryItem => ReferenceEquals(inventoryItem.Value.Context, context)).Value;
        gameObject ??= this.LocationObjects.FirstOrDefault(locationObject => ReferenceEquals(locationObject.Value.Context, context)).Value;
        return gameObject is not null || this.TryGetGameObject(context, true, out gameObject);
    }

    /// <summary>
    ///     Clears all cached objects.
    /// </summary>
    /// <returns>A list of <see cref="IGameObject" /> which were purged.</returns>
    internal IEnumerable<IGameObject> PurgeCache()
    {
        var gameObjects = this.CachedObjects.Values.Where(gameObject => gameObject is not null).ToList();
        foreach (var gameObject in this.InventoryItems.Select(inventoryItem => inventoryItem.Value))
        {
            gameObjects.Remove(gameObject);
        }

        foreach (var gameObject in this.LocationObjects.Select(locationObject => locationObject.Value))
        {
            gameObjects.Remove(gameObject);
        }

        var contexts = (
            from cachedObject in this.CachedObjects
            join gameObject in gameObjects on cachedObject.Value equals gameObject
            select cachedObject.Key).ToList();
        foreach (var context in contexts)
        {
            this.CachedObjects.Remove(context);

            var contextMaps = (
                from contextMap in this.ContextMap
                where contextMap.Value.Equals(context)
                select contextMap.Key).ToList();
            foreach (var contextMap in contextMaps)
            {
                this.ContextMap.Remove(contextMap);
            }
        }

        return gameObjects.Any() ? gameObjects : null;
    }

    private bool TryGetGameObject(object context, bool init, out IGameObject gameObject)
    {
        if (this.ContextMap.TryGetValue(context, out var actualContext))
        {
            context = actualContext;
        }

        if (this.CachedObjects.TryGetValue(context, out gameObject))
        {
            return !init || gameObject is not null;
        }

        if (!init)
        {
            return false;
        }

        switch (context)
        {
            // GameLocation
            case FarmHouse { fridge.Value: { } fridge } farmHouse:
                this.ContextMap[fridge] = context;
                gameObject = new StorageFridge(farmHouse);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case IslandFarmHouse { fridge.Value: { } islandFridge } islandFarmHouse:
                this.ContextMap[islandFridge] = context;
                gameObject = new StorageFridge(islandFarmHouse);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case IslandWest islandWest:
                gameObject = new StorageShippingBin(islandWest);
                this.CachedObjects.Add(context, gameObject);
                return true;

            // Building
            case JunimoHut { output.Value: { } junimoHutChest } junimoHut:
                this.ContextMap[junimoHutChest] = context;
                gameObject = new StorageJunimoHut(junimoHut);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case ShippingBin shippingBin:
                this.ContextMap[Game1.getFarm()] = context;
                gameObject = new StorageShippingBin(shippingBin);
                this.CachedObjects.Add(context, gameObject);
                return true;

            // TerrainFeatures
            case Bush bush:
                gameObject = new TerrainBush(bush);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case FruitTree fruitTree:
                gameObject = new TerrainFruitTree(fruitTree);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case HoeDirt hoeDirt:
                gameObject = new TerrainHoeDirt(hoeDirt);
                this.CachedObjects.Add(context, gameObject);
                return true;

            // Objects
            case Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } chest:
                gameObject = new StorageShippingBin(chest);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case Chest chest:
                gameObject = new StorageChest(chest);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest heldChest } obj:
                this.ContextMap[heldChest] = context;
                gameObject = new StorageObject(obj);
                this.CachedObjects.Add(context, gameObject);
                return true;

            case SObject { bigCraftable.Value: true } obj when obj is CrabPot || Enum.IsDefined(typeof(VanillaProducerObjects), obj.ParentSheetIndex):
                gameObject = new GenericProducer(obj);
                this.CachedObjects.Add(context, gameObject);
                return true;

            default:
                this.CachedObjects.Add(context, null);
                return false;
        }
    }
}