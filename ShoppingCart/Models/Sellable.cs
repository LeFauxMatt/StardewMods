namespace StardewMods.ShoppingCart.Models;

using System;
using System.Collections.Generic;
using System.Linq;
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
            int toTake;

            // Stack Quality Integration
            if (Integrations.StackQuality.IsLoaded
             && inventory[i] is SObject obj
             && Integrations.StackQuality.API.EquivalentObjects(obj, this.Item)
             && Integrations.StackQuality.API.GetStacks(obj, out var stacks))
            {
                var quality = ((SObject)this.Item).Quality;
                toTake = Math.Min(quantity, stacks[quality == 4 ? 3 : quality]);
                quantity -= toTake;
                stacks[quality == 4 ? 3 : quality] -= toTake;

                if (stacks.Sum() == 0)
                {
                    inventory[i] = null;
                    continue;
                }

                Integrations.StackQuality.API.UpdateStacks(obj, stacks);
                continue;
            }

            if (inventory[i] is not { } item || !this.Item.IsEquivalentTo(item))
            {
                continue;
            }

            toTake = Math.Min(quantity, item.Stack);
            item.Stack -= toTake;
            quantity -= toTake;
            if (item.Stack == 0)
            {
                inventory[i] = null;
            }
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
        var available = obj is not null
                     && Integrations.StackQuality.IsLoaded
                     && Integrations.StackQuality.API.GetStacks(obj, out var stacks)
            ? stacks[obj.Quality == 4 ? 3 : obj.Quality]
            : heldItem.Stack > 0
                ? heldItem.Stack
                : 1;

        foreach (var item in inventory)
        {
            if (item is null)
            {
                continue;
            }

            if (obj is not null
             && Integrations.StackQuality.IsLoaded
             && Integrations.StackQuality.API.EquivalentObjects(obj, item)
             && Integrations.StackQuality.API.GetStacks((SObject)item, out stacks))
            {
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