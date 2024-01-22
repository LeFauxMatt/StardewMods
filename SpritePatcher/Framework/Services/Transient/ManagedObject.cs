namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class ManagedObject : IManagedObject
{
    private readonly Dictionary<TextureKey, IManagedTexture> cachedTextures = [];
    private readonly CodeManager codeManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly ITextureManager textureManager;
    private readonly HashSet<TextureKey> disabledTextures = [];
    private readonly object source;

    /// <summary>Initializes a new instance of the <see cref="ManagedObject" /> class.</summary>
    /// <param name="source">The entity being managed.</param>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="textureManager">Dependency used for managing textures.</param>
    public ManagedObject(
        object source,
        CodeManager codeManager,
        IGameContentHelper gameContentHelper,
        ITextureManager textureManager)
    {
        this.source = source;
        this.Entity = ManagedObject.GetEntity(source);
        this.codeManager = codeManager;
        this.gameContentHelper = gameContentHelper;
        this.textureManager = textureManager;
    }

    /// <inheritdoc />
    public IHaveModData Entity { get; }

    /// <inheritdoc />
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
        var target = this.gameContentHelper.ParseAssetName(texture.Name);
        if (!this.TryGetTexture(
            texture,
            new TextureKey(target.BaseName, sourceRectangle.GetValueOrDefault(), drawMethod),
            out var managedTexture))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            managedTexture.Texture,
            position - (scale * managedTexture.Offset),
            managedTexture.SourceRectangle,
            color,
            rotation,
            origin * managedTexture.Scale,
            scale / managedTexture.Scale,
            effects,
            layerDepth);
    }

    /// <inheritdoc />
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
        DrawMethod drawMethod)
    {
        if (!this.TryGetTexture(
            texture,
            new TextureKey(texture.Name, sourceRectangle.GetValueOrDefault(), drawMethod),
            out var managedTexture))
        {
            spriteBatch.Draw(
                texture,
                destinationRectangle,
                sourceRectangle,
                color,
                rotation,
                origin,
                effects,
                layerDepth);

            return;
        }

        var valueOrDefault = sourceRectangle.GetValueOrDefault();
        var width = (int)(destinationRectangle.Width
            + (((managedTexture.SourceRectangle.Width / managedTexture.Scale) - valueOrDefault.Width)
                * Game1.pixelZoom));

        var height = (int)(destinationRectangle.Height
            + (((managedTexture.SourceRectangle.Height / managedTexture.Scale) - valueOrDefault.Height)
                * Game1.pixelZoom));

        spriteBatch.Draw(
            managedTexture.Texture,
            new Rectangle(
                destinationRectangle.X - (int)(managedTexture.Offset.X * Game1.pixelZoom),
                destinationRectangle.Y - (int)(managedTexture.Offset.Y * Game1.pixelZoom),
                width,
                height),
            managedTexture.SourceRectangle,
            color,
            rotation,
            origin,
            effects,
            layerDepth);
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        this.cachedTextures.Clear();
        this.disabledTextures.Clear();
    }

    /// <inheritdoc />
    public void ClearCache(IEnumerable<string> targets)
    {
        foreach (var target in targets)
        {
            this.cachedTextures.RemoveWhere(kvp => kvp.Key.Target.Equals(target, StringComparison.OrdinalIgnoreCase));
            this.disabledTextures.RemoveWhere(key => key.Target.Equals(target, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static IHaveModData GetEntity(object source) =>
        source switch
        {
            IHaveModData entity => entity,
            Crop
            {
                Dirt:
                { } dirt,
            } => dirt,
            _ => throw new NotSupportedException($"Cannot manage {source.GetType().FullName}"),
        };

    private bool TryGetTexture(Texture2D baseTexture, TextureKey key, [NotNullWhen(true)] out IManagedTexture? texture)
    {
        texture = null;

        // Check if patching is disabled for this object
        if (this.disabledTextures.Contains(key))
        {
            return false;
        }

        // Return texture from cache if it exists
        if (this.cachedTextures.TryGetValue(key, out texture))
        {
            return true;
        }

        // Check if any patches should apply to this texture
        if (!this.codeManager.TryGet(key, out var patches))
        {
            // Prevent future attempts to generate this texture
            this.disabledTextures.Add(key);
            return false;
        }

        var patchesToApply = patches.Where(patch => patch.Run(this)).ToList();
        if (patchesToApply.Any() && this.textureManager.TryBuildTexture(key, baseTexture, patchesToApply, out texture))
        {
            this.cachedTextures[key] = texture;
            return true;
        }

        this.disabledTextures.Add(key);
        return false;
    }
}