namespace StardewMods.BetterChests.Models;

using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;

/// <summary>
///     Mod config data related to BetterChests features.
/// </summary>
internal class ConfigData : IConfigData
{
    /// <inheritdoc />
    public int CarryChestLimit { get; set; } = 0;

    /// <inheritdoc />
    public bool CategorizeChest { get; set; } = true;

    /// <inheritdoc />
    public ControlScheme ControlScheme { get; set; } = new();

    /// <inheritdoc />
    public ComponentArea CustomColorPickerArea { get; set; } = ComponentArea.Right;

    /// <inheritdoc />
    public ChestData DefaultChest { get; set; } = new()
    {
        CarryChest = FeatureOption.Enabled,
        ChestMenuTabs = FeatureOption.Enabled,
        CollectItems = FeatureOption.Enabled,
        CraftFromChest = FeatureOptionRange.Location,
        CraftFromChestDistance = -1,
        CustomColorPicker = FeatureOption.Enabled,
        FilterItems = FeatureOption.Enabled,
        OpenHeldChest = FeatureOption.Enabled,
        ResizeChest = FeatureOption.Enabled,
        ResizeChestCapacity = 60,
        ResizeChestMenu = FeatureOption.Enabled,
        ResizeChestMenuRows = 5,
        SearchItems = FeatureOption.Enabled,
        StashToChest = FeatureOptionRange.Location,
        StashToChestDistance = -1,
        StashToChestStacks = FeatureOption.Enabled,
        UnloadChest = FeatureOption.Enabled,
    };

    /// <inheritdoc />
    public char SearchTagSymbol { get; set; } = '#';

    /// <inheritdoc />
    public bool SlotLock { get; set; } = true;
}