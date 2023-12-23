namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IFeature" />
internal abstract class BaseFeature : BaseService, IFeature
{
    private bool isActivated;

    /// <summary>Initializes a new instance of the <see cref="BaseFeature" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    protected BaseFeature(ILog log, ModConfig modConfig)
        : base(log) =>
        this.ModConfig = modConfig;

    /// <summary>Gets the dependency used for accessing config data.</summary>
    protected ModConfig ModConfig { get; }

    /// <inheritdoc />
    public abstract bool ShouldBeActive { get; }

    /// <inheritdoc />
    public void SetActivated(bool warn = false)
    {
        // if (this.ShouldBeActive && IntegrationsManager.TestConflicts(this.Id, out var mods))
        // {
        //     var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
        //     this.Logger.Warn(I18n.Warn_Incompatibility_Disabled(this.Id, modList));
        //     return;
        // }

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

    /// <summary>Activate this feature.</summary>
    protected abstract void Activate();

    /// <summary>Deactivate this feature.</summary>
    protected abstract void Deactivate();
}