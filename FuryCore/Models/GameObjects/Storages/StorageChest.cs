namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageChest : StorageContainer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageChest" /> class.
    /// </summary>
    /// <param name="chest">The source chest.</param>
    /// <param name="context">The source object.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    public StorageChest(Chest chest, object context = null, Func<ModDataDictionary> getModData = null)
        : base(context ?? chest, getModData ?? (() => chest.modData))
    {
        this.Chest = chest;
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

    private Chest Chest { get; }
}