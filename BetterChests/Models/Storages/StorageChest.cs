namespace StardewMods.BetterChests.Models.Storages;

using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Interfaces;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageChest : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageChest" /> class.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> managed by this mod.</param>
    /// <param name="data">The <see cref="IStorageData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this chest type.</param>
    public StorageChest(Chest chest, IStorageData data, string qualifiedItemId)
        : base(chest, data, qualifiedItemId)
    {
        this.Chest = chest;
        this.InitFilterItems();
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <summary>
    ///     Gets the actual instance of the <see cref="Chest" /> being managed.
    /// </summary>
    public Chest Chest { get; }

    /// <inheritdoc />
    public override List<Item> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).ToList();
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Chest.modData;
    }

    /// <inheritdoc/>
    public override void ShowMenu()
    {
        this.Chest.ShowMenu();
    }
}