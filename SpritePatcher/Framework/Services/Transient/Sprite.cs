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
    private readonly Dictionary<SpriteKey, ISpriteSheet> cachedTextures = [];
    private readonly CodeManager codeManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly ILog log;
    private readonly ISpriteSheetManager spriteSheetManager;
    private readonly HashSet<SpriteKey> disabledTextures = [];
    private readonly object source;

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
        if (!this.TryGetTexture(
            texture,
            new SpriteKey(target.BaseName, sourceRectangle.GetValueOrDefault(), drawMethod),
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
            new SpriteKey(texture.Name, sourceRectangle.GetValueOrDefault(), drawMethod),
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
        var x = destinationRectangle.X - (int)(managedTexture.Offset.X * Game1.pixelZoom);
        var y = destinationRectangle.Y - (int)(managedTexture.Offset.Y * Game1.pixelZoom);
        var width = (int)(destinationRectangle.Width
            + (((managedTexture.SourceRectangle.Width / managedTexture.Scale) - valueOrDefault.Width)
                * Game1.pixelZoom));

        var height = (int)(destinationRectangle.Height
            + (((managedTexture.SourceRectangle.Height / managedTexture.Scale) - valueOrDefault.Height)
                * Game1.pixelZoom));

        spriteBatch.Draw(
            managedTexture.Texture,
            new Rectangle(x, y, width, height),
            managedTexture.SourceRectangle,
            color,
            rotation,
            origin * managedTexture.Scale,
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

    private bool TryGetTexture(Texture2D baseTexture, SpriteKey key, [NotNullWhen(true)] out ISpriteSheet? texture)
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

        // Apply patches and generate texture
        var patchesToApply = new List<IPatchModel>();
        foreach (var patch in patches)
        {
            bool success;
            try
            {
                success = patch.Run(this);
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
            && this.spriteSheetManager.TryBuildSpriteSheet(key, baseTexture, patchesToApply, out texture))
        {
            this.cachedTextures[key] = texture;
            return true;
        }

        this.disabledTextures.Add(key);
        return false;
    }
}