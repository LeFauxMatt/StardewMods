namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.Common.Interfaces;

/// <summary>This abstract class serves as the base for all service classes.</summary>
internal abstract class BaseService
{
    /// <summary>The mod prefix used for identifying the mod.</summary>
    protected const string ModPrefix = "furyx639.BetterChests";

    /// <summary>Initializes a new instance of the <see cref="BaseService" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    protected BaseService(ILogging logging)
    {
        this.Logging = logging;
        this.Id = this.GetType().Name;
        this.Prefix = BaseService.ModPrefix + "-" + this.Id + "-";
    }

    /// <summary>Gets a unique id for this service.</summary>
    public string Id { get; }

    /// <summary>Gets a unique prefix id for this service.</summary>
    public string Prefix { get; }

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    protected ILogging Logging { get; }
}
