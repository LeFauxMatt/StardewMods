namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />

internal sealed class LogService(FuryCoreIntegration furyCoreIntegration, IMonitor monitor) : ILog
{
    private readonly ILog log = furyCoreIntegration.Api!.CreateLogService(monitor);

    /// <inheritdoc/>
    public void Trace(string message, object?[]? args = null) => this.log.Trace(message, args);

    /// <inheritdoc/>
    public void Debug(string message, object?[]? args = null) => this.log.Debug(message, args);

    /// <inheritdoc/>
    public void Info(string message, object?[]? args = null) => this.log.Info(message, args);

    /// <inheritdoc/>
    public void Warn(string message, object?[]? args = null) => this.log.Warn(message, args);

    /// <inheritdoc/>
    public void WarnOnce(string message, object?[]? args = null) => this.log.WarnOnce(message, args);

    /// <inheritdoc/>
    public void Error(string message, object?[]? args = null) => this.log.Error(message, args);

    /// <inheritdoc/>
    public void Alert(string message, object?[]? args = null) => this.log.Alert(message, args);
}