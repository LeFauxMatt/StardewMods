namespace StardewMods.ShoppingCart.Framework;

using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.StackQuality;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
#nullable disable
    private static Integrations Instance;
#nullable enable

    private readonly GenericModConfigMenuIntegration _genericModConfigMenu;
    private readonly StackQualityIntegration _stackQualityIntegration;

    private Integrations(IModHelper helper)
    {
        this._genericModConfigMenu = new(helper.ModRegistry);
        this._stackQualityIntegration = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Generic Mod Config Menu integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GMCM => Integrations.Instance._genericModConfigMenu;

    /// <summary>
    ///     Gets Stack Quality integration.
    /// </summary>
    public static StackQualityIntegration StackQuality => Integrations.Instance._stackQualityIntegration;

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