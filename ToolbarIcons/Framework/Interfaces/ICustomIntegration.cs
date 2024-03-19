namespace StardewMods.ToolbarIcons.Framework.Interfaces;

/// <summary>Represents an integration which is directly supported by this mod.</summary>
internal interface ICustomIntegration
{
    /// <summary>Gets the index of the icon on the sprite sheet.</summary>
    int Index { get; }

    /// <summary>Gets the text used when hovering over the toolbar icon.</summary>
    string HoverText { get; }
}