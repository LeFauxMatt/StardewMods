namespace BetterChests.Interfaces;

using System.Collections.Generic;
using BetterChests.Enums;
using StardewValley.Objects;

/// <summary>
/// <see cref="Chest" /> data related to BetterChests features.
/// </summary>
internal interface IChestData
{
    // ****************************************************************************************
    // Features

    /// <summary>
    /// Gets or sets the feature that allows the chest to be carried by the player.
    /// </summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>
    /// Gets or sets the feature that allows the chest to be categorized.
    /// </summary>
    public FeatureOption CategorizeChest { get; set; }

    /// <summary>
    /// Gets or sets the feature that adds tabs to the chest menu.
    /// </summary>
    public FeatureOption ChestMenuTabs { get; set; }

    /// <summary>
    /// Gets or sets the feature that allows the chest to collect dropped items.
    /// </summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>
    /// Gets or sets the range that the chest can be remotely stashed into.
    /// </summary>
    public FeatureOptionRange CraftFromChest { get; set; }

    /// <summary>
    /// Gets or sets the feature that replaces the Color Picker with an HSL Color Picker.
    /// </summary>
    public FeatureOption CustomColorPicker { get; set; }

    /// <summary>
    /// Gets or sets the feature that allows the chest to restrict what items it will accept.
    /// </summary>
    public FeatureOption FilterItems { get; set; }

    /// <summary>
    /// Gets or sets the feature that allows the chest to be opened while being held.
    /// </summary>
    public FeatureOption OpenHeldChest { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items the chest can hold.
    /// </summary>
    public FeatureOption ResizeChest { get; set; }

    /// <summary>
    /// Gets or sets maximum number of rows to show in the chest inventory menu.
    /// </summary>
    public FeatureOption ResizeChestMenu { get; set; }

    /// <summary>
    /// Gets or sets the feature that adds a search bar to the chest inventory menu.
    /// </summary>
    public FeatureOption SearchItems { get; set; }

    /// <summary>
    /// Gets or sets the range that the chest can be remotely crafted from.
    /// </summary>
    public FeatureOptionRange StashToChest { get; set; }

    // ****************************************************************************************
    // Feature Options

    /// <summary>
    /// Gets or sets the distance that the <see cref="StardewValley.Objects.Chest" /> can be remotely stashed into.
    /// </summary>
    public int CraftFromChestDistance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether stashing will fill existing stacks.
    /// </summary>
    public bool StashToChestStacks { get; set; }

    /// <summary>
    /// Gets or sets the items that the chest will accept.
    /// </summary>
    public HashSet<string> FilterItemsList { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items the chest can hold.
    /// </summary>
    public int ResizeChestCapacity { get; set; }

    /// <summary>
    /// Gets or sets maximum number of rows to show in the chest inventory menu.
    /// </summary>
    public int ResizeChestMenuRows { get; set; }

    /// <summary>
    /// Gets or sets the distance that the <see cref="StardewValley.Objects.Chest" /> can be remotely crafted from.
    /// </summary>
    public int StashToChestDistance { get; set; }

    /// <summary>
    /// Copies data from one <see cref="IChestData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IChestData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IChestData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IChestData
    {
        other.CarryChest = this.CarryChest;
        other.CategorizeChest = this.CategorizeChest;
        other.ChestMenuTabs = this.ChestMenuTabs;
        other.CollectItems = this.CollectItems;
        other.CraftFromChest = this.CraftFromChest;
        other.CustomColorPicker = this.CustomColorPicker;
        other.FilterItems = this.FilterItems;
        other.OpenHeldChest = this.OpenHeldChest;
        other.ResizeChest = this.ResizeChest;
        other.ResizeChestMenu = this.ResizeChestMenu;
        other.SearchItems = this.SearchItems;
        other.StashToChest = this.StashToChest;
        other.CraftFromChestDistance = this.CraftFromChestDistance;
        other.StashToChestStacks = this.StashToChestStacks;
        other.FilterItemsList = this.FilterItemsList;
        other.ResizeChestCapacity = this.ResizeChestCapacity;
        other.ResizeChestMenuRows = this.ResizeChestMenuRows;
        other.StashToChestDistance = this.StashToChestDistance;
    }
}