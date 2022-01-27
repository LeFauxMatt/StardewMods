namespace BetterChests.Models;

using System.Collections.Generic;
using BetterChests.Enums;
using BetterChests.Interfaces;

/// <inheritdoc />
internal class ChestData : IChestData
{
    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption CategorizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption CollectItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption FilterItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOption SearchItems { get; set; } = FeatureOption.Default;

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Default;

    /// <inheritdoc/>
    public FeatureOption UnloadChest { get; set; } = FeatureOption.Default;

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance { get; set; } = 0;

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList { get; set; } = new();

    /// <inheritdoc/>
    public bool StashToChestStacks { get; set; } = true;

    /// <inheritdoc/>
    public int ResizeChestCapacity { get; set; } = 0;

    /// <inheritdoc/>
    public int ResizeChestMenuRows { get; set; } = 0;

    /// <inheritdoc/>
    public int StashToChestDistance { get; set; } = 0;
}