namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using System.Globalization;
using NetEscapades.EnumGenerators;
using StardewMods.Common.Extensions;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal abstract class DictionaryStorageOptions : IStorageOptions
{
    private const string Prefix = "furyx639.BetterChests/";

    private readonly Dictionary<string, CachedValue<HashSet<string>>> cachedHashSet = new();
    private readonly Dictionary<string, CachedValue<int>> cachedInt = new();

    private readonly Dictionary<string, CachedValue<FeatureOption>> cachedOption = new();
    private readonly Dictionary<string, CachedValue<RangeOption>> cachedRangeOption = new();

    /// <inheritdoc />
    public string ChestLabel
    {
        get => this.Get(StringKey.ChestLabel);
        set => this.Set(StringKey.ChestLabel, value);
    }

    /// <inheritdoc />
    public FeatureOption OrganizeItems
    {
        get => this.Get(OptionKey.OrganizeItems);
        set => this.Set(OptionKey.OrganizeItems, value);
    }

    /// <inheritdoc />
    public GroupBy OrganizeItemsGroupBy { get; set; }

    /// <inheritdoc />
    public SortBy OrganizeItemsSortBy { get; set; }

    /// <inheritdoc />
    public FeatureOption TransferItems
    {
        get => this.Get(OptionKey.TransferItems);
        set => this.Set(OptionKey.TransferItems, value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Get(OptionKey.UnloadChest);
        set => this.Set(OptionKey.UnloadChest, value);
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(OptionKey.AutoOrganize);
        set => this.Set(OptionKey.AutoOrganize, value);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(OptionKey.CarryChest);
        set => this.Set(OptionKey.CarryChest, value);
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(OptionKey.CategorizeChest);
        set => this.Set(OptionKey.CategorizeChest, value);
    }

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags
    {
        get => this.Get(HashSetKey.FilterItemsList);
        set => this.Set(HashSetKey.FilterItemsList, value);
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(OptionKey.ChestFinder);
        set => this.Set(OptionKey.ChestFinder, value);
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Get(OptionKey.ChestInfo);
        set => this.Set(OptionKey.ChestInfo, value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(OptionKey.CollectItems);
        set => this.Set(OptionKey.CollectItems, value);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(OptionKey.ConfigureChest);
        set => this.Set(OptionKey.ConfigureChest, value);
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(RangeOptionKey.CraftFromChest);
        set => this.Set(RangeOptionKey.CraftFromChest, value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.Get(IntegerKey.CraftFromChestDistance);
        set => this.Set(IntegerKey.CraftFromChestDistance, value);
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(OptionKey.HslColorPicker);
        set => this.Set(OptionKey.HslColorPicker, value);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(OptionKey.InventoryTabs);
        set => this.Set(OptionKey.InventoryTabs, value);
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.Get(HashSetKey.InventoryTabList);
        set => this.Set(HashSetKey.InventoryTabList, value);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(OptionKey.OpenHeldChest);
        set => this.Set(OptionKey.OpenHeldChest, value);
    }

    /// <inheritdoc />
    public CapacityOption ResizeChest
    {
        get =>
            CapacityOptionExtensions.TryParse(this.Get(StringKey.ResizeChest), out var capacityOption)
                ? capacityOption
                : CapacityOption.Default;
        set => this.Set(StringKey.ResizeChest, value.ToStringFast());
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(OptionKey.SearchItems);
        set => this.Set(OptionKey.SearchItems, value);
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(RangeOptionKey.StashToChest);
        set => this.Set(RangeOptionKey.StashToChest, value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.Get(IntegerKey.StashToChestDistance);
        set => this.Set(IntegerKey.StashToChestDistance, value);
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.Get(IntegerKey.StashToChestPriority);
        set => this.Set(IntegerKey.StashToChestPriority, value);
    }

    /// <inheritdoc />
    public string GetDescription() => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public string GetDisplayName() => I18n.Storage_Other_Name();

    /// <summary>Tries to get the data associated with the specified key.</summary>
    /// <param name="key">The key to search for.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key, if the key is
    /// found; otherwise, null.
    /// </param>
    /// <returns>true if the key was found and the data associated with it was retrieved successfully; otherwise, false.</returns>
    protected abstract bool TryGetValue(string key, [NotNullWhen(true)] out string? value);

    /// <summary>Sets the value for a given key in the derived class.</summary>
    /// <param name="key">The key associated with the value.</param>
    /// <param name="value">The value to be set.</param>
    protected abstract void SetValue(string key, string value);

    private FeatureOption Get(OptionKey optionKey)
    {
        var key = DictionaryStorageOptions.Prefix + optionKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return FeatureOption.Default;
        }

        // Return from cache
        if (this.cachedOption.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default;
        this.cachedOption[key] = new CachedValue<FeatureOption>(value, newValue);
        return newValue;
    }

    private RangeOption Get(RangeOptionKey rangeOptionKey)
    {
        var key = DictionaryStorageOptions.Prefix + rangeOptionKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return RangeOption.Default;
        }

        // Return from cache
        if (this.cachedRangeOption.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = RangeOptionExtensions.TryParse(value, out var rangeOption) ? rangeOption : RangeOption.Default;
        this.cachedRangeOption[key] = new CachedValue<RangeOption>(value, newValue);
        return newValue;
    }

    private HashSet<string> Get(HashSetKey hashSetKey)
    {
        var key = DictionaryStorageOptions.Prefix + hashSetKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return [];
        }

        // Return from cache
        if (this.cachedHashSet.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = string.IsNullOrWhiteSpace(value) ? new HashSet<string>() : [..value.Split(',')];
        this.cachedHashSet[key] = new CachedValue<HashSet<string>>(value, newValue);
        return newValue;
    }

    private int Get(IntegerKey integerKey)
    {
        var key = DictionaryStorageOptions.Prefix + integerKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return 0;
        }

        // Return from cache
        if (this.cachedInt.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = value.GetInt();
        this.cachedInt[key] = new CachedValue<int>(value, newValue);
        return newValue;
    }

    private string Get(StringKey stringKey)
    {
        var key = DictionaryStorageOptions.Prefix + stringKey.ToStringFast();
        return !this.TryGetValue(key, out var value) ? string.Empty : value;
    }

    private void Set(OptionKey optionKey, FeatureOption value)
    {
        var key = DictionaryStorageOptions.Prefix + optionKey.ToStringFast();
        var stringValue = value == FeatureOption.Default ? string.Empty : value.ToStringFast();
        this.cachedOption[key] = new CachedValue<FeatureOption>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(RangeOptionKey rangeOptionKey, RangeOption value)
    {
        var key = DictionaryStorageOptions.Prefix + rangeOptionKey.ToStringFast();
        var stringValue = value == RangeOption.Default ? string.Empty : value.ToStringFast();
        this.cachedRangeOption[key] = new CachedValue<RangeOption>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(HashSetKey hashSetKey, HashSet<string> value)
    {
        var key = DictionaryStorageOptions.Prefix + hashSetKey.ToStringFast();
        var stringValue = !value.Any() ? string.Empty : string.Join(',', value);
        this.cachedHashSet[key] = new CachedValue<HashSet<string>>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(IntegerKey integerKey, int value)
    {
        var key = DictionaryStorageOptions.Prefix + integerKey.ToStringFast();
        var stringValue = value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);
        this.cachedInt[key] = new CachedValue<int>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void Set(StringKey stringKey, string value)
    {
        var key = DictionaryStorageOptions.Prefix + stringKey.ToStringFast();
        this.SetValue(key, value);
    }

    private readonly struct CachedValue<T>(string originalValue, T cachedValue)
    {
        public T Value { get; } = cachedValue;

        public string OriginalValue { get; } = originalValue;
    }
#pragma warning disable SA1201
#pragma warning disable SA1600
#pragma warning disable SA1602
    [EnumExtensions]
    internal enum HashSetKey
    {
        FilterItemsList,
        InventoryTabList,
    }

    [EnumExtensions]
    internal enum IntegerKey
    {
        CraftFromChestDistance,
        StashToChestDistance,
        StashToChestPriority,
    }

    [EnumExtensions]
    internal enum OptionKey
    {
        AutoOrganize,
        CarryChest,
        CategorizeChest,
        ChestFinder,
        ChestInfo,
        CollectItems,
        ConfigureChest,
        HslColorPicker,
        InventoryTabs,
        OpenHeldChest,
        OrganizeItems,
        SearchItems,
        TransferItems,
        UnloadChest,
    }

    [EnumExtensions]
    internal enum RangeOptionKey
    {
        CraftFromChest,
        StashToChest,
    }

    [EnumExtensions]
    internal enum StringKey
    {
        ChestLabel,
        ResizeChest,
    }
}