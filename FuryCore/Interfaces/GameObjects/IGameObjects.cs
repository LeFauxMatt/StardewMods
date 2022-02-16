namespace StardewMods.FuryCore.Interfaces.GameObjects;

using System;
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
    public void AddInventoryItemsGetter(Func<Farmer, IEnumerable<(int Index, object Context)>> getInventoryItems);

    /// <summary>
    ///     Adds a custom getter for location objects.
    /// </summary>
    /// <param name="getLocationObjects">The location objects getter to add.</param>
    public void AddLocationObjectsGetter(Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>> getLocationObjects);

    /// <summary>
    ///     Attempts to get a GameObject based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="gameObject">The GameObject for the context object.</param>
    /// <returns>True if a GameObject could be found.</returns>
    public bool TryGetGameObject(object context, out IGameObject gameObject);
}