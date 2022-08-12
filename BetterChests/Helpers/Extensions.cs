namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;

/// <summary>
///     Extension methods
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///     Tests whether the player is within range of the location.
    /// </summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns>Returns true if the location is within range.</returns>
    public static bool WithinRangeOfPlayer(this FeatureOptionRange range, int distance, object parent, Vector2 position)
    {
        return range switch
        {
            FeatureOptionRange.World => true,
            FeatureOptionRange.Inventory when parent is Farmer farmer && farmer.Equals(Game1.player) => true,
            FeatureOptionRange.Default or FeatureOptionRange.Disabled or FeatureOptionRange.Inventory => false,
            FeatureOptionRange.Location when parent is GameLocation location && !location.Equals(Game1.currentLocation)
                => false,
            FeatureOptionRange.Location when distance == -1 => true,
            FeatureOptionRange.Location when Math.Abs(position.X - Game1.player.getTileX())
                                           + Math.Abs(position.Y - Game1.player.getTileY())
                                          <= distance => true,
            _ => false,
        };
    }

    /// <summary>
    ///     Returns storage with parent type updated.
    /// </summary>
    /// <param name="storage">The storage object.</param>
    /// <param name="storageTypes">The storage types to populate.</param>
    /// <returns>The storage object with type updated.</returns>
    public static IStorageObject WithType(
        this IStorageObject storage,
        Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        if (storage is not IStorageNode storageNode)
        {
            return storage;
        }

        foreach (var (predicate, type) in storageTypes)
        {
            if (!predicate(storage.Context))
            {
                continue;
            }

            storageNode.Parent = type;
            break;
        }

        return storage;
    }

    /// <summary>
    ///     Returns storages with parent types updated.
    /// </summary>
    /// <param name="storages">List of storages to return from.</param>
    /// <param name="storageTypes">The storage types to populate.</param>
    /// <returns>A list of storages with types updated.</returns>
    public static IEnumerable<IStorageObject> WithTypes(
        this IEnumerable<IStorageObject> storages,
        Dictionary<Func<object, bool>, IStorageData> storageTypes)
    {
        return storages.Select(storage => storage.WithType(storageTypes));
    }
}