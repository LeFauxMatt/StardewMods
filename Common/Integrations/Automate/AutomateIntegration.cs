namespace StardewMods.Common.Integrations.Automate;

/// <inheritdoc />
internal sealed class AutomateIntegration : ModIntegration<IAutomateApi>
{
    private const string ModUniqueId = "Pathoschild.Automate";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutomateIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public AutomateIntegration(IModRegistry modRegistry)
        : base(modRegistry, AutomateIntegration.ModUniqueId)
    {
        // Nothing
    }
}