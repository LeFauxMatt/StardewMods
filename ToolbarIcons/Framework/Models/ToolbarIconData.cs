namespace StardewMods.ToolbarIcons.Framework.Models;

using StardewMods.ToolbarIcons.Framework.Enums;

/// <summary>Data model for Toolbar Icons integration.</summary>
internal sealed class ToolbarIconData
{
    /// <summary>Gets or sets the unique id for the mod integration.</summary>
    public string ModId { get; set; } = string.Empty;

    /// <summary>Gets or sets the integration type.</summary>
    public IntegrationType Type { get; set; }

    /// <summary>Gets or sets additional data depending on the integration type.</summary>
    public string ExtraData { get; set; } = string.Empty;

    /// <summary>Gets or sets the hover text.</summary>
    public string HoverText { get; set; } = string.Empty;

    /// <summary>Gets or sets the path to the icon texture.</summary>
    public string Texture { get; set; } = string.Empty;

    /// <summary>Gets or sets the index of the icon.</summary>
    public int Index { get; set; }
}