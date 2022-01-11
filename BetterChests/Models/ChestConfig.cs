namespace BetterChests.Models;

using System.Collections.Generic;
using BetterChests.Enums;
using Interfaces;

/// <inheritdoc />
internal class ChestConfig : IChestConfig
{
    /// <inheritdoc/>
    public FeatureOption AccessCarried { get; set; }

    /// <inheritdoc/>
    public int Capacity { get; set; }

    /// <inheritdoc/>
    public FeatureOption CarryChest { get; set; }

    /// <inheritdoc/>
    public FeatureOption CategorizeChest { get; set; }

    /// <inheritdoc/>
    public FeatureOption CollectItems { get; set; }

    /// <inheritdoc/>
    public FeatureOption ColorPicker { get; set; }

    /// <inheritdoc/>
    public FeatureOption SearchItems { get; set; }

    /// <inheritdoc/>
    public FeatureOption VacuumItems { get; set; }

    /// <inheritdoc/>
    public FeatureOptionRange CraftingRange { get; set; }

    /// <inheritdoc/>
    public FeatureOptionRange StashingRange { get; set; }

    /// <inheritdoc/>
    public HashSet<string> FilterItems { get; set; }
}