namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewMods.Common.Interfaces;
using StardewValley.Buildings;
using StardewValley.GameData.BigCraftables;
using StardewValley.Objects;

/// <summary>Provides access to all known storages for other services.</summary>
internal sealed class ContainerFactory : BaseService
{
    private readonly ConditionalWeakTable<object, IContainer> cachedContainers = new();
    private readonly ModConfig config;
    private readonly ItemMatcherFactory itemMatchers;
    private readonly Dictionary<string, IStorage> storageTypes = new();
    private readonly VirtualizedChestFactory vChests;

    /// <summary>Initializes a new instance of the <see cref="ContainerFactory" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="itemMatchers">Dependency used for getting an ItemMatcher.</param>
    /// <param name="vChests">Dependency used for creating virtualized chests.</param>
    public ContainerFactory(ILogging logging, ModConfig config, ItemMatcherFactory itemMatchers, VirtualizedChestFactory vChests)
        : base(logging)
    {
        this.config = config;
        this.itemMatchers = itemMatchers;
        this.vChests = vChests;
    }

    /// <summary>
    /// Retrieves all container items that satisfy the specified predicate, if provided. If no predicate is provided,
    /// returns all container items.
    /// </summary>
    /// <param name="predicate">Optional. A function that defines the conditions of the container items to search for.</param>
    /// <returns>An enumerable collection of IContainer items that satisfy the predicate, if provided.</returns>
    public IEnumerable<IContainer> GetAll(Func<IContainer, bool>? predicate = default)
    {
        var foundContainers = new HashSet<IContainer>();
        var containerQueue = new Queue<IContainer>();

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
    public IEnumerable<IContainer> GetAllFromContainer(IContainer parentContainer, Func<IContainer, bool>? predicate = default)
    {
        foreach (var item in parentContainer.Items)
        {
            if (!this.TryGetOne(item, out var childContainer))
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
    /// <param name="location">The game location where the container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IContainer> GetAllFromLocation(GameLocation location, Func<IContainer, bool>? predicate = default)
    {
        // Search for containers from placed objects
        foreach (var obj in location.Objects.Values)
        {
            if (!this.TryGetOne(obj, out var container))
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

            if (!this.storageTypes.TryGetValue($"(B){building.buildingType.Value}", out var storageType))
            {
                storageType = new BuildingStorage(this.config.DefaultOptions, building.GetData());
                this.storageTypes.Add($"(B){building.buildingType.Value}", storageType);
            }

            foreach (var chest in building.buildingChests)
            {
                if (!this.cachedContainers.TryGetValue(chest, out var container))
                {
                    container = new BuildingContainer(this.itemMatchers.GetDefault(), storageType, building, chest);

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
            if (!this.storageTypes.TryGetValue($"(L){location.Name}", out var storageType))
            {
                storageType = new LocationStorage(this.config.DefaultOptions, location.GetData());
                this.storageTypes.Add($"(L){location.Name}", storageType);
            }

            if (!this.cachedContainers.TryGetValue(fridge, out var container))
            {
                container = new FridgeContainer(this.itemMatchers.GetDefault(), storageType, location, fridge);

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
    public IEnumerable<IContainer> GetAllFromPlayer(Farmer farmer, Func<IContainer, bool>? predicate = default)
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
            if (!this.TryGetOne(item, out var childContainer))
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

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Item item, [NotNullWhen(true)] out IContainer? container)
    {
        if (this.cachedContainers.TryGetValue(item, out container))
        {
            return true;
        }

        var chest = item as Chest;
        chest ??= (item as SObject)?.heldObject.Value as Chest;
        chest ??= VirtualizedChest.TryGetId(item, out var id) && this.vChests.TryGetOne(id, out var vChest) ? vChest.Chest : null;

        if (chest is null)
        {
            container = null;
            return false;
        }

        if (!this.storageTypes.TryGetValue(item.QualifiedItemId, out var storageType))
        {
            var data = ItemRegistry.GetData(item.QualifiedItemId).RawData as BigCraftableData ?? new BigCraftableData();
            storageType = new BigCraftableStorage(this.config.DefaultOptions, data);
            this.storageTypes.Add(item.QualifiedItemId, storageType);
        }

        var itemMatcher = this.itemMatchers.GetDefault();
        container = item switch
        {
            Chest => new ChestContainer(itemMatcher, storageType, chest), SObject obj => new ObjectContainer(itemMatcher, storageType, obj, chest), _ => new ChestContainer(itemMatcher, storageType, chest),
        };

        this.cachedContainers.AddOrUpdate(item, container);
        return true;
    }

    /// <summary>Tries to retrieve a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns><c>true</c> if a container is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(Farmer farmer, [NotNullWhen(true)] out IContainer? container)
    {
        if (this.cachedContainers.TryGetValue(farmer, out container))
        {
            return true;
        }

        container = new FarmerContainer(this.itemMatchers.GetDefault(), this.config.DefaultOptions, farmer);
        this.cachedContainers.AddOrUpdate(farmer, container);
        return true;
    }

    private IEnumerable<IContainer> GetAllFromPlayers(ISet<IContainer> foundContainers, Queue<IContainer> containerQueue, Func<IContainer, bool>? predicate = default)
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

    private IEnumerable<IContainer> GetAllFromLocations(ISet<IContainer> foundContainers, Queue<IContainer> containerQueue, Func<IContainer, bool>? predicate = default)
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

    private IEnumerable<IContainer> GetAllFromContainers(ISet<IContainer> foundContainers, Queue<IContainer> containerQueue, Func<IContainer, bool>? predicate = default)
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
}
