namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class ManagedObject : IManagedObject
{
    private static readonly Dictionary<Type, IDictionary<string, (INetSerializable NetField, EventInfo? EventInfo)>>
        CachedEvents = [];

    private readonly Dictionary<string, (INetSerializable Target, Delegate Handler)> subscribedEvents =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<TextureKey, Texture2D> cachedTextures = [];
    private readonly CodeManager codeManager;
    private readonly TextureBuilder textureBuilder;
    private readonly HashSet<TextureKey> disabledTextures = [];
    private readonly Dictionary<string, HashSet<TextureKey>> fieldTargets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Type type;

    /// <summary>Initializes a new instance of the <see cref="ManagedObject" /> class.</summary>
    /// <param name="entity">The entity being managed.</param>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="textureBuilder">Dependency used for generating textures from patches.</param>
    public ManagedObject(IHaveModData entity, CodeManager codeManager, TextureBuilder textureBuilder)
    {
        this.type = entity.GetType();
        this.Entity = entity;
        this.codeManager = codeManager;
        this.textureBuilder = textureBuilder;
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
        if (!this.TryGetTexture(
            texture,
            new TextureKey(texture.Name, sourceRectangle, drawMethod, scale),
            out var newTexture))
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
            new TextureKey(texture.Name, sourceRectangle, drawMethod, 1f),
            out var newTexture))
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

        spriteBatch.Draw(
            newTexture,
            destinationRectangle,
            new Rectangle(0, 0, newTexture.Width, newTexture.Height),
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

    private bool TryGetTexture(Texture2D baseTexture, TextureKey key, [NotNullWhen(true)] out Texture2D? texture)
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
        if (!this.codeManager.TryGet(key.Target, out var allPatches))
        {
            // Prevent future attempts to generate this texture
            this.disabledTextures.Add(key);
            return false;
        }

        // Determine which textures apply
        var conditionalPatches = allPatches
            .Where(
                patch => patch.DrawMethods.Contains(key.DrawMethod)
                    && (patch.SourceArea is null
                        || key.Area is null
                        || patch.SourceArea.Value.Intersects(key.Area.Value)))
            .ToList();

        var patchesToApply = conditionalPatches.Where(patch => patch.Run(this)).ToList();
        if (patchesToApply.Any() && this.textureBuilder.TryBuildTexture(key, baseTexture, patchesToApply, out texture))
        {
            this.cachedTextures[key] = texture;
            return true;
        }

        this.disabledTextures.Add(key);
        return false;
    }
}