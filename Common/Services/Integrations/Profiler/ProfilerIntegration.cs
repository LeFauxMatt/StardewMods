namespace StardewMods.Common.Services.Integrations.Profiler;

/// <inheritdoc />
internal sealed class ProfilerIntegration : ModIntegration<IProfilerApi>
{
    private const string ModUniqueId = "SinZ.Profiler";

    /// <summary>Initializes a new instance of the <see cref="ProfilerIntegration" /> class.</summary>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ProfilerIntegration(IModRegistry modRegistry)
        : base(modRegistry, ProfilerIntegration.ModUniqueId, "2.0.0")
    {
        // Nothing
    }
}