namespace StardewMods.ExpandedStorage.Models;

/// <summary>
///     Config data for an Expanded Storage chest.
/// </summary>
internal sealed class StorageConfig
{
    /// <summary>
    ///     Gets or sets data for integration with Better Chests.
    /// </summary>
    public BetterChestsData? BetterChestsData { get; set; }
}