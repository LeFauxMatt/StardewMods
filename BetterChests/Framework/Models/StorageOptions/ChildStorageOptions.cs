namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;

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
    public Option AutoOrganize
    {
        get => this.GetOption(storage => storage.AutoOrganize);
        set => this.child.AutoOrganize = value;
    }

    /// <inheritdoc />
    public Option CarryChest
    {
        get => this.GetOption(storage => storage.CarryChest);
        set => this.child.CarryChest = value;
    }

    /// <inheritdoc />
    public Option ChestFinder
    {
        get => this.GetOption(storage => storage.ChestFinder);
        set => this.child.ChestFinder = value;
    }

    /// <inheritdoc />
    public Option ChestInfo
    {
        get => this.GetOption(storage => storage.ChestInfo);
        set => this.child.ChestInfo = value;
    }

    /// <inheritdoc />
    public string ChestLabel
    {
        get => this.child.ChestLabel;
        set => this.child.ChestLabel = value;
    }

    /// <inheritdoc />
    public Option CollectItems
    {
        get => this.GetOption(storage => storage.CollectItems);
        set => this.child.CollectItems = value;
    }

    /// <inheritdoc />
    public Option ConfigureChest
    {
        get => this.GetOption(storage => storage.ConfigureChest);
        set => this.child.ConfigureChest = value;
    }

    /// <inheritdoc />
    public InGameMenu ConfigureMenu
    {
        get => this.child.ConfigureMenu == InGameMenu.Default ? this.parent.ConfigureMenu : this.child.ConfigureMenu;
        set => this.child.ConfigureMenu = value;
    }

    /// <inheritdoc />
    public RangeOption CraftFromChest
    {
        get => this.GetRange(storage => storage.CraftFromChest);
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
        get =>
            this.child.CraftFromChestDistance == 0
                ? this.parent.CraftFromChestDistance
                : this.child.CraftFromChestDistance;
        set => this.child.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public Option HslColorPicker
    {
        get => this.GetOption(storage => storage.HslColorPicker);
        set => this.child.HslColorPicker = value;
    }

    /// <inheritdoc />
    public Option FilterItems
    {
        get => this.GetOption(storage => storage.FilterItems);
        set => this.child.FilterItems = value;
    }

    /// <inheritdoc />
    public HashSet<string> FilterItemsList
    {
        get => this.child.FilterItemsList.Union(this.parent.FilterItemsList).ToHashSet();
        set => this.child.FilterItemsList = value;
    }

    /// <inheritdoc />
    public Option HideUnselectedItems
    {
        get => this.GetOption(storage => storage.HideUnselectedItems);
        set => this.child.HideUnselectedItems = value;
    }

    /// <inheritdoc />
    public Option InventoryTabs
    {
        get => this.GetOption(storage => storage.InventoryTabs);
        set => this.child.InventoryTabs = value;
    }

    /// <inheritdoc />
    public HashSet<string> InventoryTabList
    {
        get => this.child.InventoryTabList.Union(this.parent.InventoryTabList).ToHashSet();
        set => this.child.InventoryTabList = value;
    }

    /// <inheritdoc />
    public Option LabelChest
    {
        get => this.GetOption(storage => storage.LabelChest);
        set => this.child.LabelChest = value;
    }

    /// <inheritdoc />
    public Option OpenHeldChest
    {
        get => this.GetOption(storage => storage.OpenHeldChest);
        set => this.child.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public Option OrganizeChest
    {
        get => this.GetOption(storage => storage.OrganizeChest);
        set => this.child.OrganizeChest = value;
    }

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy
    {
        get =>
            this.child.OrganizeChestGroupBy == GroupBy.Default
                ? this.parent.OrganizeChestGroupBy
                : this.child.OrganizeChestGroupBy;
        set => this.child.OrganizeChestGroupBy = value;
    }

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy
    {
        get =>
            this.child.OrganizeChestSortBy == SortBy.Default
                ? this.parent.OrganizeChestSortBy
                : this.child.OrganizeChestSortBy;
        set => this.child.OrganizeChestSortBy = value;
    }

    /// <inheritdoc />
    public CapacityOption ResizeChest
    {
        get => this.GetCapacity(storage => storage.ResizeChest);
        set => this.child.ResizeChest = value;
    }

    /// <inheritdoc />
    public Option SearchItems
    {
        get => this.GetOption(storage => storage.SearchItems);
        set => this.child.SearchItems = value;
    }

    /// <inheritdoc />
    public Option LockItemSlot
    {
        get => this.GetOption(storage => storage.LockItemSlot);
        set => this.child.LockItemSlot = value;
    }

    /// <inheritdoc />
    public RangeOption StashToChest
    {
        get => this.GetRange(storage => storage.StashToChest);
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
        get =>
            this.child.StashToChestDistance == 0 ? this.parent.StashToChestDistance : this.child.StashToChestDistance;
        set => this.child.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public int StashToChestPriority
    {
        get =>
            this.child.StashToChestPriority == 0 ? this.parent.StashToChestPriority : this.child.StashToChestPriority;
        set => this.child.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public Option StashToChestStacks
    {
        get => this.GetOption(storage => storage.StashToChestStacks);
        set => this.child.StashToChestStacks = value;
    }

    /// <inheritdoc />
    public Option TransferItems
    {
        get => this.GetOption(storage => storage.TransferItems);
        set => this.child.TransferItems = value;
    }

    /// <inheritdoc />
    public Option UnloadChest
    {
        get => this.GetOption(storage => storage.UnloadChest);
        set => this.child.UnloadChest = value;
    }

    /// <inheritdoc />
    public Option UnloadChestCombine
    {
        get => this.GetOption(storage => storage.UnloadChestCombine);
        set => this.child.UnloadChestCombine = value;
    }

    /// <inheritdoc />
    public virtual string GetDescription() => this.parent.GetDescription();

    /// <inheritdoc />
    public virtual string GetDisplayName() => this.parent.GetDisplayName();

    private CapacityOption GetCapacity(Func<IStorageOptions, CapacityOption> selector)
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

    private Option GetOption(Func<IStorageOptions, Option> selector)
    {
        var childValue = selector(this.child);
        var parentValue = selector(this.parent);
        return childValue switch
        {
            _ when parentValue == Option.Disabled => Option.Disabled,
            Option.Default => parentValue,
            _ => childValue,
        };
    }

    private RangeOption GetRange(Func<IStorageOptions, RangeOption> selector)
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