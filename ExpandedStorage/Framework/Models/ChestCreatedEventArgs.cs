namespace StardewMods.ExpandedStorage.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewValley.Objects;

/// <inheritdoc cref="IChestCreatedEventArgs" />
internal sealed class ChestCreatedEventArgs(
    Chest chest,
    GameLocation location,
    Vector2 tileLocation,
    IStorageData storageData) : EventArgs, IChestCreatedEventArgs
{
    /// <inheritdoc />
    public Chest Chest { get; } = chest;

    /// <inheritdoc />
    public GameLocation Location { get; } = location;

    /// <inheritdoc />
    public Vector2 TileLocation { get; } = tileLocation;

    /// <inheritdoc />
    public IStorageData StorageData { get; } = storageData;
}