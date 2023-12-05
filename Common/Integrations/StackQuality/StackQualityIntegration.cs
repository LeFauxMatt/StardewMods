namespace StardewMods.Common.Integrations.StackQuality;

/// <inheritdoc />
internal sealed class StackQualityIntegration : ModIntegration<IStackQualityApi>
{
    private const string ModUniqueId = "furyx639.StackQuality";

    /// <summary>Initializes a new instance of the <see cref="StackQualityIntegration" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public StackQualityIntegration(IModRegistry modRegistry)
        : base(modRegistry, StackQualityIntegration.ModUniqueId, "1.0.0-beta.6")
    {
        // Nothing
    }
}
