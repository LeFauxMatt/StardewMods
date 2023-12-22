namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.BetterChests.Framework.Enums;

/// <summary>Configurable options for a storage container.</summary>
internal interface IStorage
{
    /// <summary>Gets or sets a value indicating if the chest can be automatically organized overnight.</summary>
    public FeatureOption AutoOrganize { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be carried by the player.</summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>Gets or sets a value indicating whether chests  in the current location can be searched for.</summary>
    public FeatureOption ChestFinder { get; set; }

    /// <summary>Gets or sets a value indicating whether chest info will be displayed next to the chest menu.</summary>
    public FeatureOption ChestInfo { get; set; }

    /// <summary>Gets or sets a value indicating the label for a chest.</summary>
    public string ChestLabel { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can collect dropped items.</summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>Gets or sets a value indicating whether chests can be configured.</summary>
    public FeatureOption ConfigureChest { get; set; }

    /// <summary>Gets or sets what type of config menu will be available in game.</summary>
    public InGameMenu ConfigureMenu { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely crafted from.</summary>
    public FeatureOptionRange CraftFromChest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the chest cannot be remotely crafted from while the player is in one of the
    /// listed locations.
    /// </summary>
    public HashSet<string> CraftFromChestDisableLocations { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely crafted from.</summary>
    public int CraftFromChestDistance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if items outside of the filter item list will be greyed out and blocked from
    /// being added to the chest.
    /// </summary>
    public FeatureOption FilterItems { get; set; }

    /// <summary>Gets or sets a value indicating what categories of items are allowed in the chest.</summary>
    public HashSet<string> FilterItemsList { get; set; }

    /// <summary>Gets or sets a value indicating whether items will be hidden or grayed out.</summary>
    public FeatureOption HideUnselectedItems { get; set; }

    /// <summary>Gets or sets a value indicating if the color picker will be replaced by an hsl color picker.</summary>
    public FeatureOption HslColorPicker { get; set; }

    /// <summary>Gets or sets a value indicating if tabs can be added to the chest menu.</summary>
    public FeatureOption InventoryTabs { get; set; }

    /// <summary>Gets or sets a value indicating which tabs will be added to the chest menu.</summary>
    public HashSet<string> InventoryTabList { get; set; }

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

    /// <summary>Gets or sets a value indicating whether the slot lock feature is enabled.</summary>
    public FeatureOption SlotLock { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely stashed into.</summary>
    public FeatureOptionRange StashToChest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the chest cannot be remotely crafted from while the player is in one of the
    /// listed locations.
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
}