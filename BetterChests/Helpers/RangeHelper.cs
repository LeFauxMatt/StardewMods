namespace StardewMods.BetterChests.Helpers;

using System;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;

/// <summary>
///     Encompasses logic to determine if objects are within ranges defined by <see cref="FeatureOptionRange" />.
/// </summary>
internal static class RangeHelper
{
    /// <summary>
    ///     Tests whether the player is within range of the location.
    /// </summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns>Returns true if the location is within range.</returns>
    public static bool IsWithinRangeOfPlayer(FeatureOptionRange range, int distance, object parent, Vector2 position)
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
}