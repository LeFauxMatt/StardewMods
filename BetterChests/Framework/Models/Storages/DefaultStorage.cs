namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultStorage : IStorage
{
    /// <inheritdoc />
    public FeatureOption CarryChestSlow { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption AutoOrganize { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption CarryChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption ChestFinder { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption ChestInfo { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public string ChestLabel { get; set; } = string.Empty;

    /// <inheritdoc />
    public FeatureOption CollectItems { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption ConfigureChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public InGameMenu ConfigureMenu { get; set; } = InGameMenu.Simple;

    /// <inheritdoc />
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Location;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public int CraftFromChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public FeatureOption HslColorPicker { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption FilterItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public HashSet<string> FilterItemsList { get; set; } = [];

    /// <inheritdoc />
    public FeatureOption HideUnselectedItems { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption InventoryTabs { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public HashSet<string> InventoryTabList { get; set; } =
    [
        "Clothing", "Cooking", "Crops", "Equipment", "Fishing", "Materials", "Misc", "Seeds",
    ];

    /// <inheritdoc />
    public FeatureOption LabelChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption OrganizeChest { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public GroupBy OrganizeChestGroupBy { get; set; } = GroupBy.Default;

    /// <inheritdoc />
    public SortBy OrganizeChestSortBy { get; set; } = SortBy.Default;

    /// <inheritdoc />
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public int ResizeChestCapacity { get; set; } = 60;

    /// <inheritdoc />
    public FeatureOption SearchItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption SlotLock { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Location;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations { get; set; } = [];

    /// <inheritdoc />
    public int StashToChestDistance { get; set; } = -1;

    /// <inheritdoc />
    public int StashToChestPriority { get; set; }

    /// <inheritdoc />
    public FeatureOption StashToChestStacks { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption TransferItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc />
    public FeatureOption UnloadChest { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public FeatureOption UnloadChestCombine { get; set; } = FeatureOption.Disabled;

    /// <inheritdoc />
    public string GetDescription() => I18n.Storage_Other_Tooltip();

    /// <inheritdoc />
    public string GetDisplayName() => I18n.Storage_Other_Name();
}