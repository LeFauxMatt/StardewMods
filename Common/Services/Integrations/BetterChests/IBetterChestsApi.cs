namespace StardewMods.GarbageDay.Services.Integrations.BetterChests;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>Mod API for Better Chests.</summary>
public interface IBetterChestsApi
{
    /// <summary>Represents an event that is raised before an item is transferred.</summary>
    public event EventHandler<EventArgs> ItemTransferred;

    /// <summary>
    /// Retrieves all container items that satisfy the specified predicate, if provided. If no predicate is provided,
    /// returns all container items.
    /// </summary>
    /// <param name="predicate">Optional. A function that defines the conditions of the container items to search for.</param>
    /// <returns>An enumerable collection of IContainer items that satisfy the predicate, if provided.</returns>
    public IEnumerable<IStorageContainer> GetAllContainers(Func<IStorageContainer, bool>? predicate = default);

    /// <summary>Retrieves all containers from the specified game location that match the optional predicate.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAllContainersFromLocation(
        GameLocation location,
        Func<IStorageContainer, bool>? predicate = default);

    /// <summary>Retrieves all container items from the specified player matching the optional predicate.</summary>
    /// <param name="farmer">The player whose container items will be retrieved.</param>
    /// <param name="predicate">The predicate to filter the containers.</param>
    /// <returns>An enumerable collection of containers that match the predicate.</returns>
    public IEnumerable<IStorageContainer> GetAllContainersFromPlayer(
        Farmer farmer,
        Func<IStorageContainer, bool>? predicate = default);

    /// <summary>Transfers items from one container to another.</summary>
    /// <param name="containerFrom">The container to transfer items from.</param>
    /// <param name="containerTo">The container to transfer items to.</param>
    /// <returns>Returns the items that were transferred.</returns>
    public IEnumerable<(string ItemId, int Amount, bool Prevented)> Transfer(
        IStorageContainer containerFrom,
        IStorageContainer containerTo);

    /// <summary>Tries to retrieve a container from the specified game location at the specified position.</summary>
    /// <param name="location">The game location where the container will be retrieved.</param>
    /// <param name="pos">The position of the game location where the container will be retrieved.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetContainerFromLocation(
        GameLocation location,
        Vector2 pos,
        [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to retrieve a container from the active menu.</summary>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetContainerFromMenu([NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to get a container from the specified farmer.</summary>
    /// <param name="farmer">The player whose container will be retrieved.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <param name="index">The index of the player's inventory. Defaults to the active item.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetContainerFromPlayer(
        Farmer farmer,
        [NotNullWhen(true)] out IStorageContainer? container,
        int index = -1);

    /// <summary>Tries to retrieve a container from the specified farmer.</summary>
    /// <param name="farmer">The farmer to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetContainerFromBackpack(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container);

    /// <summary>Tries to get a container from the specified object.</summary>
    /// <param name="item">The item to get a container from.</param>
    /// <param name="container">When this method returns, contains the container if found; otherwise, null.</param>
    /// <returns>true if a container is found; otherwise, false.</returns>
    public bool TryGetContainerFromItem(Item item, [NotNullWhen(true)] out IStorageContainer? container);
}