namespace StardewMods.ShoppingCart.Helpers;

using StardewMods.Common.Integrations.StackQuality;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
    private static Integrations? Instance;

    private readonly IModHelper _helper;
    private readonly StackQualityIntegration _stackQualityIntegration;

    private Integrations(IModHelper helper)
    {
        this._helper = helper;
        this._stackQualityIntegration = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Stack Quality integration.
    /// </summary>
    public static StackQualityIntegration StackQuality => Integrations.Instance!._stackQualityIntegration;

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