namespace Common.Integrations.BetterChests
{
    using StardewModdingAPI;

    internal class BetterChestsIntegration : ModIntegration<IBetterChestsAPI>
    {
        /// <summary>Initializes a new instance of the <see cref="BetterChestsIntegration" /> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        public BetterChestsIntegration(IModRegistry modRegistry)
            : base(modRegistry, "furyx639.BetterChests")
        {
        }
    }
}