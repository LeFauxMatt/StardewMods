namespace Common.Integrations.JsonAssets
{
    using StardewModdingAPI;

    /// <inheritdoc />
    internal class JsonAssetsIntegration : ModIntegration<IJsonAssetsAPI>
    {
        /// <summary>Initializes a new instance of the <see cref="JsonAssetsIntegration"/> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        public JsonAssetsIntegration(IModRegistry modRegistry)
            : base(modRegistry, "spacechase0.JsonAssets")
        {
        }
    }
}