namespace StardewMods.Common.Integrations.ShoppingCart;

using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>A representation of a <see cref="ShopMenu" />.</summary>
public interface IShop
{
    /// <summary>Gets the area this ShoppingCart menu occupies.</summary>
    public Rectangle Bounds { get; }

    /// <summary>Gets the actual ShopMenu this ShoppingCart is attached to.</summary>
    public ShopMenu Menu { get; }

    /// <summary>Gets items being bought in the ShoppingCart.</summary>
    public IEnumerable<ICartItem> ToBuy { get; }

    /// <summary>Gets items being sold in the ShoppingCart.</summary>
    public IEnumerable<ICartItem> ToSell { get; }

    /// <summary>Try adding an item to the shopping cart to buy.</summary>
    /// <param name="toBuy">The item to buy.</param>
    /// <param name="quantity">The quantity of the item to buy.</param>
    /// <returns>Returns true if the item can be purchased.</returns>
    public bool AddToCart(ISalable toBuy, int quantity);

    /// <summary>Try adding an item to the shopping cart to sell.</summary>
    /// <param name="toSell">The item to sell.</param>
    /// <returns>Returns true if the item can be sold.</returns>
    public bool AddToCart(Item toSell);
}
