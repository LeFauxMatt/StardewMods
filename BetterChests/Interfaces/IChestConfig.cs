namespace BetterChests.Interfaces;

using System.Collections.Generic;
using BetterChests.Enums;
using StardewValley.Menus;

internal interface IChestConfig
{
    /// <summary>
    /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can be accessed while carried.
    /// </summary>
    public FeatureOption AccessCarried { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items the <see cref="StardewValley.Objects.Chest" /> is able to hold.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can be carried by the player.
    /// </summary>
    public FeatureOption CarryChest { get; set; }

    /// <summary>
    /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can be categorized by the player.
    /// </summary>
    public FeatureOption CategorizeChest { get; set; }

    /// <summary>
    /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can collect <see cref="StardewValley.Debris" />.
    /// </summary>
    public FeatureOption CollectItems { get; set; }

    /// <summary>
    /// Gets or sets if the <see cref="DiscreteColorPicker" /> will be replaced with a <see cref="" />.
    /// </summary>
    public FeatureOption ColorPicker { get; set; }

    /// <summary>
    /// Gets or sets if the <see cref="StardewValley.Objects.Chest" /> will have a search field added to it.
    /// </summary>
    public FeatureOption SearchItems { get; set; }

    /// <summary>
    /// Gets or sets if the <see cref="StardewValley.Objects.Chest" /> will collect item Debris.
    /// </summary>
    public FeatureOption VacuumItems { get; set; }

    /// <summary>
    /// Gets or sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely stashed into.
    /// </summary>
    public FeatureOptionRange CraftingRange { get; set; }

    /// <summary>
    /// Gets or sets the range that the <see cref="StardewValley.Objects.Chest" /> can be remotely crafted from.
    /// </summary>
    public FeatureOptionRange StashingRange { get; set; }

    /// <summary>
    /// Gets or sets items that the <see cref="StardewValley.Objects.Chest" /> can accept or will block.
    /// </summary>
    public HashSet<string> FilterItems { get; set; }

    public void CopyTo<TOther>(TOther other)
        where TOther : IChestConfig
    {
        other.Capacity = this.Capacity;
        other.CarryChest = this.CarryChest;
        other.CategorizeChest = this.CategorizeChest;
        other.CollectItems = this.CollectItems;
        other.CraftingRange = this.CraftingRange;
        other.StashingRange = this.StashingRange;
        other.FilterItems = this.FilterItems;
    }
}