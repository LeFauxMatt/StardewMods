namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Mods;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class FridgeContainer : ChestContainer
{
    /// <summary>Initializes a new instance of the <see cref="FridgeContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    /// <param name="location">The game location where the fridge storage is located.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public FridgeContainer(ItemMatcher itemMatcher, IStorage storageType, GameLocation location, Chest chest)
        : base(itemMatcher, storageType, chest) =>
        this.Location = location;

    /// <inheritdoc />
    public override GameLocation Location { get; }

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Location.GetFridgePosition()?.ToVector2() ?? Vector2.Zero;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Location.modData;
}
