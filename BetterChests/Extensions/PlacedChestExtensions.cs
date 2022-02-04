namespace StardewMods.BetterChests.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Records;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Objects;

/// <summary>
///     Extension methods for the PlacedChest record.
/// </summary>
internal static class PlacedChestExtensions
{
    /// <summary>
    ///     Gets or sets a function that returns accessible game locations for the current player.
    /// </summary>
    public static Func<IEnumerable<GameLocation>> GetAccessibleLocations { get; set; }

    /// <summary>
    ///     Gets the Chest associated with the PlacedChest.
    /// </summary>
    /// <param name="placedChest">The placed Chest to get the object for.</param>
    /// <param name="chest">A Chest if it is accessible to the player.</param>
    /// <returns>Returns true if the Placed Object is a Chest that is accessible to the player.</returns>
    public static bool ToChest(this PlacedChest placedChest, out Chest chest)
    {
        if (!placedChest.ToPlacedObject(out var placedObject))
        {
            chest = null;
            return false;
        }

        return placedObject.ToChest(out chest);
    }

    /// <summary>
    ///     Gets an object placed in a location at a position.
    /// </summary>
    /// <param name="placedChest">The placed Chest to get the object for.</param>
    /// <param name="placedObject">The PlacedObject representation of the placed chest.</param>
    /// <returns>Returns true if the PlacedObject could be found.</returns>
    public static bool ToPlacedObject(this PlacedChest placedChest, out PlacedObject placedObject)
    {
        placedObject = new(PlacedChestExtensions.GetAccessibleLocations().FirstOrDefault(location => location.NameOrUniqueName == placedChest.LocationName), new(placedChest.X, placedChest.Y));
        return placedObject.Location is not null;
    }
}