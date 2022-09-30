namespace StardewMods.ShoppingCart.Framework.Models;

using System;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewMods.Common.Models;

/// <inheritdoc />
internal sealed class CartItem : ICartItem
{
    private readonly Range<int> _range;

    private int _quantity;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CartItem" /> class.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="quantity">The amount of the item to buy or sell.</param>
    /// <param name="price">The item price.</param>
    /// <param name="available">The quantity of the item available.</param>
    public CartItem(ISalable item, int quantity, int price, int available)
    {
        this._range = new(0, available);
        this.Item = item;
        this.Price = price;
        this.Quantity = Math.Max(1, quantity);
    }

    /// <inheritdoc />
    public int Available => this._range.Maximum;

    /// <inheritdoc />
    public ISalable Item { get; }

    /// <inheritdoc />
    public int Price { get; }

    /// <inheritdoc />
    public int Quantity
    {
        get => this._quantity;
        set => this._quantity = this._range.Clamp(value);
    }

    /// <inheritdoc />
    public long Total => this.Price * (long)this.Quantity;

    /// <inheritdoc />
    public int CompareTo(ICartItem? other)
    {
        if (ReferenceEquals(null, other))
        {
            return -1;
        }

        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        var item = this.Item as Item ?? (Item)this.Item.GetSalableInstance();
        var otherItem = other.Item as Item ?? (Item)other.Item.GetSalableInstance();
        var category = item.getCategoryName();
        var otherCategory = otherItem.getCategoryName();
        if (string.IsNullOrWhiteSpace(category))
        {
            category = I18n.Ui_OtherCategory();
        }

        if (string.IsNullOrWhiteSpace(otherCategory))
        {
            otherCategory = I18n.Ui_OtherCategory();
        }

        return !category.Equals(otherCategory, StringComparison.OrdinalIgnoreCase)
            ? string.Compare(category, otherCategory, StringComparison.OrdinalIgnoreCase)
            : string.Compare(item.DisplayName, otherItem.DisplayName, StringComparison.OrdinalIgnoreCase);
    }
}