namespace StardewMods.FuryCore.Framework.Interfaces;

/// <summary>Handles logging debug information to the console.</summary>
public interface ILogger
{
    /// <summary>Logs a message to the console when any logging is enabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Info(string message, params object?[]? args);

    /// <summary>Logs a message to the console when more logging is enabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Trace(string message, params object?[]? args);

    /// <summary>Logs a message to the console unless it is disabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Error(string message, params object?[]? args);

    /// <summary>Logs a message to the console unless it is disabled.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    public void Warn(string message, params object?[]? args);
}
