namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <inheritdoc />
internal sealed class FarmerContainer : BaseContainer<Farmer>
{
    /// <summary>Initializes a new instance of the <see cref="FarmerContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="farmer">The farmer whose inventory is holding the container.</param>
    public FarmerContainer(IStorageOptions baseOptions, Farmer farmer)
        : base(baseOptions) =>
        this.Source = new WeakReference<Farmer>(farmer);

    /// <summary>Gets the farmer container of the storage.</summary>
    public Farmer Farmer =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(FarmerContainer));

    /// <inheritdoc />
    public override int Capacity => this.Farmer.MaxItems;

    /// <inheritdoc />
    public override IInventory Items => this.Farmer.Items;

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

    /// <inheritdoc />
    public override bool TryRemove(Item item)
    {
        if (!this.Items.Contains(item))
        {
            return false;
        }

        this.Farmer.removeItemFromInventory(item);
        return true;
    }
}