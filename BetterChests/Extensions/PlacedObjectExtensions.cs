namespace StardewMods.BetterChests.Extensions;

using Common.Records;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Extension methods for the PlacedObject record.
/// </summary>
internal static class PlacedObjectExtensions
{
    /// <summary>
    ///     Gets the Chest associated with the PlacedChest.
    /// </summary>
    /// <param name="placedObject">The PlacedObject to get the Chest for.</param>
    /// <param name="chest">A Chest if it is accessible to the player.</param>
    /// <returns>Returns true if the Placed Object is a Chest that is accessible to the player.</returns>
    public static bool ToChest(this PlacedObject placedObject, out Chest chest)
    {
        var (location, position) = placedObject;
        chest = location switch
        {
            FarmHouse farmHouse when farmHouse.fridgePosition.ToVector2().Equals(position) => farmHouse.fridge.Value,
            not null when placedObject.Object is Chest placedChest => placedChest,
            not null when placedObject.Object is { heldObject.Value: Chest heldChest } => heldChest,
            _ => null,
        };
        return chest is not null;
    }
}