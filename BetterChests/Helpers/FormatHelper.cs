namespace StardewMods.BetterChests.Helpers;

using System;
using StardewMods.FuryCore.Enums;
using StardewMods.BetterChests.Enums;

/// <summary>
/// Helper methods to convert between different text formats.
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// Gets a string representation of an area value.
    /// </summary>
    /// <param name="area">The area value to get the string representation for.</param>
    /// <returns>The string representation of the area value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid value provided for area.</exception>
    public static string GetAreaString(ComponentArea area)
    {
        return area switch
        {
            ComponentArea.Top => "Top",
            ComponentArea.Right => "Right",
            ComponentArea.Bottom => "Bottom",
            ComponentArea.Left => "Left",
            ComponentArea.Custom => "Custom",
            _ => throw new ArgumentOutOfRangeException(nameof(area), area, null),
        };
    }

    /// <summary>
    /// Gets a string representation of an option value.
    /// </summary>
    /// <param name="option">The option value to get the string representation for.</param>
    /// <returns>The string representation of the option value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid value provided for option.</exception>
    public static string GetOptionString(FeatureOption option)
    {
        return option switch
        {
            FeatureOption.Default => "Default",
            FeatureOption.Disabled => "Disabled",
            FeatureOption.Enabled => "Enabled",
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
        };
    }

    /// <summary>
    /// Gets a string representation of a range value.
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
            FeatureOptionRange.Inventory => "Inventory",
            FeatureOptionRange.Location => "Location",
            FeatureOptionRange.World => "World",
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
        };
    }

    /// <summary>
    /// Formats an area value using localized text when available.
    /// </summary>
    /// <param name="value">The area value to format.</param>
    /// <returns>Localized text for the area value.</returns>
    public static string FormatArea(string value)
    {
        if (!Enum.TryParse(value, out ComponentArea area))
        {
            return value;
        }

        return area switch
        {
            ComponentArea.Top => I18n.Area_Top_Name(),
            ComponentArea.Right => I18n.Area_Right_Name(),
            ComponentArea.Bottom => I18n.Area_Bottom_Name(),
            ComponentArea.Left => I18n.Area_Left_Name(),
            ComponentArea.Custom => I18n.Area_Custom_Name(),
            _ => value,
        };
    }

    /// <summary>
    /// Formats an option value using localized text when available.
    /// </summary>
    /// <param name="value">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public static string FormatOption(string value)
    {
        return Enum.TryParse(value, out FeatureOption option)
            ? FormatHelper.FormatOption(option)
            : value;
    }

    /// <summary>
    /// Formats an option value using localized text when available.
    /// </summary>
    /// <param name="option">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public static string FormatOption(FeatureOption option)
    {
        return option switch
        {
            FeatureOption.Default => I18n.Option_Default_Name(),
            FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            FeatureOption.Enabled => I18n.Option_Enabled_Name(),
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null),
        };
    }

    /// <summary>
    /// Formats a range value using localized text when available.
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
            FeatureOptionRange.Inventory => I18n.Option_Inventory_Name(),
            FeatureOptionRange.Location => I18n.Option_Location_Name(),
            FeatureOptionRange.World => I18n.Option_World_Name(),
            _ => value,
        };
    }

    /// <summary>
    /// Formats chest capacity using localized text when available.
    /// </summary>
    /// <param name="value">The value for capacity to format.</param>
    /// <returns>Localized text for the capacity value.</returns>
    public static string FormatChestCapacity(int value)
    {
        return value switch
        {
            0 => I18n.Option_Disabled_Name(),
            1 => I18n.Option_Default_Name(),
            8 => I18n.Config_ResizeChestCapacity_ValueUnlimited(),
            _ => string.Format(I18n.Config_ResizeChestCapacity_ValueMany(), ((value - 1) * 12).ToString()),
        };
    }

    /// <summary>
    /// Formats chest menu rows using localized text when available.
    /// </summary>
    /// <param name="value">The value for rows to format.</param>
    /// <returns>Localized text for the number of rows.</returns>
    public static string FormatChestMenuRows(int value)
    {
        return value switch
        {
            0 => I18n.Option_Disabled_Name(),
            1 => I18n.Option_Default_Name(),
            2 => I18n.Config_ResizeChestMenuRows_ValueOne(),
            _ => string.Format(I18n.Config_ResizeChestMenuRows_ValueMany(), (value - 1).ToString()),
        };
    }

    /// <summary>
    /// Formats range distance using localized text when available.
    /// </summary>
    /// <param name="value">The value for range distance to format.</param>
    /// <returns>Localized text for the range distance.</returns>
    public static string FormatRangeDistance(int value)
    {
        return value switch
        {
            0 => I18n.Option_Default_Name(),
            1 => I18n.Config_RangeDistance_ValueOne(),
            6 => I18n.Config_RangeDistance_ValueUnlimited(),
            _ => string.Format(I18n.Config_RangeDistance_ValueMany(), value.ToString()),
        };
    }
}