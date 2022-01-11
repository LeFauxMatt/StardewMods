namespace Common.Integrations.GenericModConfigMenu;

using StardewModdingAPI;

/// <inheritdoc />
internal class GenericModConfigMenuIntegration : ModIntegration<IGenericModConfigMenuApi>
{
    private const string ModUniqueId = "spacechase0.GenericModConfigMenu";

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericModConfigMenuIntegration"/> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public GenericModConfigMenuIntegration(IModRegistry modRegistry)
        : base(modRegistry, GenericModConfigMenuIntegration.ModUniqueId)
    {
    }
}