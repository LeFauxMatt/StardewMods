#nullable disable

namespace Common.Helpers;

using StardewModdingAPI;

/// <summary>
///     Provides logging across mods.
/// </summary>
internal static class Log
{
    /// <inheritdoc cref="IMonitor" />
    public static IMonitor Monitor { get; set; }

    /// <summary>Logs a message at an <see cref="LogLevel.Alert" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Alert(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Alert);
    }

    /// <summary>Logs a message at an <see cref="LogLevel.Debug" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Debug(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Debug);
    }

    /// <summary>Logs a message at an <see cref="LogLevel.Error" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Error(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Error);
    }

    /// <summary>Logs a message at an <see cref="LogLevel.Info" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Info(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Info);
    }

    /// <summary>Logs a message at an <see cref="LogLevel.Trace" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Trace(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Trace);
    }

    /// <summary>Logs a message that only appears when <see cref="IMonitor.IsVerbose" /> is enabled.</summary>
    /// <param name="message">The message to log.</param>
    public static void Verbose(string message)
    {
        if (Log.Monitor.IsVerbose)
        {
            Log.Monitor.VerboseLog(message);
        }
    }

    /// <summary>Logs a message that only appears when <see cref="IMonitor.IsVerbose" /> is enabled.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="args">The arguments to pass.</param>
    public static void Verbose(string message, params object[] args)
    {
        if (Log.Monitor.IsVerbose)
        {
            Log.Monitor.VerboseLog(string.Format(message, args));
        }
    }

    /// <summary>Logs a message at an <see cref="LogLevel.Warn" /> level.</summary>
    /// <param name="message">The message to log.</param>
    /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
    public static void Warn(string message, bool once = false)
    {
        Log.LogMessage(message, once, LogLevel.Warn);
    }

    private static void LogMessage(string message, bool once, LogLevel logLevel)
    {
        if (once)
        {
            Log.Monitor.LogOnce(message, logLevel);
        }
        else
        {
            Log.Monitor.Log(message, logLevel);
        }
    }
}