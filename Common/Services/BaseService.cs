namespace StardewMods.Common.Services;

using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService
{
    /// <summary>Initializes a new instance of the <see cref="BaseService" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    protected BaseService(ILog log, IManifest manifest)
    {
        this.Log = log;
        this.ModId = manifest.UniqueID;
    }

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    protected ILog Log { get; }

    /// <summary>Gets the unique id for this mod.</summary>
    protected string ModId { get; }
}

/// <inheritdoc />
internal abstract class BaseService<TService> : BaseService
    where TService : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseService{TService}" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    protected BaseService(ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.Id = typeof(TService).Name;
        this.UniqueId = this.ModId + "/" + this.Id;
        this.Prefix = this.ModId + "-" + this.Id + "-";
    }

    /// <summary>Gets a unique id for this service.</summary>
    public string Id { get; }

    /// <summary>Gets a globally unique id for this service.</summary>
    public string UniqueId { get; }

    /// <summary>Gets a globally unique prefix for this service.</summary>
    public string Prefix { get; }
}