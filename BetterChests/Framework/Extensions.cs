namespace StardewMods.BetterChests.Framework;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <summary>Extension methods for Better Chests.</summary>
internal static class Extensions
{
    /// <summary>Tests whether the player is within range of the location.</summary>
    /// <param name="range">The range.</param>
    /// <param name="distance">The distance in tiles to the player.</param>
    /// <param name="parent">The context where the source object is contained.</param>
    /// <param name="position">The coordinates.</param>
    /// <returns>Returns true if the location is within range.</returns>
    public static bool WithinRange(this RangeOption range, int distance, object parent, Vector2 position) =>
        range switch
        {
            RangeOption.World => true,
            RangeOption.Inventory when parent is Farmer farmer && farmer.Equals(Game1.player) => true,
            RangeOption.Default or RangeOption.Disabled or RangeOption.Inventory => false,
            RangeOption.Location when parent is GameLocation location && !location.Equals(Game1.currentLocation) =>
                false,
            RangeOption.Location when distance == -1 => true,
            RangeOption.Location when Math.Abs(position.X - Game1.player.Tile.X)
                + Math.Abs(position.Y - Game1.player.Tile.Y)
                <= distance => true,
            _ => false,
        };

    //
    // /// <summary>Add a chest capacity option at the current position in the form.</summary>
    // /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    // /// <param name="manifest">Dependency for accessing mod manifest.</param>
    // /// <param name="data">The storage data.</param>
    // public static void AddChestCapacityOption(
    //     this GenericModConfigMenuIntegration gmcm,
    //     IManifest manifest,
    //     IStorageData data) =>
    //     gmcm.Api!.AddNumberOption(
    //         manifest,
    //         data.GetChestCapacity,
    //         data.SetChestCapacity,
    //         I18n.Config_ResizeChestCapacity_Name,
    //         I18n.Config_ResizeChestCapacity_Tooltip,
    //         0,
    //         8,
    //         1,
    //         Formatting.ChestCapacity);
    //
    // /// <summary>Add a distance option at the current position in the form.</summary>
    // /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    // /// <param name="manifest">Dependency for accessing mod manifest.</param>
    // /// <param name="data">The storage data.</param>
    // /// <param name="featureName">The feature which the distance is associated with.</param>
    // /// <param name="name">The label text to show in the form.</param>
    // /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    // public static void AddDistanceOption(
    //     this GenericModConfigMenuIntegration gmcm,
    //     IManifest manifest,
    //     IStorageData data,
    //     string featureName,
    //     Func<string> name,
    //     Func<string> tooltip) =>
    //     gmcm.Api!.AddNumberOption(
    //         manifest,
    //         () => data.GetDistance(featureName),
    //         value => data.SetDistance(featureName, value),
    //         name,
    //         tooltip,
    //         (int)RangeOption.Default,
    //         (int)RangeOption.World,
    //         1,
    //         Formatting.Distance);
    //

