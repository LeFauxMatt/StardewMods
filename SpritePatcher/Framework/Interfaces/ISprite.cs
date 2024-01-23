namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents an object being managed by the mod.</summary>
public interface ISprite
{
    /// <summary>Gets the entity associated with this sprite.</summary>
    IHaveModData Entity { get; }

    /// <summary>Gets a weak reference to this sprite.</summary>
    WeakReference<ISprite> Self { get; }

    /// <summary>Submit a sprite for drawing in the current batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the sprite.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="position">The drawing location on screen.</param>
    /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="scale">A scaling of this sprite.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
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

    /// <summary>Submit a sprite for drawing in the current batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch used to draw the sprite.</param>
    /// <param name="texture">A texture.</param>
    /// <param name="destinationRectangle">The drawing bounds on screen.</param>
    /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
    /// <param name="color">A color mask.</param>
    /// <param name="rotation">A rotation of this sprite.</param>
    /// <param name="origin">Center of the rotation. 0,0 by default.</param>
    /// <param name="effects">Modificators for drawing. Can be combined.</param>
    /// <param name="layerDepth">A depth of the layer of this sprite.</param>
    /// <param name="drawMethod">The method used for drawing the sprite.</param>
    public void Draw(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth,
        DrawMethod drawMethod);

    /// <summary>Clears all cached textures.</summary>
    public void ClearCache();

    /// <summary>Clears the cache for the specified textureName.</summary>
    /// <param name="targets">The name of the texture caches to be cleared.</param>
    public void ClearCache(IEnumerable<string> targets);
}