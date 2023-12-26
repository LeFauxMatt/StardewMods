namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Api for shared functionality between mods.</summary>
public interface IFuryCoreApi
{
    /// <summary>Create an instance of the ILog service for the given IMonitor.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <returns>An instance of ILog that is associated with the provided IMonitor.</returns>
    public ILog CreateLogService(IMonitor monitor);

    /// <summary>Create an instance of the ITheming service.</summary>
    /// <returns>An instance of ITheming.</returns>
    public ITheming CreateThemingService();
}