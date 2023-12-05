namespace StardewMods.Common.Integrations.JsonAssets;

/// <inheritdoc />
internal sealed class JsonAssetsIntegration : ModIntegration<IApi>
{
    private const string ModUniqueId = "spacechase0.JsonAssets";

    /// <summary>Initializes a new instance of the <see cref="JsonAssetsIntegration" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public JsonAssetsIntegration(IModRegistry modRegistry)
        : base(modRegistry, JsonAssetsIntegration.ModUniqueId)
    {
        // Nothing
    }
}
