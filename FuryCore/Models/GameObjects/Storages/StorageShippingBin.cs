namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System.Linq;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class StorageShippingBin : StorageContainer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageShippingBin" /> class.
    /// </summary>
    /// <param name="shippingBin">The shipping bin.</param>
    public StorageShippingBin(ShippingBin shippingBin)
        : base(shippingBin, () => int.MaxValue, () => Game1.getFarm().getShippingBin(Game1.player), () => shippingBin.modData)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageShippingBin" /> class.
    /// </summary>
    /// <param name="chest">The mini-shipping bin.</param>
    public StorageShippingBin(Chest chest)
        : base(chest, () => chest.modData)
    {
    }

    /// <inheritdoc />
    public override Item AddItem(Item item)
    {
        if (!Utility.highlightShippableObjects(item))
        {
            return item;
        }

        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            item.Stack = existingItem.addToStack(item);
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