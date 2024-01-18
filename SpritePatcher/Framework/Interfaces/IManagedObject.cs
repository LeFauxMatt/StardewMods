namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents an object being managed by the mod.</summary>
public interface IManagedObject
{
    /// <summary>Gets the entity associated with this managed object.</summary>
    IHaveModData Entity { get; }

    /// <summary>Draws a sprite on the screen using the specified parameters.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the sprite.</param>
    /// <param name="texture">The texture of the sprite.</param>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="sourceRectangle">The portion of the texture to draw. Null to draw the entire texture.</param>
    /// <param name="color">The color to tint the sprite.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="origin">The origin of the sprite, relative to its position.</param>
    /// <param name="scale">The scaling factor applied to the sprite.</param>
    /// <param name="effects">The SpriteEffects applied to the sprite.</param>
    /// <param name="layerDepth">The layer depth of the sprite.</param>
    /// <param name="drawMethod">The method used for drawing the sprite.</param>
    public void Draw(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth,
        DrawMethod drawMethod);

    /// <summary>Clears all cached textures.</summary>
    public void ClearCache();

    /// <summary>Clears the cache for the specified textureName.</summary>
    /// <param name="targets">The name of the texture caches to be cleared.</param>
    public void ClearCache(IEnumerable<string> targets);
}