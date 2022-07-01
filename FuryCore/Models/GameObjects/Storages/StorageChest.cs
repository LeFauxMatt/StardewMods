#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageChest : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageChest" /> class.
    /// </summary>
    /// <param name="chest">The source chest.</param>
    public StorageChest(Chest chest)
        : base(chest)
    {
        this.Chest = chest;
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <summary>
    ///     Gets the source chest object.
    /// </summary>
    public Chest Chest { get; }

    /// <inheritdoc />
    public override IList<Item> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Chest.modData;
    }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        Game1.activeClickableMenu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            InventoryMenu.highlightAllItems,
            this.Chest.grabItemFromInventory,
            null,
            this.Chest.grabItemFromChest,
            false,
            true,
            true,
            true,
            true,
            1,
            this.Chest,
            -1,
            this.Context);
    }
}