namespace BetterChests.Models;

using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

internal class ModConfig
{
    /// <summary>Gets or sets individual config for each chest.</summary>
    public Dictionary<string, ChestConfig> ChestConfigs { get; set; } = new()
    {
        { string.Empty, new() },
        { "Chest", new() },
        { "Stone Chest", new() },
        { "Junimo Chest", new() },
        { "Mini-Shipping Bin", new() },
        { "Mini-Fridge", new() },
        { "Auto-Grabber", new() },
    };

    /// <summary>Gets or sets if chests can be categorized.</summary>
    public bool CategorizedChests { get; set; } = true;

    /// <summary>Gets or sets if tabs will be added to the chest menu.</summary>
    public bool ChestTabs { get; set; } = true;

    /// <summary>Gets or sets if the HSL Color Picker should replace the vanilla color picker.</summary>
    public bool ColorPicker { get; set; } = true;

    /// <summary>Gets or sets maximum number of rows to show in the <see cref="StardewValley.Menus.ItemGrabMenu" />.</summary>
    public int MenuRows { get; set; } = 5;

    /// <summary>Gets or sets the symbol used to denote context tags in searches.</summary>
    public char SearchTagSymbol { get; set; } = '#';

    /// <summary>Gets or sets if the search bar will be shown in the <see cref="StardewValley.Menus.ItemGrabMenu" />.</summary>
    public bool SearchItems { get; set; } = true;

    /// <summary>Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.</summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>Gets or sets controls to stash player items into <see cref="StardewValley.Objects.Chest" />.</summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.</summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.</summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>Gets or sets controls to switch to previous tab.</summary>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <summary>Gets or sets controls to switch to next tab.</summary>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);
}