namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class FauxCoreIntegration : ModIntegration<IFauxCoreApi>
{
    private const string ModUniqueId = "furyx639.FauxCore";

    /// <summary>Initializes a new instance of the <see cref="FauxCoreIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public FauxCoreIntegration(IModRegistry modRegistry)
        : base(modRegistry, FauxCoreIntegration.ModUniqueId)
    {
        // Nothing
    }
}