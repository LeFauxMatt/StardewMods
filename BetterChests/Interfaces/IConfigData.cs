namespace BetterChests.Interfaces;

using FuryCore.Enums;
using FuryCore.UI;
using StardewModdingAPI.Utilities;

/// <summary>
/// Mod config data related to BetterChests features.
/// </summary>
internal interface IConfigData : IChestData
{
    // ****************************************************************************************
    // General

    /// <summary>
    /// Gets or sets the area that the <see cref="HslColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

    /// <summary>
    /// Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; }

    // ****************************************************************************************
    // Controls

    /// <summary>
    /// Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.
    /// </summary>
    public KeybindList OpenCrafting { get; set; }

    /// <summary>
    /// Gets or sets controls to stash player items into <see cref="StardewValley.Objects.Chest" />.
    /// </summary>
    public KeybindList StashItems { get; set; }

    /// <summary>
    /// Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.
    /// </summary>
    public KeybindList ScrollUp { get; set; }

    /// <summary>
    /// Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.
    /// </summary>
    public KeybindList ScrollDown { get; set; }

    /// <summary>
    /// Gets or sets controls to switch to previous tab.
    /// </summary>
    public KeybindList PreviousTab { get; set; }

    /// <summary>
    /// Gets or sets controls to switch to next tab.
    /// </summary>
    public KeybindList NextTab { get; set; }

    /// <summary>
    /// Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyConfigDataTo<TOther>(TOther other)
        where TOther : IConfigData
    {
        other.CustomColorPickerArea = this.CustomColorPickerArea;
        other.SearchTagSymbol = this.SearchTagSymbol;
        other.OpenCrafting = this.OpenCrafting;
        other.StashItems = this.StashItems;
        other.ScrollUp = this.ScrollUp;
        other.ScrollDown = this.ScrollDown;
        other.PreviousTab = this.PreviousTab;
        other.NextTab = this.NextTab;
        other.CopyChestDataTo(this);
    }
}