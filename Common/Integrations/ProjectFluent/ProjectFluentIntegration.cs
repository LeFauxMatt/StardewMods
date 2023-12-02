namespace StardewMods.Common.Integrations.ProjectFluent;

/// <inheritdoc />
internal sealed class ProjectFluentIntegration : ModIntegration<IProjectFluentApi>
{
    private const string ModUniqueId = "Shockah.ProjectFluent";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProjectFluentIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ProjectFluentIntegration(IModRegistry modRegistry)
        : base(modRegistry, ProjectFluentIntegration.ModUniqueId, "2.0.0-alpha.20230814")
    {
        // Nothing
    }
}