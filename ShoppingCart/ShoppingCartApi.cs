namespace StardewMods.ShoppingCart;

using StardewMods.Common.Integrations.ShoppingCart;

/// <inheritdoc />
public sealed class ShoppingCartApi : IShoppingCartApi
{
    /// <inheritdoc />
    public IShop? CurrentShop => ShoppingCart.CurrentShop;
}