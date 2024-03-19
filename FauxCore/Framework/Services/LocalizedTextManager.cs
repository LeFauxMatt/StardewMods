namespace StardewMods.FauxCore.Framework.Services;

using StardewMods.Common.Enums;

/// <summary>Formats the given <see cref="LogLevel" /> into a string.</summary>
internal static class LocalizedTextManager
{
    /// <summary>Formats the given <paramref name="value" /> into a string.</summary>
    /// <param name="value">The level of logging.</param>
    /// <returns>Returns the formatted log level.</returns>
    public static string TryFormat(string value)
    {
        if (!SimpleLogLevelExtensions.TryParse(value, out var logLevel))
        {
            logLevel = SimpleLogLevel.Less;
        }

        return logLevel switch
        {
            SimpleLogLevel.None => I18n.Config_LogLevel_Options_None(),
            SimpleLogLevel.Less => I18n.Config_LogLevel_Options_Less(),
            SimpleLogLevel.More => I18n.Config_LogLevel_Options_More(),
            _ => I18n.Config_LogLevel_Options_Less(),
        };
    }
}