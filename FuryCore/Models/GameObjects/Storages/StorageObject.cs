namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class StorageObject : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageObject" /> class.
    /// </summary>
    /// <param name="obj">The source object.</param>
    public StorageObject(SObject obj)
        : base(obj)
    {
        this.Object = obj;
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <inheritdoc />
    public override IList<Item> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Object.modData;
    }

    /// <summary>
    ///     Gets the source object.
    /// </summary>
    public SObject Object { get; }

    private Chest Chest
    {
        get => (Chest)this.Object.heldObject.Value;
    }
}