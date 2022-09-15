namespace StardewMods.StackQuality.Framework;

using StardewMods.Common.Integrations.ShoppingCart;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
    private static Integrations? Instance;

    private readonly IModHelper _helper;
    private readonly ShoppingCartIntegration _shoppingCartIntegration;

    private Integrations(IModHelper helper)
    {
        this._helper = helper;
        this._shoppingCartIntegration = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Stack Quality integration.
    /// </summary>
    public static ShoppingCartIntegration ShoppingCart => Integrations.Instance!._shoppingCartIntegration;

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModHelper helper)
    {
        return Integrations.Instance ??= new(helper);
    }
}