using StardewModdingAPI;

namespace Common.Integrations.EvenBetterRNG
{
    internal class EvenBetterRNGIntegration : ModIntegration<IEvenBetterRNGAPI>
    {
        public EvenBetterRNGIntegration(IModRegistry modRegistry)
            : base(modRegistry, "pepoluan.EvenBetterRNG")
        {
        }
    }
}