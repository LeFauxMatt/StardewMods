namespace StardewMods.Common.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

/// <summary>
///     Commonly used helpers for utility methods.
/// </summary>
internal static class CommonHelpers
{
    /// <summary>
    ///     Gets all accessible game locations and sub-locations
    /// </summary>
    public static IEnumerable<GameLocation> AllLocations
    {
        get
        {
            IEnumerable<GameLocation> IterateLocations(
                IEnumerable<GameLocation>? locations = null,
                HashSet<GameLocation>? excluded = null)
            {
                locations ??= Context.IsMainPlayer ? Game1.locations : CommonHelpers.Multiplayer!.GetActiveLocations();
                excluded ??= new();

                foreach (var location in locations)
                {
                    if (excluded.Contains(location))
                    {
                        continue;
                    }

                    excluded.Add(location);
                    yield return location;

                    if (location is not BuildableGameLocation buildableGameLocation)
                    {
                        continue;
                    }

                    var indoors = buildableGameLocation.buildings.Select(building => building.indoors.Value)
                        .Where(indoors => indoors is not null);
                    foreach (var indoor in IterateLocations(indoors, excluded))
                    {
                        yield return indoor;
                    }
                }
            }

            foreach (var location in IterateLocations())
            {
                yield return location;
            }
        }
    }

    /// <inheritdoc cref="IMultiplayerHelper" />
    public static IMultiplayerHelper? Multiplayer { get; set; }

    /// <summary>
    ///     Gets or initializes ModConfig.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <typeparam name="T">The ModConfig type.</typeparam>
    /// <returns>Returns an existing or new instance of ModConfig.</returns>
    public static T GetConfig<T>(IModHelper helper)
        where T : class, new()
    {
        T? config = default;
        try
        {
            config = helper.ReadConfig<T>();
        }
        catch (Exception)
        {
            Log.Warn($"Error loading config: {typeof(T).Name}");
        }

        config ??= new();
        Log.Trace(config.ToString()!);
        return config;
    }

    /// <summary>
    ///     Gets the map tile the cursor is over.
    /// </summary>
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