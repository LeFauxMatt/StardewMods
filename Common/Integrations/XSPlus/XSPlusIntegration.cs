namespace Common.Integrations.XSPlus;

using StardewModdingAPI;

/// <inheritdoc />
internal class XSPlusIntegration : ModIntegration<IXSPlusApi>
{
    private const string ModUniqueId = "furyx639.XSPlus";

    /// <summary>
    /// Initializes a new instance of the <see cref="XSPlusIntegration"/> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public XSPlusIntegration(IModRegistry modRegistry)
        : base(modRegistry, XSPlusIntegration.ModUniqueId)
    {
    }
}