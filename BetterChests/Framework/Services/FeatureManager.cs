// <copyright file="FeatureManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>A composite service responsible for managing features.</summary>
internal sealed class FeatureManager : BaseService
{
    private readonly IEnumerable<IFeature> features;

    /// <summary>Initializes a new instance of the <see cref="FeatureManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="features">Dependency on all features.</param>
    public FeatureManager(ILog log, IEnumerable<IFeature> features)
        : base(log) =>
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