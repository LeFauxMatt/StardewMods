namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

// TODO: Get rid of this

/// <summary>A composite service responsible for managing features.</summary>
internal sealed class FeatureManager : BaseService
{
    private readonly IEnumerable<IFeature> features;

    /// <summary>Initializes a new instance of the <see cref="FeatureManager" /> class.</summary>
    /// <param name="features">Dependency on all features.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public FeatureManager(IEnumerable<IFeature> features, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.features = features;

    /// <summary>Activate all features which are set to active.</summary>
    public void Activate()
    {
        foreach (var feature in this.features)
        {
            feature.SetActivated(feature.ShouldBeActive);
        }
    }
}