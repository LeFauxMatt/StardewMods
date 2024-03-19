namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class Sprite : ISprite
{
    private readonly Dictionary<SpriteKey, WeakReference<ISpriteSheet>> cached = [];
    private readonly CodeManager codeManager;
    private readonly HashSet<SpriteKey> disabled = [];
    private readonly IGameContentHelper gameContentHelper;
    private readonly ILog log;
    private readonly object source;
    private readonly ISpriteSheetManager spriteSheetManager;

    /// <summary>Initializes a new instance of the <see cref="Sprite" /> class.</summary>
    /// <param name="source">The entity being managed.</param>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="spriteSheetManager">Dependency used for managing textures.</param>
    public Sprite(
        object source,
        CodeManager codeManager,
        IGameContentHelper gameContentHelper,
        ILog log,
        ISpriteSheetManager spriteSheetManager)
    {
        this.source = source;
        this.Entity = Sprite.GetEntity(source);
        this.Self = new WeakReference<ISprite>(this);
        this.codeManager = codeManager;
        this.gameContentHelper = gameContentHelper;
        this.log = log;
        this.spriteSheetManager = spriteSheetManager;
    }

    /// <inheritdoc />
    public IHaveModData Entity { get; }

    /// <inheritdoc />
    public WeakReference<ISprite> Self { get; }

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
        var key = new SpriteKey(target, sourceRectangle.GetValueOrDefault(), color, drawMethod);
        if (!this.TryGetSpriteSheet(key, texture, out var spriteSheet))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            spriteSheet.Texture,
            position - (scale * spriteSheet.Offset),
            spriteSheet.SourceArea,
            color,
            rotation * spriteSheet.Rotation,
            origin * spriteSheet.Scale,
            scale / spriteSheet.Scale,
            effects | spriteSheet.Effects,
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
        var target = this.gameContentHelper.ParseAssetName(texture.Name);
        var key = new SpriteKey(target, sourceRectangle.GetValueOrDefault(), color, drawMethod);
        if (!this.TryGetSpriteSheet(key, texture, out var spriteSheet))
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

        var x = destinationRectangle.X - (int)(spriteSheet.Offset.X * Game1.pixelZoom);
        var y = destinationRectangle.Y - (int)(spriteSheet.Offset.Y * Game1.pixelZoom);
        var width = (int)(destinationRectangle.Width
            + (((spriteSheet.SourceArea.Width / spriteSheet.Scale) - key.SourceRectangle.Width)
                * Game1.pixelZoom));

        var height = (int)(destinationRectangle.Height
            + (((spriteSheet.SourceArea.Height / spriteSheet.Scale) - key.SourceRectangle.Height)
                * Game1.pixelZoom));

        spriteBatch.Draw(
            spriteSheet.Texture,
            new Rectangle(x, y, width, height),
            spriteSheet.SourceArea,
            color,
            rotation * spriteSheet.Rotation,
            origin * spriteSheet.Scale,
            effects | spriteSheet.Effects,
            layerDepth);
    }

    /// <inheritdoc />
    public void Draw(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth,
        DrawMethod drawMethod)
    {
        var target = this.gameContentHelper.ParseAssetName(texture.Name);
        var key = new SpriteKey(target, sourceRectangle.GetValueOrDefault(), color, drawMethod);
        if (!this.TryGetSpriteSheet(key, texture, out var spriteSheet))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            spriteSheet.Texture,
            position - (scale * spriteSheet.Offset),
            spriteSheet.SourceArea,
            color,
            rotation * spriteSheet.Rotation,
            origin * spriteSheet.Scale,
            scale / spriteSheet.Scale,
            effects | spriteSheet.Effects,
            layerDepth);
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        this.cached.Clear();
        this.disabled.Clear();
    }

    /// <inheritdoc />
    public void ClearCache(IEnumerable<string> targets)
    {
        foreach (var target in targets)
        {
            this.cached.RemoveWhere(
                kvp => kvp.Key.Target.Equals(target, StringComparison.OrdinalIgnoreCase));

            this.disabled.RemoveWhere(key => key.Target.Equals(target, StringComparison.OrdinalIgnoreCase));
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
            AnimatedSprite sprite => sprite.Owner,
            _ => throw new NotSupportedException($"Cannot manage {source.GetType().FullName}"),
        };

    private bool TryGetSpriteSheet(
        SpriteKey key,
        Texture2D baseTexture,
        [NotNullWhen(true)] out ISpriteSheet? spriteSheet)
    {
        spriteSheet = null;

        // Check if patching is disabled for this key
        if (this.disabled.Contains(key))
        {
            return false;
        }

        // Return texture from cache if it exists
        if (this.cached.TryGetValue(key, out var cachedSpriteSheet)
            && cachedSpriteSheet.TryGetTarget(out spriteSheet))
        {
            spriteSheet.WasAccessed = true;
            return true;
        }

        // Check if any patches should apply to this texture
        if (!this.codeManager.TryGet(key, out var patches))
        {
            // Prevent future attempts to generate this texture
            this.disabled.Add(key);
            return false;
        }

        // Apply patches and generate texture
        var color = Color.White;
        var rotation = 1f;
        var effects = SpriteEffects.None;
        if (this.spriteSheetManager.TryBuildSpriteSheet(this, key, baseTexture, patches, out spriteSheet))
        {
            spriteSheet.Color = color;
            spriteSheet.Rotation = rotation;
            spriteSheet.Effects = effects;
            this.cached[key] = new WeakReference<ISpriteSheet>(spriteSheet);
            return true;
        }

        this.disabled.Add(key);
        return false;

        var patchesToApply = new List<ISpritePatch>();
        foreach (var patch in patches)
        {
            bool success;
            try
            {
                success = patch.Run(this);
                color = patch.Color.GetValueOrDefault(color);
                rotation = patch.Rotation.GetValueOrDefault(rotation);
                effects = patch.Effects.GetValueOrDefault(effects);
            }
            catch (Exception e)
            {
                this.log.WarnOnce("Patch {0} failed to run.\nError: {1}", patch.Id, e.Message);
                continue;
            }

            if (success)
            {
                patchesToApply.Add(patch);
            }
        }

        if (patchesToApply.Any()
            && this.spriteSheetManager.TryBuildSpriteSheet(key, baseTexture, patchesToApply, out spriteSheet))
        {
            spriteSheet.Color = color;
            spriteSheet.Rotation = rotation;
            spriteSheet.Effects = effects;
            this.cached[key] = new WeakReference<ISpriteSheet>(spriteSheet);
            return true;
        }
    }
}