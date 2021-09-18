namespace Common.Integrations.GenericModConfigMenu
{
    using StardewModdingAPI;

    /// <inheritdoc />
    internal class GenericModConfigMenuIntegration : ModIntegration<IGenericModConfigMenuAPI>
    {
        /// <summary>Initializes a new instance of the <see cref="GenericModConfigMenuIntegration"/> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        public GenericModConfigMenuIntegration(IModRegistry modRegistry)
            : base(modRegistry, "spacechase0.GenericModConfigMenu")
        {
        }
    }
}