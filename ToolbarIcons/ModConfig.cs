namespace StardewMods.ToolbarIcons;

using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Mod config data for Toolbar Icons.</summary>
internal sealed class ModConfig
{
    /// <summary>Gets or sets a value containing the toolbar icons.</summary>
    public List<ToolbarIcon> Icons { get; set; } = [];

    /// <summary>Gets or sets the size that icons will be scaled to.</summary>
    public float Scale { get; set; } = 2;
}