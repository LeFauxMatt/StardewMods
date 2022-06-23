namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc />
internal class ChestStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ChestStorage" /> class.
    /// </summary>
    /// <param name="chest">The source chest.</param>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    /// <param name="location">The location of the source object.</param>
    /// <param name="position">The position of the source object.</param>
    public ChestStorage(Chest chest, IStorageData defaultChest, GameLocation? location = default, Vector2? position = default)
        : base(chest is { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } ? Game1.player.team : chest, location, position, defaultChest)
    {
        this.Chest = chest;
    }

    /// <inheritdoc/>
    public override int ActualCapacity
    {
        get => this.Chest.GetActualCapacity();
    }

    /// <summary>
    ///     Gets the source chest object.
    /// </summary>
    public Chest Chest { get; }

    /// <inheritdoc/>
    public override IList<Item?> Items
    {
        get => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
    }

    /// <inheritdoc/>
    public override ModDataDictionary ModData
    {
        get => this.Chest.modData;
    }

    /// <inheritdoc/>
    public override NetMutex? Mutex
    {
        get => this.Chest.GetMutex();
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