    // /// <summary>Add a feature option range at the current position in the form.</summary>
    // /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    // /// <param name="manifest">Dependency for accessing mod manifest.</param>
    // /// <param name="getValue">Get the current value from the mod config.</param>
    // /// <param name="setValue">Set a new value in the mod config.</param>
    // /// <param name="name">The label text to show in the form.</param>
    // /// <param name="tooltip">The tooltip text shown when the cursor hovers on the field.</param>
    // public static void AddRangeOption(
    //     this GenericModConfigMenuIntegration gmcm,
    //     IManifest manifest,
    //     Func<RangeOption> getValue,
    //     Action<RangeOption> setValue,
    //     Func<string> name,
    //     Func<string> tooltip) =>
    //     gmcm.Api!.AddTextOption(
    //         manifest,
    //         () => getValue().ToStringFast(),
    //         value => setValue(
    //             RangeOptionExtensions.TryParse(value, out var range) ? range : RangeOption.Default),
    //         name,
    //         tooltip,
    //         RangeOptionExtensions.GetNames(),
    //         Formatting.Range);
    //
    // /// <summary>Gets storage distance from a player in tiles.</summary>
    // /// <param name="storage">The storage to get the distance for.</param>
    // /// <param name="player">The player to get the distance from.</param>
    // /// <returns>Returns the distance in tiles.</returns>
    // public static int GetDistanceToPlayer(this StorageNode storage, Farmer player)
    // {
    //     if (storage is not
    //         {
    //             Data: Storage storageObject,
    //         })
    //     {
    //         return 0;
    //     }
    //
    //     return (int)(Math.Abs(storageObject.Position.X - player.Tile.X)
    //         + Math.Abs(storageObject.Position.Y - player.Tile.Y));
    // }
    //
    // /// <summary>Stashes an item into storage based on categorization and stack settings.</summary>
    // /// <param name="storage">The storage to stash an item into.</param>
    // /// <param name="monitor">Dependency used for monitoring and logging.</param>
    // /// <param name="item">The item to stash.</param>
    // /// <param name="existingStacks">Whether to stash into stackable items or based on categorization.</param>
    // /// <returns>Returns the <see cref="Item" /> if not all could be stashed or null if successful.</returns>
    // public static Item? StashItem(this StorageNode storage, IMonitor monitor, Item item, bool existingStacks = false)
    // {
    //     // Disallow stashing of any Chest.
    //     if (storage is not
    //         {
    //             Data: Storage storageObject,
    //         }
    //         || item is Chest
    //             or SObject
    //             {
    //                 heldObject.Value: Chest,
    //             })
    //     {
    //         return item;
    //     }
    //
    //     var stack = item.Stack;
    //     var tmp = (existingStacks && storageObject.Inventory.Any(otherItem => otherItem?.canStackWith(item) == true))
    //         || (storage.FilterItemsList.Any()
    //             && !storage.FilterItemsList.All(filter => filter.StartsWith("!", StringComparison.OrdinalIgnoreCase))
    //             && storage.FilterMatches(item))
    //             ? storageObject.AddItem(item)
    //             : item;
    //
    //     if (tmp is null || stack != item.Stack)
    //     {
    //         monitor.Log(
    //             $"StashItem: {{ Item: {item.Name}, Quantity: {Math.Max(1, stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, To: {storage}");
    //     }
    //
    //     return tmp;
    // }
    //
    // /// <summary>Checks if the <see cref="Item" /> can be donated to a <see cref="CommunityCenter" /> bundle.</summary>
    // /// <param name="item">The item to check.</param>
    // /// <returns>Returns true if the item can be donated.</returns>
    // private static bool CanDonateToBundle(this Item item) =>
    //     item is SObject obj
    //     && (Game1.locations.OfType<CommunityCenter>().FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
    //         ?? false);
    //
    // /// <summary>Checks if the <see cref="Item" /> can be donated to the <see cref="LibraryMuseum" />.</summary>
    // /// <param name="item">The item to check.</param>
    // /// <returns>Returns true if the item can be donated.</returns>
    // private static bool CanDonateToMuseum(this Item item) =>
    //     Game1.locations.OfType<LibraryMuseum>().FirstOrDefault()?.isItemSuitableForDonation(item) ?? false;
    //
    // private static int GetChestCapacity(this IStorageData data) =>
    //     data.ResizeChestCapacity switch
    //     {
    //         _ when data.ResizeChest is Option.Default => (int)Option.Default,
    //         _ when data.ResizeChest is Option.Disabled => (int)Option.Disabled,
    //         -1 => 8,
    //         _ => ((int)Option.Enabled + (data.ResizeChestCapacity / 12)) - 1,
    //     };
    //
    // private static int GetDistance(this IStorageData data, string featureName)
    // {
    //     var feature = featureName switch
    //     {
    //         nameof(CraftFromChest) => data.CraftFromChest,
    //         nameof(StashToChest) => data.StashToChest,
    //         _ => throw new ArgumentException($"Invalid feature {featureName}"),
    //     };
    //
    //     var distance = featureName switch
    //     {
    //         nameof(CraftFromChest) => data.CraftFromChestDistance,
    //         nameof(StashToChest) => data.StashToChestDistance,
    //         _ => 0,
    //     };
    //
    //     return distance switch
    //     {
    //         _ when feature is RangeOption.Default => (int)RangeOption.Default,
    //         _ when feature is RangeOption.Disabled => (int)RangeOption.Disabled,
    //         _ when feature is RangeOption.Inventory => (int)RangeOption.Inventory,
    //         _ when feature is RangeOption.World => (int)RangeOption.World,
    //         >= 2 when feature is RangeOption.Location => ((int)RangeOption.Location
    //                 + (int)Math.Ceiling(Math.Log2(distance)))
    //             - 1,
    //         _ when feature is RangeOption.Location => (int)RangeOption.World - 1,
    //         _ => (int)RangeOption.Default,
    //     };
    // }
    //
    // /// <summary>Checks if the <see cref="Item" /> is an artifact.</summary>
    // /// <param name="item">The item to check.</param>
    // /// <returns>Returns true if the item is an artifact.</returns>
    // private static bool IsArtifact(this Item item) =>
    //     item is SObject
    //     {
    //         Type: "Arch",
    //     };
    //
    // /// <summary>Checks if the <see cref="Item" /> is <see cref="Furniture" />.</summary>
    // /// <param name="item">The item to check.</param>
    // /// <returns>Returns true if the item is furniture.</returns>
    // private static bool IsFurniture(this Item item) => item is Furniture;
    //
    // private static void SetChestCapacity(this IStorageData data, int value)
    // {
    //     data.ResizeChestCapacity = value switch
    //     {
    //         (int)Option.Default => 0,
    //         (int)Option.Disabled => 0,
    //         8 => -1,
    //         >= (int)Option.Enabled => 12 * ((1 + value) - (int)Option.Enabled),
    //         _ => 0,
    //     };
    //
    //     data.ResizeChest = value switch
    //     {
    //         (int)Option.Default => Option.Default,
    //         (int)Option.Disabled => Option.Disabled,
    //         _ => Option.Enabled,
    //     };
    // }
    //
    // private static void SetDistance(this IStorageData data, string featureName, int value)
    // {
    //     var distance = value switch
    //     {
    //         (int)RangeOption.Default => 0,
    //         (int)RangeOption.Disabled => 0,
    //         (int)RangeOption.Inventory => 0,
    //         (int)RangeOption.World - 1 => -1,
    //         (int)RangeOption.World => 0,
    //         >= (int)RangeOption.Location => (int)Math.Pow(2, (1 + value) - (int)RangeOption.Location),
    //         _ => 0,
    //     };
    //
    //     var range = value switch
    //     {
    //         (int)RangeOption.Default => RangeOption.Default,
    //         (int)RangeOption.Disabled => RangeOption.Disabled,
    //         (int)RangeOption.Inventory => RangeOption.Inventory,
    //         (int)RangeOption.World => RangeOption.World,
    //         (int)RangeOption.World - 1 => RangeOption.Location,
    //         _ => RangeOption.Location,
    //     };
    //
    //     switch (featureName)
    //     {
    //         case nameof(CraftFromChest):
    //             data.CraftFromChestDistance = distance;
    //             data.CraftFromChest = range;
    //             return;
    //
    //         case nameof(StashToChest):
    //             data.StashToChestDistance = distance;
    //             data.StashToChest = range;
    //             return;
    //
    //         default:
    //             throw new ArgumentException($"Invalid feature {featureName}");
    //     }
    // }
}