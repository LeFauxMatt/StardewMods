namespace BetterChests.Interfaces;

using BetterChests.Models;
using FuryCore.Enums;
using FuryCore.UI;

/// <summary>
/// Mod config data.
/// </summary>
internal interface IConfigData
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

    /// <summary>
    /// Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; }

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
        other.CustomColorPickerArea = this.CustomColorPickerArea;
        other.SearchTagSymbol = this.SearchTagSymbol;
        other.SlotLock = this.SlotLock;
        ((IControlScheme)other.ControlScheme).CopyTo(this.ControlScheme);
        ((IChestData)other.DefaultChest).CopyTo(this.DefaultChest);
    }
}