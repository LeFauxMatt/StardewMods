namespace StardewMods.FuryCore.Framework.Services;

using System.Globalization;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class Log : ILog
{
    private readonly IConfigWithLogLevel config;
    private readonly IMonitor monitor;

    private string lastMessage = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="Log" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public Log(IConfigWithLogLevel config, IMonitor monitor)
    {
        this.config = config;
        this.monitor = monitor;
    }

    /// <inheritdoc />
    public void Trace(string message, object?[]? args = null) => this.Raise(message, LogLevel.Trace, args);

    /// <inheritdoc />
    public void Debug(string message, object?[]? args = null) => this.Raise(message, LogLevel.Debug, args);

    /// <inheritdoc />
    public void Info(string message, object?[]? args = null) => this.Raise(message, LogLevel.Info, args);

    /// <inheritdoc />
    public void Warn(string message, object?[]? args = null) => this.Raise(message, LogLevel.Warn, args);

    /// <inheritdoc />
    public void Error(string message, object?[]? args = null) => this.Raise(message, LogLevel.Error, args);

    /// <inheritdoc />
    public void Alert(string message, object?[]? args = null) => this.Raise(message, LogLevel.Alert, args);

    private void Raise(string message, LogLevel level, object?[]? args = null)
    {
        switch (level)
        {
            case LogLevel.Trace when this.config.LogLevel == SimpleLogLevel.More:
            case LogLevel.Debug when this.config.LogLevel == SimpleLogLevel.More:
            case LogLevel.Info when this.config.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Warn when this.config.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Error:
            case LogLevel.Alert:
                if (args != null)
                {
                    message = string.Format(CultureInfo.InvariantCulture, message, args);
                }

                // Prevent consecutive duplicate messages
                if (message != this.lastMessage)
                {
                    this.lastMessage = message;
                    this.monitor.Log(message, level);
                }

                break;
            default:
                // Suppress log
                return;
        }

        if (level == LogLevel.Alert)
        {
            Game1.showRedMessage(message);
        }
    }
}