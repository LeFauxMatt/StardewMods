namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Mods;

/// <inheritdoc />
internal sealed class FarmerContainer : BaseContainer<Farmer>
{
    /// <summary>Initializes a new instance of the <see cref="FarmerContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    /// <param name="farmer">The farmer whose inventory is holding the container.</param>
    public FarmerContainer(ItemMatcher itemMatcher, IStorage storageType, Farmer farmer)
        : base(itemMatcher, storageType, farmer.Items) =>
        this.Source = new WeakReference<Farmer>(farmer);

    /// <summary>Gets the farmer container of the storage.</summary>
    public Farmer Farmer => this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(FarmerContainer));

    /// <inheritdoc />
    public override GameLocation Location => this.Farmer.currentLocation;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Farmer.Tile;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Farmer.modData;

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<Farmer> Source { get; }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        remaining = this.Farmer.addItemToInventory(item);
        return remaining is null || remaining.Stack != stack;
    }
}