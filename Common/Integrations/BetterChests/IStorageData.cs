namespace StardewMods.Common.Integrations.BetterChests;

using StardewMods.Common.Enums;

/// <summary>Per storage configurable options.</summary>
public interface IStorageData
{
    /// <summary>Gets or sets a value indicating if the chest can be automatically organized overnight.</summary>
    public FeatureOption AutoOrganize { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be carried by the player.</summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.</summary>
    public FeatureOption CarryChestSlow { get; set; }

    /// <summary>Gets or sets a value indicating whether chest info will be displayed next to the chest menu.</summary>
    public FeatureOption ChestInfo { get; set; }

    /// <summary>Gets or sets a value indicating the label for a chest.</summary>
    public string ChestLabel { get; set; }

    /// <summary>Gets or sets a value indicating if tabs can be added to the chest menu.</summary>
    public FeatureOption ChestMenuTabs { get; set; }

    /// <summary>Gets or sets a value indicating which tabs will be added to the chest menu.</summary>
    public HashSet<string> ChestMenuTabSet { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can collect dropped items.</summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>Gets or sets a value indicating whether Configurator will be enabled.</summary>
    public FeatureOption Configurator { get; set; }

    /// <summary>Gets or sets what type of config menu will be available in game.</summary>
    public InGameMenu ConfigureMenu { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely crafted from.</summary>
    public FeatureOptionRange CraftFromChest { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating if the chest cannot be remotely crafted from while the player is in one of the
    ///     listed locations.
    /// </summary>
    public HashSet<string> CraftFromChestDisableLocations { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely crafted from.</summary>
    public int CraftFromChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating if the color picker will be replaced by an hsl color picker.</summary>
    public FeatureOption CustomColorPicker { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating if items outside of the filter item list will be greyed out and blocked from
    ///     being added to the chest.
    /// </summary>
    public FeatureOption FilterItems { get; set; }

    /// <summary>Gets or sets a value indicating what categories of items are allowed in the chest.</summary>
    public HashSet<string> FilterItemsList { get; set; }

    /// <summary>Gets or sets a value indicating whether items will be hidden or grayed out.</summary>
    public FeatureOption HideItems { get; set; }

    /// <summary>Gets or sets a value indicating whether chests can be labeled.</summary>
    public FeatureOption LabelChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be opened while it's being carried in the players inventory.</summary>
    public FeatureOption OpenHeldChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be organized with custom sorting/grouping.</summary>
    public FeatureOption OrganizeChest { get; set; }

    /// <summary>Gets or sets a value indicating how items will be grouped when organized.</summary>
    public GroupBy OrganizeChestGroupBy { get; set; }

    /// <summary>Gets or sets a value indicating how items will be sorted when organized.</summary>
    public SortBy OrganizeChestSortBy { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can have it's capacity resized.</summary>
    public FeatureOption ResizeChest { get; set; }

    /// <summary>Gets or sets a value indicating the number of item stacks that the chest can hold.</summary>
    public int ResizeChestCapacity { get; set; }

    /// <summary>Gets or sets a value indicating if a search bar will be added to the chest menu.</summary>
    public FeatureOption SearchItems { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely stashed into.</summary>
    public FeatureOptionRange StashToChest { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating if the chest cannot be remotely crafted from while the player is in one of the
    ///     listed locations.
    /// </summary>
    public HashSet<string> StashToChestDisableLocations { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely stashed into.</summary>
    public int StashToChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating the priority that chests will be stashed into.</summary>
    public int StashToChestPriority { get; set; }

    /// <summary>Gets or sets a value indicating if stashing into the chest will fill existing item stacks.</summary>
    public FeatureOption StashToChestStacks { get; set; }

    /// <summary>Gets or sets a value indicating whether to add button for transferring items to/from a chest.</summary>
    public FeatureOption TransferItems { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can have its inventory unloaded into another chest.</summary>
    public FeatureOption UnloadChest { get; set; }

    /// <summary>Gets or sets a value indicating whether unloaded chests will combine with target chest.</summary>
    public FeatureOption UnloadChestCombine { get; set; }

    /// <summary>Copies all properties to another instance of <see cref="IStorageData" />.</summary>
    /// <param name="other">The other <see cref="IStorageData" /> to copy properties to.</param>
    public void CopyTo(IStorageData other)
    {
        other.AutoOrganize = this.AutoOrganize;
        other.CarryChest = this.CarryChest;
        other.CarryChestSlow = this.CarryChestSlow;
        other.ChestInfo = this.ChestInfo;
        other.ChestLabel = this.ChestLabel;
        other.ChestMenuTabs = this.ChestMenuTabs;
        other.ChestMenuTabSet = new(this.ChestMenuTabSet);
        other.CollectItems = this.CollectItems;
        other.Configurator = this.Configurator;
        other.ConfigureMenu = this.ConfigureMenu;
        other.CraftFromChest = this.CraftFromChest;
        other.CraftFromChestDisableLocations = new(this.CraftFromChestDisableLocations);
        other.CraftFromChestDistance = this.CraftFromChestDistance;
        other.CustomColorPicker = this.CustomColorPicker;
        other.FilterItems = this.FilterItems;
        other.FilterItemsList = new(this.FilterItemsList);
        other.HideItems = this.HideItems;
        other.LabelChest = this.LabelChest;
        other.OpenHeldChest = this.OpenHeldChest;
        other.OrganizeChest = this.OrganizeChest;
        other.OrganizeChestGroupBy = this.OrganizeChestGroupBy;
        other.OrganizeChestSortBy = this.OrganizeChestSortBy;
        other.ResizeChest = this.ResizeChest;
        other.ResizeChestCapacity = this.ResizeChestCapacity;
        other.SearchItems = this.SearchItems;
        other.StashToChest = this.StashToChest;
        other.StashToChestDisableLocations = new(this.StashToChestDisableLocations);
        other.StashToChestDistance = this.StashToChestDistance;
        other.StashToChestPriority = this.StashToChestPriority;
        other.StashToChestStacks = this.StashToChestStacks;
        other.TransferItems = this.TransferItems;
        other.UnloadChest = this.UnloadChest;
        other.UnloadChestCombine = this.UnloadChestCombine;
    }
}
