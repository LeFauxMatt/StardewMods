namespace StardewMods.SpritePatcher.Framework.Services.Patches;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Base class for texture patches.</summary>
internal abstract class BasePatches : BaseService
{
#nullable disable
    private static BasePatches instance;
#nullable enable

    private readonly TextureBuilder textureBuilder;

    /// <summary>Initializes a new instance of the <see cref="BasePatches" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="textureBuilder">Dependency used for building the texture.</param>
    protected BasePatches(ILog log, IManifest manifest, TextureBuilder textureBuilder)
        : base(log, manifest)
    {
        BasePatches.instance = this;
        this.textureBuilder = textureBuilder;
    }

    /// <summary>Draws a texture using a SpriteBatch with optional modifications.</summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="position">The position at which to draw the texture.</param>
    /// <param name="sourceRectangle">The portion of the texture to draw (null to draw entire texture).</param>
    /// <param name="color">The color to apply to the texture.</param>
    /// <param name="rotation">The rotation angle, in radians.</param>
    /// <param name="origin">The origin of rotation and scaling.</param>
    /// <param name="scale">The scale factor.</param>
    /// <param name="effects">The SpriteEffects to apply.</param>
    /// <param name="layerDepth">The depth at which to draw the texture.</param>
    /// <param name="entity">The entity associated with the texture.</param>
    /// <param name="drawMethod">The draw method to use for the texture.</param>
    protected static void Draw(
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
        IHaveModData entity,
        DrawMethod drawMethod)
    {
        var sourceRect = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
        if (!BasePatches.instance.textureBuilder.TryGetTexture(
            entity,
            texture,
            sourceRect,
            drawMethod,
            out var newTexture))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            newTexture,
            position,
            new Rectangle(0, 0, sourceRect.Width, sourceRect.Height),
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth);
    }
}