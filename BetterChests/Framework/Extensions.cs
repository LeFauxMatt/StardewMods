namespace StardewMods.BetterChests.Framework;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Extension methods for Better Chests.</summary>
internal static class Extensions
{
    /// <summary>Add a chest capacity option at the current position in the form.</summary>
    /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="data">The storage data.</param>
    public static void AddChestCapacityOption(
        this GenericModConfigMenuIntegration gmcm,
        IManifest manifest,
        IStorageData data) =>
        gmcm.Api!.AddNumberOption(
            manifest,
            data.GetChestCapacity,
            data.SetChestCapacity,
            I18n.Config_ResizeChestCapacity_Name,
            I18n.Config_ResizeChestCapacity_Tooltip,
            0,
            8,
            1,
            Formatting.ChestCapacity);

    /// <summary>Add a distance option at the current position in the form.</summary>
    /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="data">The storage data.</param>
    /// <param name="featureName">The feature which the distance is associated with.</param>
    /// <param name="name">The label text to show in the form.</param>
    /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    public static void AddDistanceOption(
        this GenericModConfigMenuIntegration gmcm,
        IManifest manifest,
        IStorageData data,
        string featureName,
        Func<string> name,
        Func<string> tooltip) =>
        gmcm.Api!.AddNumberOption(
            manifest,
            () => data.GetDistance(featureName),
            value => data.SetDistance(featureName, value),
            name,
            tooltip,
            (int)FeatureOptionRange.Default,
            (int)FeatureOptionRange.World,
            1,
            Formatting.Distance);

    /// <summary>Add a feature option at the current position in the form.</summary>
    /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="getValue">Get the current value from the mod config.</param>
    /// <param name="setValue">Set a new value in the mod config.</param>
    /// <param name="name">The label text to show in the form.</param>
    /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    public static void AddFeatureOption(
        this GenericModConfigMenuIntegration gmcm,
        IManifest manifest,
        Func<FeatureOption> getValue,
        Action<FeatureOption> setValue,
        Func<string> name,
        Func<string> tooltip) =>
        gmcm.Api!.AddTextOption(
            manifest,
            () => getValue().ToStringFast(),
            value => setValue(FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default),
            name,
            tooltip,
            FeatureOptionExtensions.GetNames(),
            Formatting.Option);

    /// <summary>Add a feature option range at the current position in the form.</summary>
    /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="getValue">Get the current value from the mod config.</param>
    /// <param name="setValue">Set a new value in the mod config.</param>
    /// <param name="name">The label text to show in the form.</param>
    /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    public static void AddFeatureOptionRange(
        this GenericModConfigMenuIntegration gmcm,
        IManifest manifest,
        Func<FeatureOptionRange> getValue,
        Action<FeatureOptionRange> setValue,
        Func<string> name,
        Func<string> tooltip) =>
        gmcm.Api!.AddTextOption(
            manifest,
            () => getValue().ToStringFast(),
            value => setValue(
                FeatureOptionRangeExtensions.TryParse(value, out var range) ? range : FeatureOptionRange.Default),
            name,
            tooltip,
            FeatureOptionRangeExtensions.GetNames(),
            Formatting.Range);

    /// <summary>Gets context tags from an <see cref="Item" /> with extended tag set.</summary>
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

    /// <summary>Gets storage distance from a player in tiles.</summary>
    /// <param name="storage">The storage to get the distance for.</param>
    /// <param name="player">The player to get the distance from.</param>
    /// <returns>Returns the distance in tiles.</returns>
    public static int GetDistanceToPlayer(this StorageNode storage, Farmer player)
    {
        if (storage is not
            {
                Data: Storage storageObject,
            })
        {
            return 0;
        }

        return (int)(Math.Abs(storageObject.Position.X - player.Tile.X)
            + Math.Abs(storageObject.Position.Y - player.Tile.Y));
    }

