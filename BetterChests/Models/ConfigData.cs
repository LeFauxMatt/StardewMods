namespace BetterChests.Models;

using System.Collections.Generic;
using BetterChests.Enums;
using BetterChests.Interfaces;
using FuryCore.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
/// Mod config data related to BetterChests features.
/// </summary>
internal class ConfigData : IConfigData
{
    // ****************************************************************************************
    // General

    /// <inheritdoc/>
    public ComponentArea CustomColorPickerArea { get; set; } = ComponentArea.Right;

    /// <inheritdoc/>
    public char SearchTagSymbol { get; set; } = '#';

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption CategorizeChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption CollectItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest { get; set; } = FeatureOptionRange.Location;

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption FilterItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption ResizeChest { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOption SearchItems { get; set; } = FeatureOption.Enabled;

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest { get; set; } = FeatureOptionRange.Location;

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance { get; set; } = -1;

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList { get; set; } = ConfigData.EmptyList;

    /// <inheritdoc/>
    public bool FillStacks { get; set; } = true;

    /// <inheritdoc/>
    public int ResizeChestCapacity { get; set; } = 60;

    /// <inheritdoc/>
    public int ResizeChestMenuRows { get; set; } = 5;

    /// <inheritdoc/>
    public int StashToChestDistance { get; set; } = -1;

    // ****************************************************************************************
    // Controls

    /// <summary>
    /// Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.
    /// </summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>
    /// Gets or sets controls to stash player items into <see cref="StardewValley.Objects.Chest" />.
    /// </summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <summary>
    /// Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.
    /// </summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>
    /// Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.
    /// </summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>
    /// Gets or sets controls to switch to previous tab.
    /// </summary>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <summary>
    /// Gets or sets controls to switch to next tab.
    /// </summary>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    private static HashSet<string> EmptyList { get; } = new();
}