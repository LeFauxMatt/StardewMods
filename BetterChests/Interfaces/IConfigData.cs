namespace StardewMods.BetterChests.Interfaces;

using StardewMods.FuryCore.Enums;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;

/// <summary>
/// Mod config data.
/// </summary>
internal interface IConfigData
{
    // ****************************************************************************************
    // Features

    /// <summary>
    /// Gets or sets a value indicating whether chests can be categorized.
    /// </summary>
    public bool CategorizeChest { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; }

    // ****************************************************************************************
    // General

    /// <summary>
    /// Gets or sets the <see cref="ComponentArea" /> that the <see cref="CustomColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

    /// <summary>
    /// Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; }

    /// <summary>
    /// Gets or sets the control scheme.
    /// </summary>
    ControlScheme ControlScheme { get; set; }

    /// <summary>
    /// Gets or sets the default chest configuration.
    /// </summary>
    ChestData DefaultChest { get; set; }

    /// <summary>
    /// Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IConfigData
    {
        other.CategorizeChest = this.CategorizeChest;
        other.SlotLock = this.SlotLock;
        other.CustomColorPickerArea = this.CustomColorPickerArea;
        other.SearchTagSymbol = this.SearchTagSymbol;
        ((IControlScheme)other.ControlScheme).CopyTo(this.ControlScheme);
        ((IChestData)other.DefaultChest).CopyTo(this.DefaultChest);
    }
}