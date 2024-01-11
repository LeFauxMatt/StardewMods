namespace StardewMods.ItemIconOverlays.Framework.Interfaces;

using Microsoft.Xna.Framework;

/// <summary>Data for an icon overlay.</summary>
public interface IIconData
{
    /// <summary>Gets the path the item attribute.</summary>
    string Path { get; }

    /// <summary>Gets the value to match against the item attribute.</summary>
    string Value { get; }

    /// <summary>Gets the path to the texture.</summary>
    string Texture { get; }

    /// <summary>Gets the source rectangle of the icon.</summary>
    Rectangle SourceRect { get; }
}