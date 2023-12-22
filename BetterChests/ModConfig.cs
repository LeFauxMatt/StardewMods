namespace StardewMods.BetterChests;

using System.Globalization;
using System.Text;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewMods.BetterChests.Framework.Services.Features;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class ModConfig : IConfigWithLogLevel
{
    /// <summary>Gets or sets a value containing the default storage options.</summary>
    public DefaultStorage DefaultOptions { get; set; } = new();

    /// <summary>Gets or sets a value indicating how many chests can be carried at once.</summary>
    public int CarryChestLimit { get; set; } = 1;

    /// <summary>Gets or sets a value indicating how many chests can be carried before applying a slowness effect.</summary>
    public int CarryChestSlowLimit { get; set; } = 1;

    /// <summary>Gets or sets the control scheme.</summary>
    public Controls Controls { get; set; } = new();

    /// <summary>Gets or sets a value indicating the range which workbenches will craft from.</summary>
    public FeatureOptionRange CraftFromWorkbench { get; set; } = FeatureOptionRange.Location;

    /// <summary>Gets or sets a value indicating the distance in tiles that the workbench can be remotely crafted from.</summary>
    public int CraftFromWorkbenchDistance { get; set; } = -1;

    /// <summary>
    /// Gets or sets the <see cref="Framework.Enums.ColorPickerArea" /> that the <see cref="HslColorPicker" /> will be
    /// aligned to.
    /// </summary>
    public ColorPickerArea ColorPickerArea { get; set; } = ColorPickerArea.Right;

    /// <summary>Gets or sets a value indicating whether experimental features will be enabled.</summary>
    public bool Experimental { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Framework.Enums.InventoryTabArea" /> that the <see cref="InventoryTabs" /> will be
    /// aligned to.
    /// </summary>
    public InventoryTabArea InventoryTabArea { get; set; } = InventoryTabArea.Bottom;

    /// <summary>Gets or sets the symbol used to denote context tags in searches.</summary>
    public char SearchTagSymbol { get; set; } = '#';

    /// <summary>Gets or sets the symbol used to denote negative searches.</summary>
    public char SearchNegationSymbol { get; set; } = '!';

    /// <summary>Gets or sets the color of locked slots.</summary>
    public string SlotLockColor { get; set; } = "Red";

    /// <summary>Gets or sets a value indicating whether the slot lock button needs to be held down.</summary>
    public bool SlotLockHold { get; set; } = true;

    /// <inheritdoc />
    public LogLevels LogLevel { get; set; } = LogLevels.Less;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(" Main Config".PadLeft(50, '=')[^50..]);
        sb.AppendLine(CultureInfo.InvariantCulture, $"CarryChestLimit: {this.CarryChestLimit.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"CarryChestSlowAmount: {this.CarryChestSlowLimit.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"CraftFromWorkbench: {this.CraftFromWorkbench.ToStringFast()}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"CraftFromWorkbenchDistance: {this.CraftFromWorkbenchDistance.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"CustomColorPickerArea: {this.ColorPickerArea.ToStringFast()}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"SearchTagSymbol: {this.SearchTagSymbol.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(CultureInfo.InvariantCulture, $"SlotLockColor: {this.SlotLockColor}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"SlotLockHold: {this.SlotLockHold.ToString(CultureInfo.InvariantCulture)}");

        sb.AppendLine(" Control Scheme".PadLeft(50, '=')[^50..]);
        sb.Append(this.Controls);

        sb.AppendLine(" Default Storage".PadLeft(50, '=')[^50..]);
        sb.Append(base.ToString());

        return sb.ToString();
    }
}
