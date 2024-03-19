namespace StardewMods.Common.Services.Integrations.Automate;

/// <inheritdoc />
internal sealed class AutomateIntegration : ModIntegration<IAutomateApi>
{
    private const string ModUniqueId = "Pathoschild.Automate";

    /// <summary>Initializes a new instance of the <see cref="AutomateIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public AutomateIntegration(IModRegistry modRegistry)
        : base(modRegistry, AutomateIntegration.ModUniqueId)
    {
        // Nothing
    }
}