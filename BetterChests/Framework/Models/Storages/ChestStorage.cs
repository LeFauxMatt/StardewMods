namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class ChestStorage : ObjectStorage
{
    private readonly Chest chest;

    /// <summary>Initializes a new instance of the <see cref="ChestStorage" /> class.</summary>
    /// <param name="chest">The chest object to be stored.</param>
    public ChestStorage(Chest chest) : base(chest) => this.chest = chest;

    /// <inheritdoc />
    public override IEnumerable<Item> Items => this.chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <inheritdoc />
    public override NetMutex Mutex => this.chest.GetMutex();
}
