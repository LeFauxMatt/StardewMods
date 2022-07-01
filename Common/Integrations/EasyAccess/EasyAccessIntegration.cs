namespace StardewMods.Common.Integrations.EasyAccess;

using StardewModdingAPI;

/// <inheritdoc />
internal class EasyAccessIntegration : ModIntegration<IEasyAccessApi>
{
    private const string ModUniqueId = "furyx639.EasyAccess";

    /// <summary>
    ///     Initializes a new instance of the <see cref="EasyAccessIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public EasyAccessIntegration(IModRegistry modRegistry)
        : base(modRegistry, EasyAccessIntegration.ModUniqueId)
    {
    }
}