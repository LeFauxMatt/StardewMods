namespace StardewMods.EasyAccess.Helpers;

using System;
using StardewMods.EasyAccess.Enums;

/// <summary>
///     Helper methods to convert between different text formats.
/// </summary>
internal static class FormatHelper
{
    /// <summary>
    ///     Formats a range value using localized text when available.
    /// </summary>
    /// <param name="value">The range value to format.</param>
    /// <returns>Localized text for the range value.</returns>
    public static string FormatRange(string value)
    {
        if (!Enum.TryParse(value, out FeatureOptionRange option))
        {
            return value;
        }

        return option switch
        {
            FeatureOptionRange.Default => I18n.Option_Default_Name(),
            FeatureOptionRange.Disabled => I18n.Option_Disabled_Name(),
            FeatureOptionRange.Location => I18n.Option_Location_Name(),
            FeatureOptionRange.World => I18n.Option_World_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Formats range distance using localized text when available.
    /// </summary>
    /// <param name="value">The value for range distance to format.</param>
    /// <returns>Localized text for the range distance.</returns>
    public static string FormatRangeDistance(int value)
    {
        return value switch
        {
            0 => I18n.Option_Default_Name(),
            1 => I18n.Config_RangeDistance_ValueOne(),
            16 => I18n.Config_RangeDistance_ValueUnlimited(),
            _ => string.Format(I18n.Config_RangeDistance_ValueMany(), value.ToString()),
        };
    }

    /// <summary>
    ///     Gets a string representation of a range value.
    /// </summary>
    /// <param name="range">The range value to get the string representation for.</param>
    /// <returns>The string representation of the range value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid value provided for range.</exception>
    public static string GetRangeString(FeatureOptionRange range)
    {
        return range switch
        {
            FeatureOptionRange.Default => "Default",
            FeatureOptionRange.Disabled => "Disabled",
            FeatureOptionRange.Location => "Location",
            FeatureOptionRange.World => "World",
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
        };
    }
}