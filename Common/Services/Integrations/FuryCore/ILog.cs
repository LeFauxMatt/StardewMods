namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Handles logging debug information to the console.</summary>
public interface ILog
{
    /// <summary>Logs a trace message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Trace(string message, object?[]? args = null);

    /// <summary>Logs a debug message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Debug(string message, object?[]? args = null);

    /// <summary>Logs an info message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Info(string message, object?[]? args = null);

    /// <summary>Logs a warn message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Warn(string message, object?[]? args = null);

    /// <summary>Logs an error message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Error(string message, object?[]? args = null);

    /// <summary>Logs an alert message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Alert(string message, object?[]? args = null);
}