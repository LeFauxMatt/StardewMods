namespace StardewMods.BetterChests.Framework;

using StardewMods.BetterChests.Framework.Services;

/// <inheritdoc />
internal abstract class BaseFeature : IFeature
{
    private readonly Func<bool> activeCondition;

    private bool isActivated;

    /// <summary>Initializes a new instance of the <see cref="BaseFeature" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="id">A unique id for this feature.</param>
    /// <param name="activeCondition">The condition to activate this feature.</param>
    protected BaseFeature(IMonitor monitor, string id, Func<bool>? activeCondition = default)
    {
        this.Monitor = monitor;
        this.Id = id;
        this.activeCondition = activeCondition ?? BaseFeature.AlwaysActive;
    }

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    protected IMonitor Monitor { get; }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public void SetActivated(bool warn = false)
    {
        var shouldBeActive = this.activeCondition();
        if (shouldBeActive && IntegrationsManager.TestConflicts(this.Id, out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            this.Monitor.LogOnce(I18n.Warn_Incompatibility_Disabled(this.Id, modList), LogLevel.Warn);
            shouldBeActive = false;
        }

        if (this.isActivated == shouldBeActive)
        {
            return;
        }

        this.isActivated = shouldBeActive;
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

    private static bool AlwaysActive() => true;
}
