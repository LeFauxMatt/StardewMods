namespace StardewMods.Common.Integrations.ShoppingCart;

/// <summary>Represents an item being bought or sold.</summary>
public interface ICartItem : IComparable<ICartItem>
{
    /// <summary>Gets available quantity of the item.</summary>
    public int Available { get; }

    /// <summary>Gets the item.</summary>
    public ISalable Item { get; }

    /// <summary>Gets the individual sale price of an item.</summary>
    public int Price { get; }

    /// <summary>Gets or sets the quantity to buy/sell.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets the total price.</summary>
    public long Total { get; }
}
