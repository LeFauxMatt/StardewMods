namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultStorageOptions : IStorageOptions
{
    /// <inheritdoc />
    public Option AutoOrganize { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option CarryChest { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option CategorizeChest { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public HashSet<string> CategorizeChestTags { get; set; } = [];

    /// <inheritdoc />
    public Option ChestFinder { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option ChestInfo { get; set; } = Option.Disabled;

    /// <inheritdoc />
    public string ChestLabel { get; set; } = string.Empty;

    /// <inheritdoc />
    public Option CollectItems { get; set; } = Option.Disabled;

    /// <inheritdoc />
    public Option ConfigureChest { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public RangeOption CraftFromChest { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public Option HslColorPicker { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option InventoryTabs { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public HashSet<string> InventoryTabList { get; set; } =
    [
        "Clothing", "Cooking", "Crops", "Equipment", "Fishing", "Materials", "Misc", "Seeds",
    ];

    /// <inheritdoc />
    public Option OpenHeldChest { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option OrganizeItems { get; set; } = Option.Disabled;

    /// <inheritdoc />
    public GroupBy OrganizeItemsGroupBy { get; set; } = GroupBy.Default;

    /// <inheritdoc />
    public SortBy OrganizeItemsSortBy { get; set; } = SortBy.Default;

    /// <inheritdoc />
    public CapacityOption ResizeChest { get; set; } = CapacityOption.Large;

    /// <inheritdoc />
    public Option SearchItems { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public RangeOption StashToChest { get; set; } = RangeOption.Location;

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public Option TransferItems { get; set; } = Option.Enabled;

    /// <inheritdoc />
    public Option UnloadChest { get; set; } = Option.Disabled;

    /// <inheritdoc />
    public string GetDescription() => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public string GetDisplayName() => I18n.Storage_Other_Name();
}