namespace StardewMods.ShoppingCart.Models;

using System.Collections.Generic;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewMods.ShoppingCart.Framework;
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
            Sellable.GetAvailable(item, inventory));
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
            if (inventory[i] is not { } item)
            {
                continue;
            }

            if (Integrations.StackQuality.IsLoaded
             && item is SObject obj
             && this.Item is SObject otherObj
             && otherObj.canStackWith(obj))
            {
                var stacks = Integrations.StackQuality.API.GetStacks(obj);
                if (stacks[otherObj.Quality == 4 ? 3 : otherObj.Quality] <= quantity)
                {
                    quantity -= stacks[otherObj.Quality == 4 ? 3 : otherObj.Quality];
                    stacks[otherObj.Quality == 4 ? 3 : otherObj.Quality] = 0;
                    Integrations.StackQuality.API.UpdateQuality(obj, stacks);
                    continue;
                }

                stacks[otherObj.Quality == 4 ? 3 : otherObj.Quality] -= quantity;
                quantity = 0;
                Integrations.StackQuality.API.UpdateQuality(obj, stacks);
                continue;
            }

            if (!this.Item.IsEquivalentTo(item))
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

    private static int GetAvailable(ISalable heldItem, IEnumerable<Item?> inventory)
    {
        var obj = heldItem as SObject;
        var available = 0;

        if (Integrations.StackQuality.IsLoaded && obj is not null)
        {
            var stacks = Integrations.StackQuality.API.GetStacks(obj);
            available += stacks[obj.Quality == 4 ? 3 : obj.Quality];
        }
        else
        {
            available += heldItem.Stack > 0 ? heldItem.Stack : 1;
        }

        foreach (var item in inventory)
        {
            if (item is null)
            {
                continue;
            }

            if (Integrations.StackQuality.IsLoaded
             && heldItem.canStackWith(item)
             && obj is not null
             && item is SObject otherObj)
            {
                var stacks = Integrations.StackQuality.API.GetStacks(otherObj);
                available += stacks[obj.Quality == 4 ? 3 : obj.Quality];
                continue;
            }

            if (heldItem.IsEquivalentTo(item))
            {
                available += item.Stack > 0 ? item.Stack : 1;
            }
        }

        return available;
    }
}