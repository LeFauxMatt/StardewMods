namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Helper methods to convert between different text formats.</summary>
internal sealed class LocalizedTextManager : BaseService
{
    private readonly ITranslationHelper translations;

    /// <summary>Initializes a new instance of the <see cref="LocalizedTextManager" /> class.</summary>
    /// <param name="translations">Dependency used for accessing translations.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public LocalizedTextManager(ILog log, IManifest manifest, ITranslationHelper translations)
        : base(log, manifest) =>
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
    public string FormatMethod(string value) =>
        (FilterMethodExtensions.TryParse(value, out var method) ? method : FilterMethod.Default) switch
        {
            FilterMethod.Sorted => I18n.Method_Sorted_Name(),
            FilterMethod.GrayedOut => I18n.Method_GrayedOut_Name(),
            FilterMethod.Hidden => I18n.Method_Hidden_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats an option value using localized text when available.</summary>
    /// <param name="value">The option value to format.</param>
    /// <returns>Localized text for the option value.</returns>
    public string FormatOption(string value) =>
        (FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default) switch
        {
            FeatureOption.Disabled => I18n.Option_Disabled_Name(),
            FeatureOption.Enabled => I18n.Option_Enabled_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a group by value using localized text when available.</summary>
    /// <param name="value">The group by value to format.</param>
    /// <returns>Localized text for the group by value.</returns>
    public string FormatGroupBy(string value) =>
        (GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default) switch
        {
            GroupBy.Category => I18n.GroupBy_Category_Name(),
            GroupBy.Color => I18n.GroupBy_Color_Name(),
            GroupBy.Name => I18n.SortBy_Name_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a sort by value using localized text when available.</summary>
    /// <param name="value">The sort by value to format.</param>
    /// <returns>Localized text for the sort by value.</returns>
    public string FormatSortBy(string value) =>
        (SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default) switch
        {
            SortBy.Type => I18n.SortBy_Type_Name(),
            SortBy.Quality => I18n.SortBy_Quality_Name(),
            SortBy.Quantity => I18n.SortBy_Quantity_Name(),
            _ => I18n.Option_Default_Name(),
        };

    /// <summary>Formats a range value using localized text when available.</summary>
    /// <param name="value">The range value to format.</param>
    /// <returns>Localized text for the range value.</returns>
    public string FormatRange(string value) =>
        (RangeOptionExtensions.TryParse(value, out var rangeOption) ? rangeOption : RangeOption.Default) switch
        {
            RangeOption.Disabled => I18n.Option_Disabled_Name(),
            RangeOption.Inventory => I18n.Option_Inventory_Name(),
            RangeOption.Location => I18n.Option_Location_Name(),
            RangeOption.World => I18n.Option_World_Name(),
            _ => I18n.Option_Default_Name(),
        };
}