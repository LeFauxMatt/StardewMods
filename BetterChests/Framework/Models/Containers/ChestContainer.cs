namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class ChestContainer : BaseContainer<Chest>
{
    /// <summary>Initializes a new instance of the <see cref="ChestContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    /// <param name="chest">The chest storage of the container.</param>
    public ChestContainer(IStorageOptions baseOptions, Chest chest)
        : base(baseOptions) =>
        this.Source = new WeakReference<Chest>(chest);

    /// <summary>Gets the source chest of the container.</summary>
    public Chest Chest =>
        this.Source.TryGetTarget(out var target) ? target : throw new ObjectDisposedException(nameof(ChestContainer));

    /// <inheritdoc />
    public override int Capacity => this.Chest.GetActualCapacity();

    /// <inheritdoc />
    public override IInventory Items => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <inheritdoc />
    public override GameLocation Location => this.Chest.Location;

    /// <inheritdoc />
    public override Vector2 TileLocation => this.Chest.TileLocation;

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Chest.modData;

    /// <inheritdoc />
    public override NetMutex Mutex => this.Chest.GetMutex();

    /// <inheritdoc />
    public override bool IsAlive => this.Source.TryGetTarget(out _);

    /// <inheritdoc />
    public override WeakReference<Chest> Source { get; }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        Game1.player.currentLocation.localSound("openChest");
        this.Chest.ShowMenu();
    }

    /// <inheritdoc />
    public override bool TryAdd(Item item, out Item? remaining)
    {
        var stack = item.Stack;
        remaining = this.Chest.addItem(item);
        return remaining is null || remaining.Stack != stack;
    }

    /// <inheritdoc />
    public override bool TryRemove(Item item)
    {
        if (!this.Items.Contains(item))
        {
            return false;
        }

        this.Items.Remove(item);
        this.Items.RemoveEmptySlots();
        return true;
    }
}