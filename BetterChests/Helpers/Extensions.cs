namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///     Checks if the <see cref="Item" /> can be donated to a <see cref="CommunityCenter" /> bundle.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item can be donated.</returns>
    public static bool CanDonateToBundle(this Item item)
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
    public static bool CanDonateToMuseum(this Item item)
    {
        return Game1.locations.OfType<LibraryMuseum>().FirstOrDefault()?.isItemSuitableForDonation(item) ?? false;
    }

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
    ///     Checks if the <see cref="Item" /> is an artifact.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is an artifact.</returns>
    public static bool IsArtifact(this Item item)
    {
        return item is SObject { Type: "Arch" };
    }

    /// <summary>
    ///     Checks if the <see cref="Item" /> is <see cref="Furniture" />.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is furniture.</returns>
    public static bool IsFurniture(this Item item)
    {
        return item is Furniture;
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