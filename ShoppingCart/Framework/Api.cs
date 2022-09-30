namespace StardewMods.ShoppingCart.Framework;

using StardewMods.Common.Integrations.ShoppingCart;

/// <inheritdoc />
public sealed class Api : IShoppingCartApi
{
    /// <inheritdoc />
    public IShop? CurrentShop => ModEntry.CurrentShop;
}