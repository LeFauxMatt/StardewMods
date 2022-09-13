namespace StardewMods.Common.Integrations.ShoppingCart;

using System;
using System.Collections.Generic;

/// <summary>
///     Represents an item being bought.
/// </summary>
public interface IBuyable : ICartItem
{
    /// <summary>
    ///     Gets any extra items required to purchase this item.
    /// </summary>
    public int ExtraItem { get; }

    /// <summary>
    ///     Gets the quantity of extra items per quantity of item to purchase.
    /// </summary>
    public int ExtraItemAmount { get; }

    /// <summary>
    ///     Tests if an item can be bought.
    /// </summary>
    /// <param name="inventory">The inventory to buy items to.</param>
    /// <param name="qiGems">The number of Qi Gems.</param>
    /// <param name="walnuts">The number of Golden Walnuts.</param>
    /// <param name="index">The shop index of the item.</param>
    /// <param name="canPurchaseCheck">Function to test if item can be purchased..</param>
    /// <returns>Returns true if item can be bought.</returns>
    public bool TestBuy(
        IList<Item?> inventory,
        ref int qiGems,
        ref int walnuts,
        int index,
        Func<int, bool>? canPurchaseCheck);
}