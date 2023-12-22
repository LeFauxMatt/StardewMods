namespace StardewMods.Common.Services;

using System.Globalization;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;

/// <summary>Handles logging debug information to the console.</summary>
internal sealed class Logging
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

    /// <summary>Logs a message to the console when any logging is enabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Info(string message, params object?[]? args)
    {
        if (this.config.LogLevel != SimpleLogLevel.None)
        {
            this.Log(message, LogLevel.Info, args);
        }
    }

    /// <summary>Logs a message to the console when more logging is enabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Trace(string message, params object?[]? args)
    {
        if (this.config.LogLevel == SimpleLogLevel.More)
        {
            this.Log(message, LogLevel.Trace, args);
        }
    }

    /// <summary>Logs a message to the console unless it is disabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Error(string message, params object?[]? args)
    {
        if (this.config.LogLevel != SimpleLogLevel.None)
        {
            this.Log(message, LogLevel.Error, args);
        }
    }

    /// <summary>Logs a message to the console unless it is disabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
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
