namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Helper methods to convert between different text formats.</summary>
internal sealed class LocalizedTextManager : BaseService
{
    private readonly ITranslationHelper translations;

    /// <summary>Initializes a new instance of the <see cref="LocalizedTextManager" /> class.</summary>
    /// <param name="translations">Dependency used for accessing translations.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    public LocalizedTextManager(ILog log, ITranslationHelper translations)
        : base(log) =>
        this.translations = translations;

    public string CarryChestLimit(int value) =>
        value switch
        {
            1 => I18n.Config_CarryChestLimit_ValueOne(),
            > 1 => I18n.Config_CarryChestLimit_ValueMany(value),
            _ => I18n.Config_CarryChestLimit_ValueUnlimited(),
        };

    /// <summary>Formats capacity option using localized text when available.</summary>
    /// <param name="value">The value for capacity to format.</param>
    /// <returns>Localized text for the capacity.</returns>
    public string Capacity(string value) =>
        (CapacityOptionExtensions.TryParse(value, out var capacity) ? capacity : CapacityOption.Default) switch
        {
            CapacityOption.Disabled => I18n.Option_Disabled_Name(),
            CapacityOption.Small => I18n.Capacity_Small_Name(),
            CapacityOption.Medium => I18n.Capacity_Medium_Name(),
            CapacityOption.Large => I18n.Capacity_Large_Name(),
            >= CapacityOption.Unlimited => I18n.Capacity_Unlimited_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats range distance using localized text when available.</summary>
    /// <param name="value">The value for range distance to format.</param>
    /// <returns>Localized text for the range distance.</returns>
    public string Distance(int value) =>
        value switch
        {
            (int)RangeOption.Default => I18n.Option_Default_Name(),
            (int)RangeOption.Disabled => I18n.Option_Disabled_Name(),
            (int)RangeOption.Inventory => I18n.Option_Inventory_Name(),
            (int)RangeOption.World - 1 => I18n.Range_Distance_Unlimited(),
            (int)RangeOption.World => I18n.Option_World_Name(),
            >= (int)RangeOption.Location => I18n.Range_Distance_Many(
                Math.Pow(2, 1 + value - (int)RangeOption.Location).ToString(CultureInfo.InvariantCulture)),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a method value using localized text when available.</summary>
    /// <param name="value">The method value to format.</param>
    /// <returns>Localized text for the method value.</returns>
    public string Method(string value) =>
        (MethodExtensions.TryParse(value, out var method) ? method : Enums.Method.Default) switch
        {
            Enums.Method.Sorted => I18n.Method_Sorted_Name(),
            Enums.Method.GrayedOut => I18n.Method_GrayedOut_Name(),
            Enums.Method.Hidden => I18n.Method_Hidden_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats an option value using localized text when available.</summary>
    /// <param name="value">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public string Option(string value) =>
        (OptionExtensions.TryParse(value, out var option) ? option : Enums.Option.Default) switch
        {
            Enums.Option.Disabled => I18n.Option_Disabled_Name(),
            Enums.Option.Enabled => I18n.Option_Enabled_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a group by value using localized text when available.</summary>
    /// <param name="value">The group by value to format.</param>
    /// <returns>Localized text for the group by value.</returns>
    public string GroupBy(string value) =>
        (GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : Enums.GroupBy.Default) switch
        {
            Enums.GroupBy.Category => I18n.GroupBy_Category_Name(),
            Enums.GroupBy.Color => I18n.GroupBy_Color_Name(),
            Enums.GroupBy.Name => I18n.SortBy_Name_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a sort by value using localized text when available.</summary>
    /// <param name="value">The sort by value to format.</param>
    /// <returns>Localized text for the sort by value.</returns>
    public string SortBy(string value) =>
        (SortByExtensions.TryParse(value, out var sortBy) ? sortBy : Enums.SortBy.Default) switch
        {
            Enums.SortBy.Type => I18n.SortBy_Type_Name(),
            Enums.SortBy.Quality => I18n.SortBy_Quality_Name(),
            Enums.SortBy.Quantity => I18n.SortBy_Quantity_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a range value using localized text when available.</summary>
    /// <param name="value">The range value to format.</param>
    /// <returns>Localized text for the range value.</returns>
    public string Range(string value) =>
        (RangeOptionExtensions.TryParse(value, out var rangeOption) ? rangeOption : RangeOption.Default) switch
        {
            RangeOption.Disabled => I18n.Option_Disabled_Name(),
            RangeOption.Inventory => I18n.Option_Inventory_Name(),
            RangeOption.Location => I18n.Option_Location_Name(),
            RangeOption.World => I18n.Option_World_Name(),
            _ => I18n.Option_Default_Name(),
        };
}