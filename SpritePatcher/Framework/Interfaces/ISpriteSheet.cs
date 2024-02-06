namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>Represents a generated texture.</summary>
public interface ISpriteSheet : IRawTextureData
{
    /// <summary>Gets the generated texture.</summary>
    Texture2D Texture { get; }

    /// <summary>Gets the scaled factor for the generated texture.</summary>
    float Scale { get; }

    /// <summary>Gets the origin offset of the generated texture.</summary>
    Vector2 Offset { get; }

    /// <summary>Gets the source rectangle of the generated texture.</summary>
    Rectangle SourceArea { get; }

    /// <summary>Gets or sets the color of the draw method.</summary>
    Color Color { get; set; }

    /// <summary>Gets or sets the rotation of the draw method.</summary>
    float Rotation { get; set; }

    /// <summary>Gets or sets the effects of the draw method.</summary>
    SpriteEffects Effects { get; set; }

    /// <summary>Gets or sets a value indicating whether the sprite sheet was accessed.</summary>
    bool WasAccessed { get; set; }

    /// <summary>Retrieves the current ID.</summary>
    /// <returns>The current ID as an integer.</returns>
    int GetCurrentId();

    /// <summary>Sets the data for the given color array.</summary>
    /// <param name="data">The raw texture data to set.</param>
    void SetData(IRawTextureData data);
}