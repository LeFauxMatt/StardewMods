using StardewModdingAPI;

namespace Common.Integrations.DynamicGameAssets
{
    internal class DynamicGameAssetsIntegration : ModIntegration<IDynamicGameAssetsAPI>
    {
        public DynamicGameAssetsIntegration(IModRegistry modRegistry)
            : base(modRegistry, "spacechase0.DynamicGameAssets")
        {
        }
    }
}