namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Mods;
using StardewValley.Objects;

/// <inheritdoc />
internal class ObjectContainer : ChestContainer
{
    private readonly SObject obj;

    /// <summary>Initializes a new instance of the <see cref="ObjectContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    /// <param name="obj">The storage object.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public ObjectContainer(ItemMatcher itemMatcher, IStorage storageType, SObject obj, Chest chest)
        : base(itemMatcher, storageType, chest) =>
        this.obj = obj;

    /// <inheritdoc />
    public override GameLocation Location => this.obj.Location;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.obj.TileLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.obj.modData;
}