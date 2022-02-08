namespace StardewMods.BetterChests.Interfaces;

using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Enums;

/// <summary>
///     Mod config data.
/// </summary>
internal interface IConfigData
{
    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public int CarryChestLimit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public int CarryChestSlow { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be categorized.
    /// </summary>
    public bool CategorizeChest { get; set; }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    ControlScheme ControlScheme { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="CustomColorPicker" /> will be aligned to.
    /// </summary>
    public ComponentArea CustomColorPickerArea { get; set; }

    /// <summary>
    ///     Gets or sets the default chest configuration.
    /// </summary>
    StorageData DefaultChest { get; set; }

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public char SearchTagSymbol { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public bool SlotLock { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IConfigData
    {
        other.CarryChestLimit = this.CarryChestLimit;
        other.CarryChestSlow = this.CarryChestSlow;
        other.CategorizeChest = this.CategorizeChest;
        ((IControlScheme)other.ControlScheme).CopyTo(this.ControlScheme);
        other.CustomColorPickerArea = this.CustomColorPickerArea;
        ((IStorageData)other.DefaultChest).CopyTo(this.DefaultChest);
        other.SearchTagSymbol = this.SearchTagSymbol;
        other.SlotLock = this.SlotLock;
    }
}