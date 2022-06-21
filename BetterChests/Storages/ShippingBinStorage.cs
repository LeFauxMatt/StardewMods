namespace StardewMods.BetterChests.Storages;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ShippingBinStorage : BaseStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="location">The location of the shipping bin.</param>
    public ShippingBinStorage(GameLocation location)
        : base(location)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="shippingBin">The shipping bin.</param>
    public ShippingBinStorage(ShippingBin shippingBin)
        : base(shippingBin)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="chest">The mini-shipping bin.</param>
    public ShippingBinStorage(Chest chest)
        : base(chest)
    {
    }

    /// <inheritdoc />
    public override int Capacity
    {
        get => this.Context switch
        {
            Chest chest => chest.GetActualCapacity(),
            _ => int.MaxValue,
        };
    }

    /// <inheritdoc />
    public override IList<Item?> Items
    {
        get => this.Context switch
        {
            Chest chest => chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID),
            _ => Game1.getFarm().getShippingBin(Game1.player),
        };
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Context switch
        {
            Building building => building.modData,
            GameLocation location => location.modData,
            Chest chest => chest.modData,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    /// <inheritdoc />
    public override Item? AddItem(Item item)
    {
        if (!Utility.highlightShippableObjects(item))
        {
            return item;
        }

        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem is not null && existingItem.canStackWith(item)))
        {
            item.Stack = existingItem!.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Items.Count < this.Capacity)
        {
            this.Items.Add(item);
            return null;
        }

        return item;
    }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        Game1.activeClickableMenu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            Utility.highlightShippableObjects,
            this.GrabInventoryItem,
            null,
            this.GrabStorageItem,
            false,
            true,
            true,
            true,
            true,
            0,
            null,
            -1,
            this.Context);
    }
}