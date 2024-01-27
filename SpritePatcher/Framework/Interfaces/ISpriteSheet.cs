namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>Represents a generated texture.</summary>
public interface ISpriteSheet
{
    /// <summary>Gets the generated texture.</summary>
    Texture2D Texture { get; }

    /// <summary>Gets the scaled factor for the generated texture.</summary>
    float Scale { get; }

    /// <summary>Gets the origin offset of the generated texture.</summary>
    Vector2 Offset { get; }

    /// <summary>Gets the source rectangle of the generated texture.</summary>
    Rectangle SourceRectangle { get; }

    /// <summary>Gets or sets a value indicating whether the sprite sheet was accessed.</summary>
    bool WasAccessed { get; set; }
}