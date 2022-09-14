namespace StardewMods.Common.Integrations.ShoppingCart;

/// <inheritdoc />
internal sealed class ShoppingCartIntegration : ModIntegration<IShoppingCartApi>
{
    private const string ModUniqueId = "furyx639.ShoppingCart";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShoppingCartIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ShoppingCartIntegration(IModRegistry modRegistry)
        : base(modRegistry, ShoppingCartIntegration.ModUniqueId)
    {
        // Nothing
    }
}