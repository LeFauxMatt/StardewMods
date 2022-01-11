namespace Common.Integrations.DynamicGameAssets;

using StardewModdingAPI;

/// <inheritdoc />
internal class DynamicGameAssetsIntegration : ModIntegration<IDynamicGameAssetsApi>
{
    private const string ModUniqueId = "spacechase0.DynamicGameAssets";

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicGameAssetsIntegration"/> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public DynamicGameAssetsIntegration(IModRegistry modRegistry)
        : base(modRegistry, DynamicGameAssetsIntegration.ModUniqueId)
    {
    }
}