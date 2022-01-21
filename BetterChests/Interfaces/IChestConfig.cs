namespace BetterChests.Interfaces;

using System.Collections.Generic;
using BetterChests.Enums;

/// <summary>
///
/// </summary>
internal interface IChestConfig
{
    /// <summary>
    /// Gets or sets the maximum number of items the <see cref="StardewValley.Objects.Chest" /> is able to hold.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets whether the <see cref="StardewValley.Objects.Chest" /> can collect <see cref="StardewValley.Debris" />.
    /// </summary>
    public FeatureOption CollectItems { get; set; }

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

    /// <summary>
    ///
    /// </summary>
    /// <param name="other"></param>
    /// <typeparam name="TOther"></typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IChestConfig
    {
        other.Capacity = this.Capacity;
        other.CollectItems = this.CollectItems;
        other.CraftingRange = this.CraftingRange;
        other.StashingRange = this.StashingRange;
        other.FilterItems = this.FilterItems;
    }
}