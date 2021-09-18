namespace Common.Integrations.DynamicGameAssets
{
    using StardewModdingAPI;

    /// <inheritdoc />
    internal class DynamicGameAssetsIntegration : ModIntegration<IDynamicGameAssetsAPI>
    {
        /// <summary>Initializes a new instance of the <see cref="DynamicGameAssetsIntegration"/> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        public DynamicGameAssetsIntegration(IModRegistry modRegistry)
            : base(modRegistry, "spacechase0.DynamicGameAssets")
        {
        }
    }
}