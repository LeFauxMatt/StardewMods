namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley.Extensions;

/// <summary>Represents an object being managed by the mod.</summary>
internal sealed class ManagedObject
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
    private readonly IHaveModData entity;
    private readonly Type type;

    /// <summary>Initializes a new instance of the <see cref="ManagedObject" /> class.</summary>
    /// <param name="entity">The entity being managed.</param>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="textureBuilder">Dependency used for generating textures from patches.</param>
    public ManagedObject(IHaveModData entity, CodeManager codeManager, TextureBuilder textureBuilder)
    {
        this.type = entity.GetType();
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
        if (!this.TryGetTexture(texture, new TextureKey(texture.Name, sourceRectangle, drawMethod), out var newTexture))
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
                    && (patch.SourceArea is null || patch.SourceArea == key.Area))
            .ToList();

        // Add any net fields that will be used to invalidate the cache for this specific texture
        this.UnsubscribeFromFieldEvents(key);
        var netFields = conditionalPatches.SelectMany(patch => patch.NetFields).Distinct().ToList();
        foreach (var netField in netFields)
        {
            this.SubscribeToFieldEvent(netField, key);
        }

        var patchesToApply = conditionalPatches.Where(patch => patch.Run(this.entity)).ToList();
        if (this.textureBuilder.TryBuildTexture(
            patchesToApply,
            baseTexture,
            key.Area ?? new Rectangle(0, 0, baseTexture.Width, baseTexture.Height),
            out texture))
        {
            this.cachedTextures[key] = texture;
            return true;
        }

        this.disabledTextures.Add(key);
        return false;
    }

    private void SubscribeToFieldEvent(string name, TextureKey key)
    {
        if (!ManagedObject.CachedEvents.TryGetValue(this.type, out var objectEvents))
        {
            objectEvents = new Dictionary<string, (INetSerializable NetField, EventInfo? EventInfo)>();
            ManagedObject.CachedEvents[this.type] = objectEvents;
        }

        if (this.entity is not INetObject<NetFields> obj)
        {
            return;
        }

        // Create targets for field if they dont' exist
        var fieldName = obj.NetFields.Name + ": " + name;
        if (!this.fieldTargets.TryGetValue(fieldName, out var targets))
        {
            targets = new HashSet<TextureKey>();
            this.fieldTargets[fieldName] = targets;
        }

        // Check if already subscribed to event and add target
        if (this.subscribedEvents.ContainsKey(fieldName))
        {
            targets.Add(key);
            return;
        }

        // Check if cached event info exists
        if (!objectEvents.TryGetValue(fieldName, out var objectEvent))
        {
            foreach (var field in obj.NetFields.GetFields())
            {
                // Check if field name matches
                if (!field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var fieldType = field.GetType();
                var eventInfo = fieldType.GetEvent("fieldChangeVisibleEvent");
                objectEvents[fieldName] = (field, eventInfo);
                break;
            }
        }

        if (objectEvent.EventInfo?.EventHandlerType == null)
        {
            return;
        }

        // Create a delegate for the event
        EventHandler eventHandler = (_, _) => this.ClearCache(targets.Select(t => t.Target));

        objectEvent.EventInfo.AddEventHandler(objectEvent.NetField, eventHandler);
        this.subscribedEvents[fieldName] = (objectEvent.NetField, eventHandler);
        targets.Add(key);
    }

    private void UnsubscribeFromFieldEvents(TextureKey key)
    {
        if (!ManagedObject.CachedEvents.TryGetValue(this.type, out var objectEvents))
        {
            return;
        }

        foreach (var (fieldName, (netField, eventInfo)) in objectEvents)
        {
            if (!this.fieldTargets.TryGetValue(fieldName, out var targets))
            {
                continue;
            }

            // Remove this particular target
            targets.Remove(key);
            if (targets.Any())
            {
                continue;
            }

            // If no targets remain, then unsubscribe from the actual event
            if (eventInfo?.EventHandlerType == null
                || !this.subscribedEvents.TryGetValue(fieldName, out var subscribedEvent))
            {
                continue;
            }

            eventInfo.RemoveEventHandler(netField, subscribedEvent.Handler);
            this.subscribedEvents.Remove(fieldName);
        }
    }
}