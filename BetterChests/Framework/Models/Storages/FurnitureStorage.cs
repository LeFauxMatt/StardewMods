namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class FurnitureStorage : ObjectStorage
{
    private readonly StorageFurniture furniture;

    /// <summary>Initializes a new instance of the <see cref="FurnitureStorage" /> class.</summary>
    /// <param name="furniture">The storage furniture item to be used for this storage.</param>
    public FurnitureStorage(StorageFurniture furniture) : base(furniture) => this.furniture = furniture;

    /// <inheritdoc />
    public override IEnumerable<Item> Items => this.furniture.heldItems;

    /// <inheritdoc />
    public override NetMutex Mutex => this.furniture.mutex;
}