    /// <summary>Organizes items in a storage.</summary>
    /// <param name="storage">The storage to organize.</param>
    /// <param name="descending">Sort in descending order.</param>
    public static void OrganizeItems(this StorageNode storage, bool descending = false)
    {
        if (storage is not
            {
                Data: Storage storageObject,
            })
        {
            return;
        }

        if (storage.OrganizeChestGroupBy == GroupBy.Default && storage.OrganizeChestSortBy == SortBy.Default)
        {
            ItemGrabMenu.organizeItemsInList(storageObject.Inventory);
            return;
        }

        var items = storageObject.Inventory.ToArray();
        Array.Sort(
            items,
            (i1, i2) =>
            {
                if (ReferenceEquals(i2, null))
                {
                    return -1;
                }

                if (ReferenceEquals(i1, null))
                {
                    return 1;
                }

                if (ReferenceEquals(i1, i2))
                {
                    return 0;
                }

                var g1 = storage.OrganizeChestGroupBy switch
                {
                    GroupBy.Category => i1.GetContextTagsExt()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase))
                        ?? string.Empty,
                    GroupBy.Color => i1.GetContextTagsExt()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase))
                        ?? string.Empty,
                    GroupBy.Name => i1.DisplayName,
                    _ => string.Empty,
                };

                var g2 = storage.OrganizeChestGroupBy switch
                {
                    GroupBy.Category => i2.GetContextTagsExt()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase))
                        ?? string.Empty,
                    GroupBy.Color => i2.GetContextTagsExt()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase))
                        ?? string.Empty,
                    GroupBy.Name => i2.DisplayName,
                    _ => string.Empty,
                };

                if (!g1.Equals(g2, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Compare(g1, g2, StringComparison.OrdinalIgnoreCase);
                }

                var o1 = storage.OrganizeChestSortBy switch
                {
                    SortBy.Quality when i1 is SObject obj => obj.Quality,
                    SortBy.Quantity => i1.Stack,
                    SortBy.Type => i1.Category,
                    _ => 0,
                };

                var o2 = storage.OrganizeChestSortBy switch
                {
                    SortBy.Quality when i2 is SObject obj => obj.Quality,
                    SortBy.Quantity => i2.Stack,
                    SortBy.Type => i2.Category,
                    _ => 0,
                };

                return o1.CompareTo(o2);
            });

        if (descending)
        {
            Array.Reverse(items);
        }

        storageObject.Inventory.Clear();
        foreach (var item in items)
        {
            storageObject.Inventory.Add(item);
        }
    }

    /// <summary>Removes an item from a storage.</summary>
    /// <param name="storage">The storage to remove an item from.</param>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns true if the item could be removed.</returns>
    public static bool RemoveItem(this StorageNode storage, Item item) =>
        storage is
        {
            Data: Storage storageObject,
        }
        && storageObject.Inventory.Remove(item);

    /// <summary>Stashes an item into storage based on categorization and stack settings.</summary>
    /// <param name="storage">The storage to stash an item into.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="item">The item to stash.</param>
    /// <param name="existingStacks">Whether to stash into stackable items or based on categorization.</param>
    /// <returns>Returns the <see cref="Item" /> if not all could be stashed or null if successful.</returns>
    public static Item? StashItem(this StorageNode storage, IMonitor monitor, Item item, bool existingStacks = false)
    {
        // Disallow stashing of any Chest.
        if (storage is not
            {
                Data: Storage storageObject,
            }
            || item is Chest
                or SObject
                {
                    heldObject.Value: Chest,
                })
        {
            return item;
        }

        var stack = item.Stack;
        var tmp = (existingStacks && storageObject.Inventory.Any(otherItem => otherItem?.canStackWith(item) == true))
            || (storage.FilterItemsList.Any()
                && !storage.FilterItemsList.All(filter => filter.StartsWith("!", StringComparison.OrdinalIgnoreCase))
                && storage.FilterMatches(item))
                ? storageObject.AddItem(item)
                : item;

        if (tmp is null || stack != item.Stack)
        {
            monitor.Log(
                $"StashItem: {{ Item: {item.Name}, Quantity: {Math.Max(1, stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, To: {storage}");
        }

        return tmp;
    }

    /// <summary>Tests whether the player is within range of the location.</summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns>Returns true if the location is within range.</returns>
    public static bool WithinRangeOfPlayer(
        this FeatureOptionRange range,
        int distance,
        object parent,
        Vector2 position) =>
        range switch
        {
            FeatureOptionRange.World => true,
            FeatureOptionRange.Inventory when parent is Farmer farmer && farmer.Equals(Game1.player) => true,
            FeatureOptionRange.Default or FeatureOptionRange.Disabled or FeatureOptionRange.Inventory => false,
            FeatureOptionRange.Location when parent is GameLocation location && !location.Equals(Game1.currentLocation)
                => false,
            FeatureOptionRange.Location when distance == -1 => true,
            FeatureOptionRange.Location when Math.Abs(position.X - Game1.player.Tile.X)
                + Math.Abs(position.Y - Game1.player.Tile.Y)
                <= distance => true,
            _ => false,
        };

    /// <summary>Checks if the <see cref="Item" /> can be donated to a <see cref="CommunityCenter" /> bundle.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item can be donated.</returns>
    private static bool CanDonateToBundle(this Item item) =>
        item is SObject obj
        && (Game1.locations.OfType<CommunityCenter>().FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
            ?? false);

    /// <summary>Checks if the <see cref="Item" /> can be donated to the <see cref="LibraryMuseum" />.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item can be donated.</returns>
    private static bool CanDonateToMuseum(this Item item) =>
        Game1.locations.OfType<LibraryMuseum>().FirstOrDefault()?.isItemSuitableForDonation(item) ?? false;

    private static int GetChestCapacity(this IStorageData data) =>
        data.ResizeChestCapacity switch
        {
            _ when data.ResizeChest is FeatureOption.Default => (int)FeatureOption.Default,
            _ when data.ResizeChest is FeatureOption.Disabled => (int)FeatureOption.Disabled,
            -1 => 8,
            _ => ((int)FeatureOption.Enabled + (data.ResizeChestCapacity / 12)) - 1,
        };

    private static int GetDistance(this IStorageData data, string featureName)
    {
        var feature = featureName switch
        {
            nameof(CraftFromChest) => data.CraftFromChest,
            nameof(StashToChest) => data.StashToChest,
            _ => throw new ArgumentException($"Invalid feature {featureName}"),
        };

        var distance = featureName switch
        {
            nameof(CraftFromChest) => data.CraftFromChestDistance,
            nameof(StashToChest) => data.StashToChestDistance,
            _ => 0,
        };

        return distance switch
        {
            _ when feature is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
            _ when feature is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
            _ when feature is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
            _ when feature is FeatureOptionRange.World => (int)FeatureOptionRange.World,
            >= 2 when feature is FeatureOptionRange.Location => ((int)FeatureOptionRange.Location
                    + (int)Math.Ceiling(Math.Log2(distance)))
                - 1,
            _ when feature is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
            _ => (int)FeatureOptionRange.Default,
        };
    }

    /// <summary>Checks if the <see cref="Item" /> is an artifact.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is an artifact.</returns>
    private static bool IsArtifact(this Item item) =>
        item is SObject
        {
            Type: "Arch",
        };

    /// <summary>Checks if the <see cref="Item" /> is <see cref="Furniture" />.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if the item is furniture.</returns>
    private static bool IsFurniture(this Item item) => item is Furniture;

    private static void SetChestCapacity(this IStorageData data, int value)
    {
        data.ResizeChestCapacity = value switch
        {
            (int)FeatureOption.Default => 0,
            (int)FeatureOption.Disabled => 0,
            8 => -1,
            >= (int)FeatureOption.Enabled => 12 * ((1 + value) - (int)FeatureOption.Enabled),
            _ => 0,
        };

        data.ResizeChest = value switch
        {
            (int)FeatureOption.Default => FeatureOption.Default,
            (int)FeatureOption.Disabled => FeatureOption.Disabled,
            _ => FeatureOption.Enabled,
        };
    }

    private static void SetDistance(this IStorageData data, string featureName, int value)
    {
        var distance = value switch
        {
            (int)FeatureOptionRange.Default => 0,
            (int)FeatureOptionRange.Disabled => 0,
            (int)FeatureOptionRange.Inventory => 0,
            (int)FeatureOptionRange.World - 1 => -1,
            (int)FeatureOptionRange.World => 0,
            >= (int)FeatureOptionRange.Location => (int)Math.Pow(2, (1 + value) - (int)FeatureOptionRange.Location),
            _ => 0,
        };

        var range = value switch
        {
            (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
            (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
            (int)FeatureOptionRange.World => FeatureOptionRange.World,
            (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
            _ => FeatureOptionRange.Location,
        };

        switch (featureName)
        {
            case nameof(CraftFromChest):
                data.CraftFromChestDistance = distance;
                data.CraftFromChest = range;
                return;

            case nameof(StashToChest):
                data.StashToChestDistance = distance;
                data.StashToChest = range;
                return;

            default:
                throw new ArgumentException($"Invalid feature {featureName}");
        }
    }
}
