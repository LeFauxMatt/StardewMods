namespace StardewMods.FuryCore.Framework.Services.Integrations.FuryCore;

using StardewMods.FuryCore.Framework.Interfaces;

/// <summary>Api for shared functionality between mods.</summary>
public interface IFuryCoreApi
{

    /// <summary>Retrieves an instance of an ILogger for the given IMonitor.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <returns>An instance of ILogger that is associated with the provided IMonitor.</returns>
    public ILogging GetLogger(IMonitor monitor);

    /// <summary>Retrieves the theme helper object.</summary>
    /// <returns>The theme helper object.</returns>
    public IThemeHelper GetThemeHelper();
}