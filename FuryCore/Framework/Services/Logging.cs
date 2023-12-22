namespace StardewMods.FuryCore.Framework.Services;

using System.Globalization;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;

/// <inheritdoc />
internal sealed class Logging : ILogging
{
    private readonly IConfigWithLogLevel config;
    private readonly IMonitor monitor;

    /// <summary>Initializes a new instance of the <see cref="Logging" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public Logging(IConfigWithLogLevel config, IMonitor monitor)
    {
        this.config = config;
        this.monitor = monitor;
    }

    /// <inheritdoc />
    public void Info(string message, params object?[]? args)
    {
        if (this.config.LogLevel != SimpleLogLevel.None)
        {
            this.Log(message, LogLevel.Info, args);
        }
    }

    /// <inheritdoc />
    public void Trace(string message, params object?[]? args)
    {
        if (this.config.LogLevel == SimpleLogLevel.More)
        {
            this.Log(message, LogLevel.Trace, args);
        }
    }

    /// <inheritdoc />
    public void Error(string message, params object?[]? args)
    {
        if (this.config.LogLevel != SimpleLogLevel.None)
        {
            this.Log(message, LogLevel.Error, args);
        }
    }

    /// <inheritdoc />
    public void Warn(string message, params object?[]? args)
    {
        if (this.config.LogLevel != SimpleLogLevel.None)
        {
            this.Log(message, LogLevel.Warn, args);
        }
    }

    private void Log(string message, LogLevel level, object?[]? args)
    {
        if (args != null)
        {
            message = string.Format(CultureInfo.InvariantCulture, message, args);
        }

        this.monitor.Log(message, level);
    }
}
