namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Represents an object being managed by the mod.</summary>
internal sealed partial class ManagedObject
{
    private readonly CachedTextures cachedTextures;
    private readonly IHaveModData entity;
    private readonly CodeManager codeManager;
    private readonly TextureBuilder textureBuilder;

    /// <summary>Initializes a new instance of the <see cref="ManagedObject" /> class.</summary>
    /// <param name="entity">The entity being managed.</param>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="textureBuilder">Dependency used for generating textures.</param>
    public ManagedObject(IHaveModData entity, CodeManager codeManager, TextureBuilder textureBuilder)
    {
        this.cachedTextures = new CachedTextures(this);
        this.entity = entity;
        this.codeManager = codeManager;
        this.textureBuilder = textureBuilder;
    }

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
        DrawMethod drawMethod)
    {
        if (!this.TryGetTexture(texture, sourceRectangle, drawMethod, out var newTexture))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            newTexture,
            position,
            new Rectangle(0, 0, newTexture.Width, newTexture.Height),
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth);
    }

    /// <summary>Clears the cache for the specified textureName.</summary>
    /// <param name="targets">The name of the texture caches to be cleared.</param>
    public void ClearCache(IEnumerable<string> targets) => this.cachedTextures.ClearCache(targets);

    private bool TryGetTexture(
        Texture2D baseTexture,
        Rectangle? sourceRectangle,
        DrawMethod drawMethod,
        [NotNullWhen(true)] out Texture2D? texture)
    {
        // Attempt to retrieve texture from cache
        if (this.cachedTextures.TryGet(baseTexture.Name, sourceRectangle, drawMethod, out texture))
        {
            return texture != null;
        }

        // Check if any patches may apply to this texture
        if (!this.codeManager.TryGet(baseTexture.Name, out var allPatches))
        {
            this.cachedTextures.Disable(baseTexture.Name, sourceRectangle, drawMethod);
            texture = null;
            return false;
        }

        // Determine which textures apply
        var patches = allPatches.Where(patch => patch.DrawMethods.Contains(drawMethod));
        patches = patches.Where(patch => patch.SourceArea is null || patch.SourceArea == sourceRectangle);
        patches = patches.Where(patch => patch.Run(this.entity));
        var patchesToApply = patches.OfType<ITextureModel>().ToList();

        if (!patchesToApply.Any())
        {
            this.cachedTextures.Disable(baseTexture.Name, sourceRectangle, drawMethod);
            texture = null;
            return false;
        }

        // Attempt to build the texture
        var sourceRect = sourceRectangle ?? new Rectangle(0, 0, baseTexture.Width, baseTexture.Height);
        if (!this.textureBuilder.TryBuildTexture(baseTexture, sourceRect, patchesToApply, out texture))
        {
            this.cachedTextures.Disable(baseTexture.Name, sourceRectangle, drawMethod);
            return false;
        }

        // Cache the texture
        this.cachedTextures.AddOrUpdate(baseTexture.Name, sourceRectangle, drawMethod, texture);
        return true;
    }
}