namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Mods;
using StardewValley.Objects;

/// <inheritdoc />
internal class ChestContainer : BaseContainer<Chest>
{
    /// <summary>Initializes a new instance of the <see cref="ChestContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public ChestContainer(ItemMatcher itemMatcher, IStorage storageType, Chest chest)
        : base(itemMatcher, storageType, chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)) =>
        this.Source = new WeakReference<Chest>(chest);

    /// <summary>Gets the chest container of the storage.</summary>
    public Chest Chest => this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(ChestContainer));

    /// <inheritdoc />
    public override GameLocation Location => this.Chest.Location;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Chest.TileLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Chest.modData;

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<Chest> Source { get; }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        remaining = this.Chest.addItem(item);
        return remaining is null || remaining.Stack != stack;
    }
}