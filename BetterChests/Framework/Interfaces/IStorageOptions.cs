namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.BetterChests.Framework.Enums;

/// <summary>Configurable options for a storage container.</summary>
internal interface IStorageOptions
{
    /// <summary>Gets or sets a value indicating if the chest can be automatically organized overnight.</summary>
    public Option AutoOrganize { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be carried by the player.</summary>
    public Option CarryChest { get; set; }

    /// <summary>Gets or sets a value indicating if can have categories added to it, and which items can be added.</summary>
    public Option CategorizeChest { get; set; }

    /// <summary>Gets or sets a value indicating what categories of items are allowed in the chest.</summary>
    public HashSet<string> CategorizeChestTags { get; set; }

    /// <summary>Gets or sets a value indicating whether chests  in the current location can be searched for.</summary>
    public Option ChestFinder { get; set; }

    /// <summary>Gets or sets a value indicating whether chest info will be displayed next to the chest menu.</summary>
    public Option ChestInfo { get; set; }

    /// <summary>Gets or sets a value indicating the label for a chest.</summary>
    public string ChestLabel { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can collect dropped items.</summary>
    public Option CollectItems { get; set; }

    /// <summary>Gets or sets a value indicating whether chests can be configured.</summary>
    public Option ConfigureChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely crafted from.</summary>
    public RangeOption CraftFromChest { get; set; }

    /// <summary>Gets or sets a value indicating if the color picker will be replaced by an hsl color picker.</summary>
    public Option HslColorPicker { get; set; }

    /// <summary>Gets or sets a value indicating if tabs can be added to the chest menu.</summary>
    public Option InventoryTabs { get; set; }

    /// <summary>Gets or sets a value indicating which tabs will be added to the chest menu.</summary>
    public HashSet<string> InventoryTabList { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be opened while it's being carried in the players inventory.</summary>
    public Option OpenHeldChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be organized with custom sorting/grouping.</summary>
    public Option OrganizeItems { get; set; }

    /// <summary>Gets or sets a value indicating how items will be grouped when organized.</summary>
    public GroupBy OrganizeItemsGroupBy { get; set; }

    /// <summary>Gets or sets a value indicating how items will be sorted when organized.</summary>
    public SortBy OrganizeItemsSortBy { get; set; }

    /// <summary>Gets or sets a value the chest's carrying capacity.</summary>
    public CapacityOption ResizeChest { get; set; }

    /// <summary>Gets or sets a value indicating if a search bar will be added to the chest menu.</summary>
    public Option SearchItems { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely stashed into.</summary>
    public RangeOption StashToChest { get; set; }

    /// <summary>Gets or sets a value indicating the priority that chests will be stashed into.</summary>
    public int StashToChestPriority { get; set; }

    /// <summary>Gets or sets a value indicating whether to add button for transferring items to/from a chest.</summary>
    public Option TransferItems { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can have its inventory unloaded into another chest.</summary>
    public Option UnloadChest { get; set; }

    /// <summary>Gets the name of the storage.</summary>
    /// <returns>Returns the name.</returns>
    public string GetDisplayName();

    /// <summary>Gets a description of the storage.</summary>
    /// <returns>Returns the description.</returns>
    public string GetDescription();
}