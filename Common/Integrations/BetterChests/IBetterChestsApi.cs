namespace StardewMods.Common.Integrations.BetterChests;

using System;
using System.Collections.Generic;

/// <summary>
///     API for Better Chests.
/// </summary>
public interface IBetterChestsApi
{
    /// <summary>
    ///     Gets storages from all locations and farmer inventory in the game.
    /// </summary>
    public IEnumerable<IStorageObject> AllStorages { get; }

    /// <summary>
    ///     Gets the types of storages in the game.
    /// </summary>
    public Dictionary<string, IStorageData> StorageTypes { get; }

    /// <summary>
    ///     Adds all applicable config options to an existing GMCM for this storage data.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="storage">The storage to configure for.</param>
    public void AddConfigOptions(IManifest manifest, IStorageData storage);

    /// <summary>
    ///     Gets all storages being held by a player.
    /// </summary>
    /// <param name="farmer">The farmer to get storages from.</param>
    /// <returns>Returns the storages.</returns>
    public IEnumerable<IStorageObject> GetStorages(Farmer farmer);

    /// <summary>
    ///     Gets all storages placed in a location.
    /// </summary>
    /// <param name="location">The location to get storages from.</param>
    /// <returns>Returns the storages.</returns>
    public IEnumerable<IStorageObject> GetStorages(GameLocation location);

    /// <summary>
    ///     Registers a chest type based on any object containing the mod data key-value pair.
    /// </summary>
    /// <param name="predicate">A function which returns true for valid storages.</param>
    /// <param name="storage">The storage data.</param>
    public void RegisterChest(Func<object, bool> predicate, IStorageData storage);

    /// <summary>
    ///     Opens the crafting menu.
    /// </summary>
    /// <param name="storages">The storages to craft from.</param>
    public void ShowCraftingPage(IEnumerable<IStorageObject> storages);

    /// <summary>
    ///     Attempts to retrieve a storage based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found for the context object.</returns>
    public bool TryGetStorage(object context, [NotNullWhen(true)] out IStorageObject? storage);
}