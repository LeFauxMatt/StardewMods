namespace StardewMods.FuryCore.Interfaces.GameObjects;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;

/// <summary>
///     Provides enumerable access to all game objects.
/// </summary>
public interface IGameObjects
{
    /// <summary>
    ///     Returns context objects for items in the player's inventory.
    /// </summary>
    /// <param name="player">The player to get context items from.</param>
    /// <returns>Context objects belonging to the player.</returns>
    public delegate IEnumerable<(int Index, object Context)> GetInventoryItems(Farmer player);

    /// <summary>
    ///     Returns context objects for coordinates at a game location.
    /// </summary>
    /// <param name="location">The location to get context objects from.</param>
    /// <returns>Context objects and their coordinates.</returns>
    public delegate IEnumerable<(Vector2 Position, object Context)> GetLocationObjects(GameLocation location);

    /// <summary>
    ///     Gets all items in the player's inventory.
    /// </summary>
    public IEnumerable<KeyValuePair<InventoryItem, IGameObject>> InventoryItems { get; }

    /// <summary>
    ///     Gets objects and buildings in all locations.
    /// </summary>
    public IEnumerable<KeyValuePair<LocationObject, IGameObject>> LocationObjects { get; }

    /// <summary>
    ///     Adds a custom getter for inventory items.
    /// </summary>
    /// <param name="getInventoryItems">The inventory items getter to add.</param>
    public void AddInventoryItemsGetter(GetInventoryItems getInventoryItems);

    /// <summary>
    ///     Adds a custom getter for location objects.
    /// </summary>
    /// <param name="getLocationObjects">The location objects getter to add.</param>
    public void AddLocationObjectsGetter(GetLocationObjects getLocationObjects);

    /// <summary>
    ///     Attempts to get a GameObject based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="gameObject">The GameObject for the context object.</param>
    /// <returns>True if a GameObject could be found.</returns>
    public bool TryGetGameObject(object context, out IGameObject gameObject);
}