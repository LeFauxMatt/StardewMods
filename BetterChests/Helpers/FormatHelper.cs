namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using StardewMods.Common.Enums;
using StardewValley;

/// <summary>
///     Helper methods to convert between different text formats.
/// </summary>
internal static class FormatHelper
{
    private static Dictionary<string, string>? _blueprintsData;

    private static Dictionary<string, string> BlueprintsData => FormatHelper._blueprintsData ??=
        Game1.content.Load<Dictionary<string, string>>("Data\\Blueprints");

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
            _ => string.Format(
                I18n.Config_ResizeChestCapacity_ValueMany(),
                (12 * (value - (int)FeatureOption.Enabled + 1)).ToString()),
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
            _ => string.Format(
                I18n.Config_ResizeChestMenuRows_ValueMany(),
                (value - (int)FeatureOption.Enabled + 3).ToString()),
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
            >= (int)FeatureOptionRange.Location => string.Format(
                I18n.Config_RangeDistance_ValueMany(),
                Math.Pow(2, 1 + value - (int)FeatureOptionRange.Location).ToString(CultureInfo.InvariantCulture)),
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
    ///     Formats a storage name using localized text when available.
    /// </summary>
    /// <param name="value">The storage to format.</param>
    /// <returns>Localized text for the storage name.</returns>
    public static string FormatStorageName(string value)
    {
        return value switch
        {
            "Chest" when Game1.bigCraftablesInformation.TryGetValue(130, out var info) => info.Split('/')[8],
            "Mini-Fridge" when Game1.bigCraftablesInformation.TryGetValue(215, out var info) => info.Split('/')[8],
            "Stone Chest" when Game1.bigCraftablesInformation.TryGetValue(232, out var info) => info.Split('/')[8],
            "Mini-Shipping Bin" when Game1.bigCraftablesInformation.TryGetValue(248, out var info) => info.Split('/')
                [8],
            "Junimo Chest" when Game1.bigCraftablesInformation.TryGetValue(256, out var info) => info.Split('/')[8],
            "Junimo Hut" when FormatHelper.BlueprintsData.TryGetValue("Junimo Hut", out var info) => info.Split('/')[8],
            "Shipping Bin" when FormatHelper.BlueprintsData.TryGetValue("Shipping Bin", out var info) => info.Split('/')
                [8],
            "Fridge" => I18n.Storage_Fridge_Name(),
            _ => value,
        };
    }

    /// <summary>
    ///     Formats a storage tooltip using localized text when available.
    /// </summary>
    /// <param name="value">The storage to format.</param>
    /// <returns>Localized text for the storage tooltip.</returns>
    public static string FormatStorageTooltip(string value)
    {
        return value switch
        {
            "Chest" when Game1.bigCraftablesInformation.TryGetValue(130, out var info) => info.Split('/')[4],
            "Mini-Fridge" when Game1.bigCraftablesInformation.TryGetValue(215, out var info) => info.Split('/')[4],
            "Stone Chest" when Game1.bigCraftablesInformation.TryGetValue(232, out var info) => info.Split('/')[4],
            "Mini-Shipping Bin" when Game1.bigCraftablesInformation.TryGetValue(248, out var info) => info.Split('/')
                [4],
            "Junimo Chest" when Game1.bigCraftablesInformation.TryGetValue(256, out var info) => info.Split('/')[4],
            "Junimo Hut" when FormatHelper.BlueprintsData.TryGetValue("Junimo Hut", out var info) => info.Split('/')[9],
            "Shipping Bin" when FormatHelper.BlueprintsData.TryGetValue("Shipping Bin", out var info) => info.Split('/')
                [9],
            "Fridge" => I18n.Storage_Fridge_Tooltip(),
            _ => value,
        };
    }
}