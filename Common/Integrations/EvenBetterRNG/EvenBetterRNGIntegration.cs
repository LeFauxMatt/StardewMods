namespace Common.Integrations.EvenBetterRNG
{
    using StardewModdingAPI;

    internal class EvenBetterRngIntegration : ModIntegration<IEvenBetterRNGAPI>
    {
        /// <summary>Initializes a new instance of the <see cref="EvenBetterRngIntegration"/> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        public EvenBetterRngIntegration(IModRegistry modRegistry)
            : base(modRegistry, "pepoluan.EvenBetterRNG")
        {
        }
    }
}