namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal class ChildStorageOptions : IStorageOptions
{
    private readonly IStorageOptions child;
    private readonly IStorageOptions parent;

    /// <summary>Initializes a new instance of the <see cref="ChildStorageOptions" /> class.</summary>
    /// <param name="parent">The parent storage options.</param>
    /// <param name="child">The child storage options.</param>
    public ChildStorageOptions(IStorageOptions parent, IStorageOptions child)
    {
        this.parent = parent;
        this.child = child;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.Get(storage => storage.AutoOrganize);
        set => this.child.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.Get(storage => storage.CarryChest);
        set => this.child.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.Get(storage => storage.ChestFinder);
        set => this.child.ChestFinder = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Get(storage => storage.ChestInfo);
        set => this.child.ChestInfo = value;
    }

    /// <inheritdoc />
    public string ChestLabel
    {
        get => this.child.ChestLabel;
        set => this.child.ChestLabel = value;
    }

    /// <inheritdoc />
    public FeatureOption CollectItems
    {
        get => this.Get(storage => storage.CollectItems);
        set => this.child.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.Get(storage => storage.ConfigureChest);
        set => this.child.ConfigureChest = value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.Get(storage => storage.CraftFromChest);
        set => this.child.CraftFromChest = value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.Get(storage => storage.HslColorPicker);
        set => this.child.HslColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption CategorizeChest
    {
        get => this.Get(storage => storage.CategorizeChest);
        set => this.child.CategorizeChest = value;
    }

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags
    {
        get => this.child.CategorizeChestTags.Union(this.parent.CategorizeChestTags).ToHashSet();
        set => this.child.CategorizeChestTags = value;
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.Get(storage => storage.InventoryTabs);
        set => this.child.InventoryTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.child.InventoryTabList.Union(this.parent.InventoryTabList).ToHashSet();
        set => this.child.InventoryTabList = value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.Get(storage => storage.OpenHeldChest);
        set => this.child.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public FeatureOption OrganizeItems
    {
        get => this.Get(storage => storage.OrganizeItems);
        set => this.child.OrganizeItems = value;
    }

    /// <inheritdoc />
    public GroupBy OrganizeItemsGroupBy
    {
        get =>
            this.child.OrganizeItemsGroupBy == GroupBy.Default
                ? this.parent.OrganizeItemsGroupBy
                : this.child.OrganizeItemsGroupBy;
        set => this.child.OrganizeItemsGroupBy = value;
    }

    /// <inheritdoc />
    public SortBy OrganizeItemsSortBy
    {
        get =>
            this.child.OrganizeItemsSortBy == SortBy.Default
                ? this.parent.OrganizeItemsSortBy
                : this.child.OrganizeItemsSortBy;
        set => this.child.OrganizeItemsSortBy = value;
    }

    /// <inheritdoc />
    public CapacityOption ResizeChest
    {
        get => this.Get(storage => storage.ResizeChest);
        set => this.child.ResizeChest = value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.Get(storage => storage.SearchItems);
        set => this.child.SearchItems = value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.Get(storage => storage.StashToChest);
        set => this.child.StashToChest = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get =>
            this.child.StashToChestPriority == 0 ? this.parent.StashToChestPriority : this.child.StashToChestPriority;
        set => this.child.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public FeatureOption TransferItems
    {
        get => this.Get(storage => storage.TransferItems);
        set => this.child.TransferItems = value;
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.Get(storage => storage.UnloadChest);
        set => this.child.UnloadChest = value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.parent.GetDescription();

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.parent.GetDisplayName();

    private CapacityOption Get(Func<IStorageOptions, CapacityOption> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch
        {
            _ when parentValue == CapacityOption.Disabled => CapacityOption.Disabled,
            CapacityOption.Default => parentValue,
            _ => childValue,
        };
    }

    private FeatureOption Get(Func<IStorageOptions, FeatureOption> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch
        {
            _ when parentValue == FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default => parentValue,
            _ => childValue,
        };
    }

    private RangeOption Get(Func<IStorageOptions, RangeOption> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch
        {
            _ when parentValue == RangeOption.Disabled => RangeOption.Disabled,
            RangeOption.Default => parentValue,
            _ => childValue,
        };
    }
}