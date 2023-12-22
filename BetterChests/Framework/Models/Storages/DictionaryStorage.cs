namespace StardewMods.BetterChests.Framework.Models.Storages;

using System.Globalization;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;

/// <inheritdoc />
internal abstract class DictionaryStorage : IStorage
{
    private const string Prefix = "furyx639.BetterChests/";

    private readonly Dictionary<string, CachedValue<FeatureOption>> cachedFeatureOption = new();
    private readonly Dictionary<string, CachedValue<FeatureOptionRange>> cachedFeatureOptionRange = new();
    private readonly Dictionary<string, CachedValue<HashSet<string>>> cachedHashSet = new();
    private readonly Dictionary<string, CachedValue<int>> cachedInt = new();

    /// <inheritdoc />
    public FeatureOption CarryChestSlow
    {
        get => this.GetFeatureOption(FeatureOptionKey.CarryChestSlow);
        set => this.SetFeatureOption(FeatureOptionKey.CarryChestSlow, value);
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.GetFeatureOption(FeatureOptionKey.AutoOrganize);
        set => this.SetFeatureOption(FeatureOptionKey.AutoOrganize, value);
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.CarryChest);
        set => this.SetFeatureOption(FeatureOptionKey.CarryChest, value);
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.GetFeatureOption(FeatureOptionKey.ChestFinder);
        set => this.SetFeatureOption(FeatureOptionKey.ChestFinder, value);
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.GetFeatureOption(FeatureOptionKey.ChestInfo);
        set => this.SetFeatureOption(FeatureOptionKey.ChestInfo, value);
    }

    /// <inheritdoc />
    public string ChestLabel
    {
        get => this.GetString(StringKey.ChestLabel);
        set => this.SetString(StringKey.ChestLabel, value);
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.GetFeatureOption(FeatureOptionKey.CollectItems);
        set => this.SetFeatureOption(FeatureOptionKey.CollectItems, value);
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.ConfigureChest);
        set => this.SetFeatureOption(FeatureOptionKey.ConfigureChest, value);
    }

    /// <inheritdoc />
    public InGameMenu ConfigureMenu { get; set; }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.GetFeatureOptionRange(FeatureOptionRangeKey.CraftFromChest);
        set => this.SetFeatureOptionRange(FeatureOptionRangeKey.CraftFromChest, value);
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.GetHashSet(HashSetKey.CraftFromChestDisableLocations);
        set => this.SetHashSet(HashSetKey.CraftFromChestDisableLocations, value);
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.GetInt(IntegerKey.CraftFromChestDistance);
        set => this.SetInt(IntegerKey.CraftFromChestDistance, value);
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.GetFeatureOption(FeatureOptionKey.HslColorPicker);
        set => this.SetFeatureOption(FeatureOptionKey.HslColorPicker, value);
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.GetFeatureOption(FeatureOptionKey.FilterItems);
        set => this.SetFeatureOption(FeatureOptionKey.FilterItems, value);
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.GetHashSet(HashSetKey.FilterItemsList);
        set => this.SetHashSet(HashSetKey.FilterItemsList, value);
    }

    /// <inheritdoc />
    public FeatureOption HideUnselectedItems
    {
        get => this.GetFeatureOption(FeatureOptionKey.HideUnselectedItems);
        set => this.SetFeatureOption(FeatureOptionKey.HideUnselectedItems, value);
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.GetFeatureOption(FeatureOptionKey.InventoryTabs);
        set => this.SetFeatureOption(FeatureOptionKey.InventoryTabs, value);
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.GetHashSet(HashSetKey.InventoryTabList);
        set => this.SetHashSet(HashSetKey.InventoryTabList, value);
    }

    /// <inheritdoc />
    public FeatureOption LabelChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.LabelChest);
        set => this.SetFeatureOption(FeatureOptionKey.LabelChest, value);
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.OpenHeldChest);
        set => this.SetFeatureOption(FeatureOptionKey.OpenHeldChest, value);
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.OrganizeChest);
        set => this.SetFeatureOption(FeatureOptionKey.OrganizeChest, value);
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy { get; set; }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy { get; set; }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.ResizeChest);
        set => this.SetFeatureOption(FeatureOptionKey.ResizeChest, value);
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.GetInt(IntegerKey.ResizeChestCapacity);
        set => this.SetInt(IntegerKey.ResizeChestCapacity, value);
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.GetFeatureOption(FeatureOptionKey.SearchItems);
        set => this.SetFeatureOption(FeatureOptionKey.SearchItems, value);
    }

    /// <inheritdoc />
    public FeatureOption SlotLock
    {
        get => this.GetFeatureOption(FeatureOptionKey.SlotLock);
        set => this.SetFeatureOption(FeatureOptionKey.SlotLock, value);
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.GetFeatureOptionRange(FeatureOptionRangeKey.StashToChest);
        set => this.SetFeatureOptionRange(FeatureOptionRangeKey.StashToChest, value);
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.GetHashSet(HashSetKey.StashToChestDisableLocations);
        set => this.SetHashSet(HashSetKey.StashToChestDisableLocations, value);
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.GetInt(IntegerKey.StashToChestDistance);
        set => this.SetInt(IntegerKey.StashToChestDistance, value);
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.GetInt(IntegerKey.StashToChestPriority);
        set => this.SetInt(IntegerKey.StashToChestPriority, value);
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.GetFeatureOption(FeatureOptionKey.StashToChestStacks);
        set => this.SetFeatureOption(FeatureOptionKey.StashToChestStacks, value);
    }

    /// <inheritdoc />
    public FeatureOption TransferItems
    {
        get => this.GetFeatureOption(FeatureOptionKey.TransferItems);
        set => this.SetFeatureOption(FeatureOptionKey.TransferItems, value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.GetFeatureOption(FeatureOptionKey.UnloadChest);
        set => this.SetFeatureOption(FeatureOptionKey.UnloadChest, value);
    }

    /// <inheritdoc />
    public FeatureOption UnloadChestCombine
    {
        get => this.GetFeatureOption(FeatureOptionKey.UnloadChestCombine);
        set => this.SetFeatureOption(FeatureOptionKey.UnloadChestCombine, value);
    }

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

    private FeatureOption GetFeatureOption(FeatureOptionKey featureOptionKey)
    {
        var key = DictionaryStorage.Prefix + featureOptionKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return FeatureOption.Default;
        }

        // Return from cache
        if (this.cachedFeatureOption.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = FeatureOptionExtensions.TryParse(value, out var featureOption) ? featureOption : FeatureOption.Default;

        this.cachedFeatureOption[key] = new CachedValue<FeatureOption>(value, newValue);
        return newValue;
    }

    private FeatureOptionRange GetFeatureOptionRange(FeatureOptionRangeKey featureOptionRangeKey)
    {
        var key = DictionaryStorage.Prefix + featureOptionRangeKey.ToStringFast();
        if (!this.TryGetValue(key, out var value))
        {
            return FeatureOptionRange.Default;
        }

        // Return from cache
        if (this.cachedFeatureOptionRange.TryGetValue(key, out var cachedValue) && cachedValue.OriginalValue == value)
        {
            return cachedValue.Value;
        }

        // Save to cache
        var newValue = FeatureOptionRangeExtensions.TryParse(value, out var featureOptionRange) ? featureOptionRange : FeatureOptionRange.Default;

        this.cachedFeatureOptionRange[key] = new CachedValue<FeatureOptionRange>(value, newValue);
        return newValue;
    }

    private HashSet<string> GetHashSet(HashSetKey hashSetKey)
    {
        var key = DictionaryStorage.Prefix + hashSetKey.ToStringFast();
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

    private int GetInt(IntegerKey integerKey)
    {
        var key = DictionaryStorage.Prefix + integerKey.ToStringFast();
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
        var newValue = string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out var result) ? 0 : result;
        this.cachedInt[key] = new CachedValue<int>(value, newValue);
        return newValue;
    }

    private string GetString(StringKey stringKey)
    {
        var key = DictionaryStorage.Prefix + stringKey.ToStringFast();
        return !this.TryGetValue(key, out var value) ? string.Empty : value;
    }

    private void SetFeatureOption(FeatureOptionKey featureOptionKey, FeatureOption value)
    {
        var key = DictionaryStorage.Prefix + featureOptionKey.ToStringFast();
        var stringValue = value == FeatureOption.Default ? string.Empty : value.ToStringFast();
        this.cachedFeatureOption[key] = new CachedValue<FeatureOption>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void SetFeatureOptionRange(FeatureOptionRangeKey featureOptionRangeKey, FeatureOptionRange value)
    {
        var key = DictionaryStorage.Prefix + featureOptionRangeKey.ToStringFast();
        var stringValue = value == FeatureOptionRange.Default ? string.Empty : value.ToStringFast();
        this.cachedFeatureOptionRange[key] = new CachedValue<FeatureOptionRange>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void SetHashSet(HashSetKey hashSetKey, HashSet<string> value)
    {
        var key = DictionaryStorage.Prefix + hashSetKey.ToStringFast();
        var stringValue = !value.Any() ? string.Empty : string.Join(',', value);
        this.cachedHashSet[key] = new CachedValue<HashSet<string>>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void SetInt(IntegerKey integerKey, int value)
    {
        var key = DictionaryStorage.Prefix + integerKey.ToStringFast();
        var stringValue = value == 0 ? string.Empty : value.ToString(CultureInfo.InvariantCulture);
        this.cachedInt[key] = new CachedValue<int>(stringValue, value);
        this.SetValue(key, stringValue);
    }

    private void SetString(StringKey stringKey, string value)
    {
        var key = DictionaryStorage.Prefix + stringKey.ToStringFast();
        this.SetValue(key, value);
    }

    private readonly struct CachedValue<T>(string originalValue, T cachedValue)
    {
        public T Value { get; } = cachedValue;

        public string OriginalValue { get; } = originalValue;
    }
}
