namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using StardewMods.FuryCore.Models.GameObjects.Producers;
using StardewMods.FuryCore.Models.GameObjects.Storages;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc cref="IGameObjects" />
[FuryCoreService(true)]
internal class GameObjects : IGameObjects, IModService
{
    private readonly PerScreen<IDictionary<object, IGameObject>> _cachedObjects = new(() => new Dictionary<object, IGameObject>());
    private readonly PerScreen<IDictionary<object, object>> _contextMap = new(() => new Dictionary<object, object>());

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjects" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    public GameObjects(IModHelper helper)
    {
        this.Helper = helper;
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
                    if (exclude.Contains(context) || !this.TryGetGameObject(context, out var gameObject))
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
                if (item is null || exclude.Contains(item) || !this.TryGetGameObject(item, out var gameObject))
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
                        if (exclude.Contains(context) || !this.TryGetGameObject(context, out var gameObject))
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
                        foreach (var building in buildableGameLocation.buildings)
                        {
                            if (exclude.Contains(building) || !this.TryGetGameObject(building, out var buildingObject))
                            {
                                continue;
                            }

                            exclude.Add(building);
                            yield return new(new(location, new(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2)), buildingObject);
                        }

                        break;

                    // Storage from FarmHouse.fridge.Value
                    case FarmHouse farmHouse when farmHouse.fridge.Value is not null && !farmHouse.fridgePosition.Equals(Point.Zero):
                        if (exclude.Contains(farmHouse) || !this.TryGetGameObject(farmHouse, out var farmHouseObject))
                        {
                            break;
                        }

                        exclude.Add(farmHouse);
                        yield return new(new(location, farmHouse.fridgePosition.ToVector2()), farmHouseObject);
                        break;

                    // Storage from IslandFarmHouse.fridge.Value
                    case IslandFarmHouse islandFarmHouse when islandFarmHouse.fridge.Value is not null && !islandFarmHouse.fridgePosition.Equals(Point.Zero):
                        if (exclude.Contains(islandFarmHouse) || !this.TryGetGameObject(islandFarmHouse, out var islandFarmHouseObject))
                        {
                            break;
                        }

                        exclude.Add(islandFarmHouse);
                        yield return new(new(location, islandFarmHouse.fridgePosition.ToVector2()), islandFarmHouseObject);
                        break;
                }

                // Storages from GameLocation.Objects
                foreach (var (position, obj) in location.Objects.Pairs)
                {
                    if (exclude.Contains(obj) || !this.TryGetGameObject(obj, out var gameObject))
                    {
                        continue;
                    }

                    exclude.Add(obj);
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

    private IList<IGameObjects.GetInventoryItems> ExternalInventoryItems { get; } = new List<IGameObjects.GetInventoryItems>();

    private IList<IGameObjects.GetLocationObjects> ExternalLocationObjects { get; } = new List<IGameObjects.GetLocationObjects>();

    private IModHelper Helper { get; }

    /// <inheritdoc />
    public void AddInventoryItemsGetter(IGameObjects.GetInventoryItems getInventoryItems)
    {
        this.ExternalInventoryItems.Add(getInventoryItems);
    }

    /// <inheritdoc />
    public void AddLocationObjectsGetter(IGameObjects.GetLocationObjects getLocationObjects)
    {
        this.ExternalLocationObjects.Add(getLocationObjects);
    }

    /// <inheritdoc />
    public bool TryGetGameObject(object context, out IGameObject gameObject)
    {
        if (this.ContextMap.TryGetValue(context, out var actualContext))
        {
            context = actualContext;
        }

        if (this.CachedObjects.TryGetValue(context, out gameObject))
        {
            return gameObject is not null;
        }

        switch (context)
        {
            case Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } chest:
                gameObject = new StorageShippingBin(chest);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case Chest chest:
                gameObject = new StorageChest(chest);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest heldChest }:
                this.ContextMap[heldChest] = context;
                gameObject = new StorageChest(heldChest, context);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case FarmHouse { fridge.Value: { } fridge } farmHouse:
                this.ContextMap[fridge] = context;
                gameObject = new StorageChest(fridge, context, () => farmHouse.modData);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case IslandFarmHouse { fridge.Value: { } islandFridge } islandFarmHouse:
                this.ContextMap[islandFridge] = context;
                gameObject = new StorageChest(islandFridge, context, () => islandFarmHouse.modData);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case JunimoHut { output.Value: { } junimoHutChest } junimoHut:
                this.ContextMap[junimoHutChest] = context;
                gameObject = new StorageChest(junimoHutChest, context, () => junimoHut.modData);
                this.CachedObjects.Add(context, gameObject);
                return true;
            case ShippingBin shippingBin:
                this.ContextMap[Game1.getFarm()] = context;
                gameObject = new StorageShippingBin(shippingBin);
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
}