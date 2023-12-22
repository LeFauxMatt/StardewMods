namespace StardewMods.FuryCore.Framework.Services.Integrations.FuryCore;

/// <inheritdoc />
public sealed class FuryCoreIntegration : BaseIntegration<IFuryCoreApi>
{
    private const string ModUniqueId = "furyx639.FuryCore";

    /// <summary>Initializes a new instance of the <see cref="FuryCoreIntegration" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public FuryCoreIntegration(IModRegistry modRegistry)
        : base(modRegistry, FuryCoreIntegration.ModUniqueId)
    {
        // Nothing
    }
}
