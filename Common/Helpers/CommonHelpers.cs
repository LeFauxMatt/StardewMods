namespace StardewMods.Common.Helpers;

using Microsoft.Xna.Framework;

/// <summary>Commonly used helpers for utility methods.</summary>
internal static class CommonHelpers
{
    /// <summary>Gets the map tile the cursor is over.</summary>
    /// <param name="radius">The tile distance from the player.</param>
    /// <param name="fallback">Fallback to grab tile if cursor tile is out of range.</param>
    /// <returns>Returns the tile position.</returns>
    public static Vector2 GetCursorTile(int radius = 0, bool fallback = true)
    {
        if (radius == 0)
        {
            return Game1.lastCursorTile;
        }

        var pos = Game1.GetPlacementGrabTile();
        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;

        if (fallback && !Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, radius, Game1.player))
        {
            pos = Game1.player.GetGrabTile();
        }

        return pos;
    }
}