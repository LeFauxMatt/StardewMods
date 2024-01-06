namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class FuryCoreIntegration : ModIntegration<IFuryCoreApi>
{
    private const string ModUniqueId = "furyx639.FuryCore";

    /// <summary>Initializes a new instance of the <see cref="FuryCoreIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public FuryCoreIntegration(IModRegistry modRegistry)
        : base(modRegistry, FuryCoreIntegration.ModUniqueId)
    {
        // Nothing
    }
}