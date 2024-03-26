namespace StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

using StardewMods.Common.Services.Integrations.BetterChests.Enums;

/// <summary>Configurable options for a storage container.</summary>
public interface IStorageOptions
{
    /// <summary>Gets or sets a value indicating if the chest can be automatically organized overnight.</summary>
    public FeatureOption AutoOrganize { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be carried by the player.</summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>Gets or sets a value indicating if can have categories added to it, and which items can be added.</summary>
    public FeatureOption CategorizeChest { get; set; }

    /// <summary>Gets or sets a value indicating whether items added to the chest will be added to the chest's categories.</summary>
    public FeatureOption CategorizeChestAutomatically { get; set; }

    /// <summary>Gets or sets a value indicating how categorized items will be displayed.</summary>
    public FilterMethod CategorizeChestMethod { get; set; }

    /// <summary>Gets or sets a value indicating what categories of items are allowed in the chest.</summary>
    public HashSet<string> CategorizeChestTags { get; set; }

    /// <summary>Gets or sets a value indicating whether chests  in the current location can be searched for.</summary>
    public FeatureOption ChestFinder { get; set; }

    /// <summary>Gets or sets a value indicating whether chest info will be displayed next to the chest menu.</summary>
    public FeatureOption ChestInfo { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can collect dropped items.</summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>Gets or sets a value indicating whether chests can be configured.</summary>
    public FeatureOption ConfigureChest { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely crafted from.</summary>
    public RangeOption CraftFromChest { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely crafted from.</summary>
    public int CraftFromChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating if the color picker will be replaced by an hsl color picker.</summary>
    public FeatureOption HslColorPicker { get; set; }

    /// <summary>Gets or sets a value indicating if tabs can be added to the chest menu.</summary>
    public FeatureOption InventoryTabs { get; set; }

    /// <summary>Gets or sets a value indicating which tabs will be added to the chest menu.</summary>
    public HashSet<string> InventoryTabList { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be opened while it's being carried in the players inventory.</summary>
    public FeatureOption OpenHeldChest { get; set; }

    /// <summary>Gets or sets a value the chest's carrying capacity.</summary>
    public CapacityOption ResizeChest { get; set; }

    /// <summary>Gets or sets a value indicating if a search bar will be added to the chest menu.</summary>
    public FeatureOption SearchItems { get; set; }

    /// <summary>Gets or sets a value indicating if the chest can be remotely stashed into.</summary>
    public RangeOption StashToChest { get; set; }

    /// <summary>Gets or sets a value indicating the distance in tiles that the chest can be remotely stashed into.</summary>
    public int StashToChestDistance { get; set; }

    /// <summary>Gets or sets a value indicating the priority that chests will be stashed into.</summary>
    public int StashToChestPriority { get; set; }

    /// <summary>Gets the name of the storage.</summary>
    /// <returns>Returns the name.</returns>
    public string GetDisplayName();

    /// <summary>Gets a description of the storage.</summary>
    /// <returns>Returns the description.</returns>
    public string GetDescription();
}