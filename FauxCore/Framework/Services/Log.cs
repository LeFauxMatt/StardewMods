namespace StardewMods.FauxCore.Framework.Services;

using System.Globalization;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;

/// <inheritdoc />
internal sealed class Log : ILog
{
    private readonly Lazy<IModConfig> modConfig;
    private readonly IMonitor monitor;

    private string lastMessage = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="Log" /> class.</summary>
    /// <param name="getConfig">Dependency used for accessing config data.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public Log(Func<IModConfig> getConfig, IMonitor monitor)
    {
        this.modConfig = new Lazy<IModConfig>(getConfig);
        this.monitor = monitor;
    }

    /// <inheritdoc />
    public void Trace(string message, object?[]? args = null) => this.Raise(message, LogLevel.Trace, false, args);

    /// <inheritdoc />
    public void TraceOnce(string message, params object?[]? args) => this.Raise(message, LogLevel.Trace, true, args);

    /// <inheritdoc />
    public void Debug(string message, object?[]? args = null) => this.Raise(message, LogLevel.Debug, false, args);

    /// <inheritdoc />
    public void Info(string message, object?[]? args = null) => this.Raise(message, LogLevel.Info, false, args);

    /// <inheritdoc />
    public void Warn(string message, object?[]? args = null) => this.Raise(message, LogLevel.Warn, false, args);

    /// <inheritdoc />
    public void WarnOnce(string message, object?[]? args = null) => this.Raise(message, LogLevel.Warn, true, args);

    /// <inheritdoc />
    public void Error(string message, object?[]? args = null) => this.Raise(message, LogLevel.Error, false, args);

    /// <inheritdoc />
    public void Alert(string message, object?[]? args = null) => this.Raise(message, LogLevel.Alert, false, args);

    private void Raise(string message, LogLevel level, bool once, object?[]? args = null)
    {
        if (args != null)
        {
            message = string.Format(CultureInfo.InvariantCulture, message, args);
        }

        // Prevent consecutive duplicate messages
        this.lastMessage = message;
        if (message == this.lastMessage)
        {
            return;
        }

        switch (level)
        {
            case LogLevel.Trace when this.modConfig.Value.LogLevel == SimpleLogLevel.More:
            case LogLevel.Debug when this.modConfig.Value.LogLevel == SimpleLogLevel.More:
            case LogLevel.Info when this.modConfig.Value.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Warn when this.modConfig.Value.LogLevel >= SimpleLogLevel.Less:
            case LogLevel.Error:
            case LogLevel.Alert:
                if (once)
                {
                    this.monitor.LogOnce(message, level);
                    break;
                }

                this.monitor.Log(message, level);
                break;
            default:
                // Suppress log from console
                this.monitor.Log(message);
                return;
        }

        if (level == LogLevel.Alert)
        {
            Game1.showRedMessage(message);
        }
    }
}