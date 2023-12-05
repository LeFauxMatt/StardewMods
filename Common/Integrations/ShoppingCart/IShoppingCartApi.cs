namespace StardewMods.Common.Integrations.ShoppingCart;

/// <summary>API for Shopping Cart.</summary>
public interface IShoppingCartApi
{
    /// <summary>Gets the current shop.</summary>
    IShop? CurrentShop { get; }
}
