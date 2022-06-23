namespace StardewMods.BetterChests.Helpers;

using System;
using Common.Enums;

/// <summary>
///     Helper methods to convert between different text formats.
/// </summary>
internal static class FormatHelper
{
    /// <summary>
    ///     Formats an area value using localized text when available.
    /// </summary>
    /// <param name="value">The area value to format.</param>
    /// <returns>Localized text for the area value.</returns>
    public static string FormatArea(string value)
    {
        return value switch
        {
            nameof(ComponentArea.Top) => I18n.Area_Top_Name(),
            nameof(ComponentArea.Right) => I18n.Area_Right_Name(),
            nameof(ComponentArea.Bottom) => I18n.Area_Bottom_Name(),
            nameof(ComponentArea.Left) => I18n.Area_Left_Name(),
            nameof(ComponentArea.Custom) => I18n.Area_Custom_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Formats carry chest limit using localized text when available.
    /// </summary>
    /// <param name="value">The value for carry chest limit to format.</param>
    /// <returns>Localized text for the carry chest limit value.</returns>
    public static string FormatCarryChestLimit(int value)
    {
        return value switch
        {
            (int)FeatureOption.Default => I18n.Option_Default_Name(),
            (int)FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            (int)FeatureOption.Enabled => I18n.Config_CarryChestLimit_ValueOne(),
            _ => string.Format(I18n.Config_CarryChestLimit_ValueMany(), (1 + value - (int)FeatureOption.Enabled).ToString()),
        };
    }

    /// <summary>
    ///     Formats carry chest slow using localized text when available.
    /// </summary>
    /// <param name="value">The value for carry chest slow to format.</param>
    /// <returns>Localized text for the carry chest slow value.</returns>
    public static string FormatCarryChestSlow(int value)
    {
        return value switch
        {
            0 => I18n.Config_CarryChestSlow_ValueZero(),
            _ => string.Format(I18n.Config_CarryChestSlow_Value(), value.ToString()),
        };
    }

    /// <summary>
    ///     Formats chest capacity using localized text when available.
    /// </summary>
    /// <param name="value">The value for capacity to format.</param>
    /// <returns>Localized text for the capacity value.</returns>
    public static string FormatChestCapacity(int value)
    {
        return value switch
        {
            (int)FeatureOption.Default => I18n.Option_Default_Name(),
            (int)FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            8 => I18n.Config_ResizeChestCapacity_ValueUnlimited(),
            _ => string.Format(I18n.Config_ResizeChestCapacity_ValueMany(), (12 * (value - (int)FeatureOption.Enabled + 1)).ToString()),
        };
    }

    /// <summary>
    ///     Formats chest menu rows using localized text when available.
    /// </summary>
    /// <param name="value">The value for rows to format.</param>
    /// <returns>Localized text for the number of rows.</returns>
    public static string FormatChestMenuRows(int value)
    {
        return value switch
        {
            (int)FeatureOption.Default => I18n.Option_Default_Name(),
            (int)FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            (int)FeatureOption.Enabled => I18n.Config_ResizeChestMenuRows_ValueOne(),
            _ => string.Format(I18n.Config_ResizeChestMenuRows_ValueMany(), (value - 1).ToString()),
        };
    }

    /// <summary>
    ///     Formats a group by value using localized text when available.
    /// </summary>
    /// <param name="value">The group by value to format.</param>
    /// <returns>Localized text for the group by value.</returns>
    public static string FormatGroupBy(string value)
    {
        return value switch
        {
            nameof(GroupBy.Default) => I18n.Option_Default_Name(),
            nameof(GroupBy.Category) => I18n.GroupBy_Category_Name(),
            nameof(GroupBy.Color) => I18n.GroupBy_Color_Name(),
            nameof(GroupBy.Name) => I18n.SortBy_Name_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Formats an option value using localized text when available.
    /// </summary>
    /// <param name="value">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public static string FormatOption(string value)
    {
        return value switch
        {
            nameof(FeatureOption.Default) => I18n.Option_Default_Name(),
            nameof(FeatureOption.Disabled) => I18n.Option_Disabled_Name(),
            nameof(FeatureOption.Enabled) => I18n.Option_Enabled_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Formats a range value using localized text when available.
    /// </summary>
    /// <param name="value">The range value to format.</param>
    /// <returns>Localized text for the range value.</returns>
    public static string FormatRange(string value)
    {
        return value switch
        {
            nameof(FeatureOptionRange.Default) => I18n.Option_Default_Name(),
            nameof(FeatureOptionRange.Disabled) => I18n.Option_Disabled_Name(),
            nameof(FeatureOptionRange.Inventory) => I18n.Option_Inventory_Name(),
            nameof(FeatureOptionRange.Location) => I18n.Option_Location_Name(),
            nameof(FeatureOptionRange.World) => I18n.Option_World_Name(),
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
            (int)FeatureOptionRange.Default => I18n.Option_Default_Name(),
            (int)FeatureOptionRange.Disabled => I18n.Option_Disabled_Name(),
            (int)FeatureOptionRange.Inventory => I18n.Option_Inventory_Name(),
            (int)FeatureOptionRange.World - 1 => I18n.Config_RangeDistance_ValueUnlimited(),
            (int)FeatureOptionRange.World => I18n.Option_World_Name(),
            >= (int)FeatureOptionRange.Location => string.Format(I18n.Config_RangeDistance_ValueMany(), (2 ^ (value - (int)FeatureOptionRange.Location + 1)).ToString()),
            _ => I18n.Option_Default_Name(),
        };
    }

    /// <summary>
    ///     Formats a sort by value using localized text when available.
    /// </summary>
    /// <param name="value">The sort by value to format.</param>
    /// <returns>Localized text for the sort by value.</returns>
    public static string FormatSortBy(string value)
    {
        return value switch
        {
            nameof(SortBy.Default) => I18n.Option_Default_Name(),
            nameof(SortBy.Type) => I18n.SortBy_Type_Name(),
            nameof(SortBy.Quality) => I18n.SortBy_Quality_Name(),
            nameof(SortBy.Quantity) => I18n.SortBy_Quantity_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Gets a string representation of an area value.
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
    ///     Gets a string representation of a group by value.
    /// </summary>
    /// <param name="groupBy">The group by value to get the string representation for.</param>
    /// <returns>The string representation of the group by value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid value provided for group by.</exception>
    public static string GetGroupByString(GroupBy groupBy)
    {
        return groupBy switch
        {
            GroupBy.Default => "Default",
            GroupBy.Category => "Category",
            GroupBy.Color => "Color",
            GroupBy.Name => "Name",
            _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null),
        };
    }

    /// <summary>
    ///     Gets a string representation of an option value.
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
            FeatureOptionRange.Inventory => "Inventory",
            FeatureOptionRange.Location => "Location",
            FeatureOptionRange.World => "World",
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
        };
    }

    /// <summary>
    ///     Gets a string representation of a sort by value.
    /// </summary>
    /// <param name="sortBy">The sort by value to get the string representation for.</param>
    /// <returns>The string representation of the sort by value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">An invalid value provided for sort by.</exception>
    public static string GetSortByString(SortBy sortBy)
    {
        return sortBy switch
        {
            SortBy.Default => "Default",
            SortBy.Type => "Type",
            SortBy.Quality => "Quality",
            SortBy.Quantity => "Quantity",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null),
        };
    }
}