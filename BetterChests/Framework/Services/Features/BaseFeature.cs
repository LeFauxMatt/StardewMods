namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IFeature" />
internal abstract class BaseFeature<TFeature> : BaseService<TFeature>, IFeature
    where TFeature : class, IFeature
{
    private bool isActivated;

    /// <summary>Initializes a new instance of the <see cref="BaseFeature{TFeature}" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="configManager">Dependency used for managing config data.</param>
    protected BaseFeature(ILog log, IManifest manifest, ConfigManager configManager)
        : base(log, manifest)
    {
        this.Config = configManager;
        configManager.ConfigChanged += this.OnConfigChanged;
    }

    /// <inheritdoc />
    public abstract bool ShouldBeActive { get; }

    /// <summary>Gets the dependency used for accessing config data.</summary>
    protected IModConfig Config { get; }

    /// <summary>Activate this feature.</summary>
    protected abstract void Activate();

    /// <summary>Deactivate this feature.</summary>
    protected abstract void Deactivate();

    private void OnConfigChanged(object? sender, ConfigChangedEventArgs e)
    {
        if (this.isActivated == this.ShouldBeActive)
        {
            return;
        }

        this.isActivated = this.ShouldBeActive;
        if (this.isActivated)
        {
            this.Activate();
            return;
        }

        this.Deactivate();
    }
}