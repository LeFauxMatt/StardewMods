namespace StardewMods.CustomBush.Framework.Services;

using StardewMods.Common.Enums;

/// <summary>Formats the given <see cref="LogLevels" /> into a string.</summary>
internal static class Formatting
{
    /// <summary>Formats the given <paramref name="value" /> into a string.</summary>
    /// <param name="value">The level of logging.</param>
    /// <returns>Returns the formatted log level.</returns>
    public static string TryFormat(string value)
    {
        if (!LogLevelsExtensions.TryParse(value, out var logLevel))
        {
            logLevel = LogLevels.Less;
        }

        return logLevel switch
        {
            LogLevels.None => I18n.Config_LogLevel_Options_None(),
            LogLevels.Less => I18n.Config_LogLevel_Options_Less(),
            LogLevels.More => I18n.Config_LogLevel_Options_More(),
            _ => I18n.Config_LogLevel_Options_Less(),
        };
    }
}
