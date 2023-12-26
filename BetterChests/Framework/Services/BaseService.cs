namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService
{
    /// <summary>The mod prefix used for identifying the mod.</summary>
    protected const string ModId = "furyx639.BetterChests";

    /// <summary>Initializes a new instance of the <see cref="BaseService" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    protected BaseService(ILog log) => this.Log = log;

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    protected ILog Log { get; }
}

/// <inheritdoc />
internal abstract class BaseService<TFeature> : BaseService
    where TFeature : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseService{TFeature}" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    protected BaseService(ILog log)
        : base(log)
    {
        this.Id = typeof(TFeature).Name;
        this.UniqueId = BaseService.ModId + "/" + this.Id;
        this.Prefix = BaseService.ModId + "-" + this.Id + "-";
    }

    /// <summary>Gets a unique id for this service.</summary>
    public string Id { get; }

    /// <summary>Gets a globally unique id for this service.</summary>
    public string UniqueId { get; }

    /// <summary>Gets a globally unique prefix for this service.</summary>
    public string Prefix { get; }
}