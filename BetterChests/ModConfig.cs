namespace StardewMods.BetterChests;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Storages;

/// <summary>Mod config data for Better Chests.</summary>
internal sealed class ModConfig
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

    /// <summary>Gets or sets a value indicating whether experimental features will be enabled.</summary>
    public bool Experimental { get; set; }

    /// <summary>Gets or sets the symbol used to denote context tags in searches.</summary>
    public char SearchTagSymbol { get; set; } = '#';

    /// <summary>Gets or sets the symbol used to denote negative searches.</summary>
    public char SearchNegationSymbol { get; set; } = '!';

    /// <summary>Gets or sets the color of locked slots.</summary>
    public string SlotLockColor { get; set; } = "Red";

    /// <summary>Gets or sets a value indicating whether the slot lock button needs to be held down.</summary>
    public bool SlotLockHold { get; set; } = true;
}