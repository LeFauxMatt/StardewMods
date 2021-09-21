namespace Common.Services
{
    using StardewModdingAPI;

    /// <summary>Mediator for logging across mod.</summary>
    internal static class Log
    {
        /// <inheritdoc cref="IMonitor"/>
        public static IMonitor Monitor { get; private set; }

        /// <summary>Return and optionally create an instance of the <see cref="Log"/> class.</summary>
        /// <param name="monitor">The instance of Monitor from the Mod.</param>
        public static void Init(IMonitor monitor)
        {
            Log.Monitor = monitor;
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Alert"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Alert(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Alert);
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Debug"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Debug(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Debug);
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Error"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Error(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Error);
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Info"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Info(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Info);
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Trace"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Trace(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Trace);
        }

        /// <summary>Logs a message at an <see cref="LogLevel.Warn"/> level.</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="once">Log message only if it hasn't already been logged since the last game launch.</param>
        public static void Warn(string message, bool once = false)
        {
            Log.LogMessage(message, once, LogLevel.Warn);
        }

        /// <summary>Logs a message that only appears when <see cref="IMonitor.IsVerbose"/> is enabled.</summary>
        /// <param name="message">The message to log.</param>
        public static void Verbose(string message)
        {
            Log.Monitor.VerboseLog(message);
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
}