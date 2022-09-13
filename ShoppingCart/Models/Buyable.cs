namespace StardewMods.ShoppingCart.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.ShoppingCart;

/// <inheritdoc />
internal sealed class Buyable : IBuyable
{
    private readonly ICartItem _cartItem;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Buyable" /> class.
    /// </summary>
    /// <param name="item">The item to purchase.</param>
    /// <param name="quantity">The quantity of the item to purchase.</param>
    /// <param name="priceAndStock">The shop prices and stock.</param>
    public Buyable(ISalable item, int quantity, IReadOnlyList<int> priceAndStock)
    {
        this._cartItem = new CartItem(item, quantity, priceAndStock[0], priceAndStock[1]);

        if (priceAndStock.Count <= 2)
        {
            return;
        }

        this.ExtraItem = priceAndStock[2];
        this.ExtraItemAmount = priceAndStock.Count > 3 ? priceAndStock[3] : 5;
    }

    /// <inheritdoc />
    public int Available => this._cartItem.Available;

    /// <inheritdoc />
    public int ExtraItem { get; }

    /// <inheritdoc />
    public int ExtraItemAmount { get; }

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
    public bool TestBuy(
        IList<Item?> inventory,
        ref int qiGems,
        ref int walnuts,
        int index,
        Func<int, bool>? canPurchaseCheck)
    {
        if (index != -1 && canPurchaseCheck is not null && !canPurchaseCheck(index))
        {
            return false;
        }

        // Check affordability
        var extraItemAmount = this.ExtraItemAmount * this.Quantity;
        switch (this.ExtraItem)
        {
            case 858:
                qiGems -= extraItemAmount;
                if (qiGems < 0)
                {
                    return false;
                }

                break;
            case 73:
                walnuts -= extraItemAmount;
                if (walnuts < 0)
                {
                    return false;
                }

                break;
            case > 0:
                foreach (var item in inventory.OfType<SObject>().Where(obj => obj.ParentSheetIndex == this.ExtraItem))
                {
                    if (item.Stack >= extraItemAmount)
                    {
                        item.Stack -= extraItemAmount;
                        extraItemAmount = 0;
                        break;
                    }

                    extraItemAmount -= item.Stack;
                    item.Stack = 0;
                    if (extraItemAmount == 0)
                    {
                        break;
                    }
                }

                if (extraItemAmount > 0)
                {
                    return false;
                }

                break;
        }

        var quantity = this.Quantity;
        var maxStack = Math.Max(1, this.Item.maximumStackSize());

        // Add to existing stacks
        if (maxStack > 1)
        {
            foreach (var item in inventory.OfType<Item>().Where(this.Item.IsEquivalentTo))
            {
                if (item.Stack + quantity <= maxStack)
                {
                    item.Stack += quantity;
                    quantity = 0;
                    break;
                }

                quantity -= maxStack - item.Stack;
                item.Stack = maxStack;
                if (quantity == 0)
                {
                    break;
                }
            }
        }

        // Add remaining to inventory
        for (var i = 0; i < inventory.Count; ++i)
        {
            if (inventory[i] is not null)
            {
                continue;
            }

            var clone = (Item)this.Item.GetSalableInstance();
            if (maxStack > 1)
            {
                clone.Stack = Math.Min(quantity, maxStack);
                quantity -= clone.Stack;
            }
            else
            {
                quantity--;
            }

            inventory[i] = clone;
            if (quantity == 0)
            {
                break;
            }
        }

        return quantity == 0;
    }
}