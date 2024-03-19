namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Buildings;
using StardewValley.GameData.BigCraftables;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class ContainerFactory : BaseService
{
    private readonly ConditionalWeakTable<object, IStorageContainer> cachedContainers = new();
    private readonly IModConfig modConfig;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly Dictionary<string, IStorageOptions> storageOptions = new();

    /// <summary>Initializes a new instance of the <see cref="ContainerFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public ContainerFactory(ILog log, IManifest manifest, IModConfig modConfig, ProxyChestFactory proxyChestFactory)
        : base(log, manifest)
    {
        this.modConfig = modConfig;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <summary>
    /// Retrieves all container items that satisfy the specified predicate, if provided. If no predicate is provided,
    /// returns all container items.
    /// </summary>
    /// <param name="predicate">Optional. A function that defines the conditions of the container items to search for.</param>
    /// <returns>An enumerable collection of IContainer items that satisfy the predicate, if provided.</returns>
    public IEnumerable<IStorageContainer> GetAll(Func<IStorageContainer, bool>? predicate = default)
    {
        var foundContainers = new HashSet<IStorageContainer>();
        var containerQueue = new Queue<IStorageContainer>();

        foreach (var container in this.GetAllFromPlayers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromLocations(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }

        foreach (var container in this.GetAllFromContainers(foundContainers, containerQueue, predicate))
        {
            yield return container;
        }
    }

    /// <summary>Retrieves all containers from the specified container that match the optional predicate.</summary>
    /// <param name="parentContainer">The container where the container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAllFromContainer(
        IStorageContainer parentContainer,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var item in parentContainer.Items)
        {
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(parentContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Retrieves all containers from the specified game location that match the optional predicate.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAllFromLocation(
        GameLocation location,
        Func<IStorageContainer, bool>? predicate = default)
    {
        // Search for containers from placed objects
        foreach (var obj in location.Objects.Values)
        {
            if (!this.TryGetAny(obj, out var container))
            {
                continue;
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }

        // Search for containers from buildings
        foreach (var building in location.buildings)
        {
            if (!building.buildingChests.Any())
            {
                continue;
            }

            if (!this.storageOptions.TryGetValue($"(B){building.buildingType.Value}", out var storageType))
            {
                storageType = new BuildingStorageOptions(this.modConfig.DefaultOptions, building.GetData());
                this.storageOptions.Add($"(B){building.buildingType.Value}", storageType);
            }

            foreach (var chest in building.buildingChests)
            {
                if (!this.cachedContainers.TryGetValue(chest, out var container))
                {
                    container = new BuildingContainer(storageType, building, chest);

                    this.cachedContainers.AddOrUpdate(chest, container);
                }

                if (predicate is null || predicate(container))
                {
                    yield return container;
                }
            }
        }

        // Get container for fridge in location
        if (location.GetFridge() is
            { } fridge)
        {
            if (!this.storageOptions.TryGetValue($"(L){location.Name}", out var storageType))
            {
                storageType = new LocationStorageOptions(this.modConfig.DefaultOptions, location.GetData());
                this.storageOptions.Add($"(L){location.Name}", storageType);
            }

            if (!this.cachedContainers.TryGetValue(fridge, out var container))
            {
                container = new FridgeContainer(storageType, location, fridge);

                this.cachedContainers.AddOrUpdate(fridge, container);
            }

            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Retrieves all container items from the specified player matching the optional predicate.</summary>
    /// <param name="farmer">The player whose container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAllFromPlayer(
        Farmer farmer,
        Func<IStorageContainer, bool>? predicate = default)
    {
        // Get container from farmer backpack
        if (!this.TryGetOne(farmer, out var farmerContainer))
        {
            yield break;
        }

        if (predicate is not null && predicate(farmerContainer))
        {
            yield return farmerContainer;
        }

        // Search for containers from farmer inventory
        foreach (var item in farmer.Items)
        {
            if (item is null || !this.TryGetAny(item, out var childContainer))
            {
                continue;
            }

            var container = new ChildContainer(farmerContainer, childContainer);
            if (predicate is null || predicate(container))
            {
                yield return container;
            }
        }
    }

    /// <summary>Tries to retrieve a container from the specified game location at the specified position.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="pos">The position of the game location where the container will be retrieved.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOneFromLocation(
        GameLocation location,
        Vector2 pos,
        [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (!location.Objects.TryGetValue(pos, out var obj))
        {
            container = null;
            return false;
        }

        if (this.cachedContainers.TryGetValue(obj, out container) || this.TryGetAny(obj, out container))
        {
            return true;
        }

        container = null;
        return false;
    }

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOneFromMenu([NotNullWhen(true)] out IStorageContainer? container)
    {
        if ((Game1.activeClickableMenu as ItemGrabMenu)?.context is not Chest chest)
        {
            container = null;
            return false;
        }

        if (chest.Location is not null && this.TryGetOneFromLocation(chest.Location, chest.TileLocation, out container))
        {
            return true;
        }

        if (chest == Game1.player.ActiveObject && this.TryGetOneFromPlayer(Game1.player, out container))
        {
            return true;
        }

        container = null;
        return false;
    }

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOneFromPlayer(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container, int index = -1)
    {
        var item = farmer.Items.ElementAtOrDefault(index) ?? farmer.ActiveObject;
        if (item is null || !this.TryGetOne(farmer, out var farmerContainer))
        {
            container = null;
            return false;
        }

        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        if (!this.TryGetAny(item, out var childContainer))
        {
            container = null;
            return false;
        }

        container = new ChildContainer(farmerContainer, childContainer);
        this.cachedContainers.AddOrUpdate(item, container);
        return true;
    }

    /// <summary>Tries to retrieve a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(farmer, out container))
        {
            return true;
        }

        container = new FarmerContainer(this.modConfig.DefaultOptions, farmer);
        this.cachedContainers.AddOrUpdate(farmer, container);
        return true;
    }

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        container = this.GetAll(Predicate).FirstOrDefault();
        return container is not null;

        bool Predicate(IStorageContainer container) =>
            container switch
            {
                ChestContainer chestContainer => chestContainer.Chest == item,
                ObjectContainer objectContainer => objectContainer.Object.heldObject.Value == item,
                _ => false,
            };
    }

    private IEnumerable<IStorageContainer> GetAllFromPlayers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var container in this.GetAllFromPlayer(farmer, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromLocations(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        var foundLocations = new HashSet<GameLocation>();
        var locationQueue = new Queue<GameLocation>();

        foreach (var location in Game1.locations)
        {
            locationQueue.Enqueue(location);
        }

        while (locationQueue.TryDequeue(out var location))
        {
            if (!foundLocations.Add(location))
            {
                continue;
            }

            foreach (var container in this.GetAllFromLocation(location, predicate))
            {
                if (!foundContainers.Add(container))
                {
                    continue;
                }

                containerQueue.Enqueue(container);
                yield return container;
            }

            foreach (var building in location.buildings)
            {
                if (building.GetIndoorsType() == IndoorsType.Instanced)
                {
                    locationQueue.Enqueue(building.GetIndoors());
                }
            }
        }
    }

    private IEnumerable<IStorageContainer> GetAllFromContainers(
        ISet<IStorageContainer> foundContainers,
        Queue<IStorageContainer> containerQueue,
        Func<IStorageContainer, bool>? predicate = default)
    {
        while (containerQueue.TryDequeue(out var container))
        {
            foreach (var childContainer in this.GetAllFromContainer(container, predicate))
            {
                if (!foundContainers.Add(childContainer))
                {
                    continue;
                }

                containerQueue.Enqueue(childContainer);
                yield return childContainer;
            }
        }
    }

    private bool TryGetAny(Item item, [NotNullWhen(true)] out IStorageContainer? container)
    {
        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        var chest = item as Chest ?? (item as SObject)?.heldObject.Value as Chest;
        if (chest is null && !this.proxyChestFactory.TryGetProxy(item, out chest))
        {
            container = null;
            return false;
        }

        if (!this.storageOptions.TryGetValue(item.QualifiedItemId, out var storageOption))
        {
            var data =
                ItemRegistry.GetData(item.QualifiedItemId)?.RawData as BigCraftableData ?? new BigCraftableData();

            storageOption = new BigCraftableStorageOptions(this.modConfig.DefaultOptions, data);
            this.storageOptions.Add(item.QualifiedItemId, storageOption);
        }

        container = item switch
        {
            Chest => new ChestContainer(storageOption, chest),
            SObject obj => new ObjectContainer(storageOption, obj, chest),
            _ => new ChestContainer(storageOption, chest),
        };

        this.cachedContainers.AddOrUpdate(item, container);
        this.cachedContainers.AddOrUpdate(chest, container);
        return true;
    }
}