namespace StardewMods.ShoppingCart.Models;

using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class Sellable : ISellable
{
    private readonly ICartItem _cartItem;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Sellable" /> class.
    /// </summary>
    /// <param name="item">The item to sell.</param>
    /// <param name="sellPercentage">The shop's sell percentage modifier.</param>
    /// <param name="inventory">The player's inventory selling the item.</param>
    public Sellable(ISalable item, float sellPercentage, IEnumerable<Item?> inventory)
    {
        this._cartItem = new CartItem(
            item.GetSalableInstance(),
            item.Stack,
            item switch
            {
                SObject obj => (int)(obj.sellToStorePrice() * sellPercentage),
                _ => (int)(item.salePrice() / 2f * sellPercentage),
            },
            (item.Stack > 0 ? item.Stack : 1)
          + inventory.OfType<Item>().Where(item.IsEquivalentTo).Sum(i => i.Stack > 0 ? i.Stack : 1));
    }

    /// <inheritdoc />
    public int Available => this._cartItem.Available;

    /// <inheritdoc />
    public ISalable Item => this._cartItem.Item;

    /// <inheritdoc />
    public int Price => this._cartItem.Price;

    /// <inheritdoc />
    public int Quantity
    {
        get => this._cartItem.Quantity;
        set => this._cartItem.Quantity = value;
    }

    /// <inheritdoc />
    public long Total => this._cartItem.Total;

    /// <inheritdoc />
    public int CompareTo(ICartItem? other)
    {
        return this._cartItem.CompareTo(other);
    }

    /// <inheritdoc />
    public bool TrySell(IList<Item?> inventory, int currency, bool test = false)
    {
        var quantity = this.Quantity;
        for (var i = 0; i < inventory.Count; ++i)
        {
            if (inventory[i] is not { } item || !this.Item.IsEquivalentTo(item))
            {
                continue;
            }

            if (item.Stack <= quantity)
            {
                inventory[i] = null;
                quantity -= item.Stack;
                continue;
            }

            item.Stack -= quantity;
            quantity = 0;
        }

        if (!test)
        {
            ShopMenu.chargePlayer(Game1.player, currency, -this.Price * (this.Quantity - quantity));
        }

        return quantity == 0;
    }
}