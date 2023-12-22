namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;

/// <inheritdoc />
internal class ChildStorage : IStorage
{
    private readonly IStorage child;
    private readonly IStorage parent;

    /// <summary>Initializes a new instance of the <see cref="ChildStorage" /> class.</summary>
    /// <param name="parent">The parent storage options.</param>
    /// <param name="child">The child storage options.</param>
    public ChildStorage(IStorage parent, IStorage child)
    {
        this.parent = parent;
        this.child = child;
    }

    /// <inheritdoc />
    public FeatureOption AutoOrganize
    {
        get => this.GetFeatureOption(storage => storage.AutoOrganize);
        set => this.child.AutoOrganize = value;
    }

    /// <inheritdoc />
    public FeatureOption CarryChest
    {
        get => this.GetFeatureOption(storage => storage.CarryChest);
        set => this.child.CarryChest = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestFinder
    {
        get => this.GetFeatureOption(storage => storage.ChestFinder);
        set => this.child.ChestFinder = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.GetFeatureOption(storage => storage.ChestInfo);
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
        get => this.GetFeatureOption(storage => storage.CollectItems);
        set => this.child.CollectItems = value;
    }

    /// <inheritdoc />
    public FeatureOption ConfigureChest
    {
        get => this.GetFeatureOption(storage => storage.ConfigureChest);
        set => this.child.ConfigureChest = value;
    }

    /// <inheritdoc />
    public InGameMenu ConfigureMenu
    {
        get => this.child.ConfigureMenu == InGameMenu.Default ? this.parent.ConfigureMenu : this.child.ConfigureMenu;
        set => this.child.ConfigureMenu = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest
    {
        get => this.GetFeatureOptionRange(storage => storage.CraftFromChest);
        set => this.child.CraftFromChest = value;
    }

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations
    {
        get => this.child.CraftFromChestDisableLocations.Union(this.parent.CraftFromChestDisableLocations).ToHashSet();
        set => this.child.CraftFromChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int CraftFromChestDistance
    {
        get => this.child.CraftFromChestDistance == 0 ? this.parent.CraftFromChestDistance : this.child.CraftFromChestDistance;
        set => this.child.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public FeatureOption HslColorPicker
    {
        get => this.GetFeatureOption(storage => storage.HslColorPicker);
        set => this.child.HslColorPicker = value;
    }

    /// <inheritdoc />
    public FeatureOption FilterItems
    {
        get => this.GetFeatureOption(storage => storage.FilterItems);
        set => this.child.FilterItems = value;
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.child.FilterItemsList.Union(this.parent.FilterItemsList).ToHashSet();
        set => this.child.FilterItemsList = value;
    }

    /// <inheritdoc />
    public FeatureOption HideUnselectedItems
    {
        get => this.GetFeatureOption(storage => storage.HideUnselectedItems);
        set => this.child.HideUnselectedItems = value;
    }

    /// <inheritdoc />
    public FeatureOption InventoryTabs
    {
        get => this.GetFeatureOption(storage => storage.InventoryTabs);
        set => this.child.InventoryTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.child.InventoryTabList.Union(this.parent.InventoryTabList).ToHashSet();
        set => this.child.InventoryTabList = value;
    }

    /// <inheritdoc />
    public FeatureOption LabelChest
    {
        get => this.GetFeatureOption(storage => storage.LabelChest);
        set => this.child.LabelChest = value;
    }

    /// <inheritdoc />
    public FeatureOption OpenHeldChest
    {
        get => this.GetFeatureOption(storage => storage.OpenHeldChest);
        set => this.child.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public FeatureOption OrganizeChest
    {
        get => this.GetFeatureOption(storage => storage.OrganizeChest);
        set => this.child.OrganizeChest = value;
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get => this.child.OrganizeChestGroupBy == GroupBy.Default ? this.parent.OrganizeChestGroupBy : this.child.OrganizeChestGroupBy;
        set => this.child.OrganizeChestGroupBy = value;
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get => this.child.OrganizeChestSortBy == SortBy.Default ? this.parent.OrganizeChestSortBy : this.child.OrganizeChestSortBy;
        set => this.child.OrganizeChestSortBy = value;
    }

    /// <inheritdoc />
    public FeatureOption ResizeChest
    {
        get => this.GetFeatureOption(storage => storage.ResizeChest);
        set => this.child.ResizeChest = value;
    }

    /// <inheritdoc />
    public int ResizeChestCapacity
    {
        get => this.child.ResizeChestCapacity == 0 ? this.parent.ResizeChestCapacity : this.child.ResizeChestCapacity;
        set => this.child.ResizeChestCapacity = value;
    }

    /// <inheritdoc />
    public FeatureOption SearchItems
    {
        get => this.GetFeatureOption(storage => storage.SearchItems);
        set => this.child.SearchItems = value;
    }

    /// <inheritdoc />
    public FeatureOption SlotLock
    {
        get => this.GetFeatureOption(storage => storage.SlotLock);
        set => this.child.SlotLock = value;
    }

    /// <inheritdoc />
    public FeatureOptionRange StashToChest
    {
        get => this.GetFeatureOptionRange(storage => storage.StashToChest);
        set => this.child.StashToChest = value;
    }

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations
    {
        get => this.child.StashToChestDisableLocations.Union(this.parent.StashToChestDisableLocations).ToHashSet();
        set => this.child.StashToChestDisableLocations = value;
    }

    /// <inheritdoc />
    public int StashToChestDistance
    {
        get => this.child.StashToChestDistance == 0 ? this.parent.StashToChestDistance : this.child.StashToChestDistance;
        set => this.child.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get => this.child.StashToChestPriority == 0 ? this.parent.StashToChestPriority : this.child.StashToChestPriority;
        set => this.child.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks
    {
        get => this.GetFeatureOption(storage => storage.StashToChestStacks);
        set => this.child.StashToChestStacks = value;
    }

    /// <inheritdoc />
    public FeatureOption TransferItems
    {
        get => this.GetFeatureOption(storage => storage.TransferItems);
        set => this.child.TransferItems = value;
    }

    /// <inheritdoc />
    public FeatureOption UnloadChest
    {
        get => this.GetFeatureOption(storage => storage.UnloadChest);
        set => this.child.UnloadChest = value;
    }

    /// <inheritdoc />
    public FeatureOption UnloadChestCombine
    {
        get => this.GetFeatureOption(storage => storage.UnloadChestCombine);
        set => this.child.UnloadChestCombine = value;
    }

    private FeatureOption GetFeatureOption(Func<IStorage, FeatureOption> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch { _ when parentValue == FeatureOption.Disabled => FeatureOption.Disabled, FeatureOption.Default => parentValue, _ => childValue };
    }

    private FeatureOptionRange GetFeatureOptionRange(Func<IStorage, FeatureOptionRange> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch { _ when parentValue == FeatureOptionRange.Disabled => FeatureOptionRange.Disabled, FeatureOptionRange.Default => parentValue, _ => childValue };
    }
}
