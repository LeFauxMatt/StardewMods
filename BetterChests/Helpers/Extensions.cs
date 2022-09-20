namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Extension methods for Better Chests.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///     Gets context tags from an <see cref="Item" /> with extended tag set.
    /// </summary>
    /// <param name="item">The item to get context tags from.</param>
    /// <returns>Returns the context tags.</returns>
    public static IEnumerable<string> GetContextTagsExt(this Item item)
    {
        var tags = item.GetContextTags();

        if (item.CanDonateToBundle())
        {
            tags.Add("donate_bundle");
        }

        if (item.CanDonateToMuseum())
        {
            tags.Add("donate_museum");
        }

        if (item.IsArtifact())
        {
            tags.Add("category_artifact");
        }

        if (item.IsFurniture())
        {
            tags.Add("category_furniture");
        }

        return tags;
    }

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
    ///     Checks if the <see cref="Item" /> can be donated to a <see cref="CommunityCenter" /> bundle.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item can be donated.</returns>
    private static bool CanDonateToBundle(this Item item)
    {
        return item is SObject obj
            && (Game1.locations.OfType<CommunityCenter>().FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
             ?? false);
    }

    /// <summary>
    ///     Checks if the <see cref="Item" /> can be donated to the <see cref="LibraryMuseum" />.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item can be donated.</returns>
    private static bool CanDonateToMuseum(this Item item)
    {
        return Game1.locations.OfType<LibraryMuseum>().FirstOrDefault()?.isItemSuitableForDonation(item) ?? false;
    }

    /// <summary>
    ///     Checks if the <see cref="Item" /> is an artifact.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is an artifact.</returns>
    private static bool IsArtifact(this Item item)
    {
        return item is SObject { Type: "Arch" };
    }

    /// <summary>
    ///     Checks if the <see cref="Item" /> is <see cref="Furniture" />.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is furniture.</returns>
    private static bool IsFurniture(this Item item)
    {
        return item is Furniture;
    }
}