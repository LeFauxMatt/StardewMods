namespace StardewMods.ExpandedStorage.Framework;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Models;
using StardewValley.Objects;

/// <summary>
///     Extension methods for Expanded Storage.
/// </summary>
internal static class Extensions
{
#nullable disable
    private static IDictionary<string, CachedStorage> StorageCache;
#nullable enable

    /// <summary>
    ///     Draws an Expanded Storage chest.
    /// </summary>
    /// <param name="storage">The storage to draw.</param>
    /// <param name="obj">The source object.</param>
    /// <param name="currentLidFrame">The current animation frame.</param>
    /// <param name="b">The sprite batch to draw to.</param>
    /// <param name="pos">The position to draw the storage at.</param>
    /// <param name="color">The color to draw the chest.</param>
    /// <param name="origin">The origin of the texture.</param>
    /// <param name="alpha">The alpha level to draw.</param>
    /// <param name="scaleSize">The scale size.</param>
    /// <param name="layerDepth">The layer depth.</param>
    public static void Draw(
        this ICustomStorage storage,
        SObject obj,
        int currentLidFrame,
        SpriteBatch b,
        Vector2 pos,
        Color? color = null,
        Vector2? origin = null,
        float alpha = 1f,
        float scaleSize = 1f,
        float layerDepth = 0.0001f)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        var startingLidFrame = (obj as Chest)?.startingLidFrame.Value ?? 0;
        var lastLidFrame = (obj as Chest)?.getLastLidFrame() ?? 1;
        var colored = storage.PlayerColor && (obj as Chest)?.playerChoiceColor.Value.Equals(Color.Black) == false;
        var tint = (obj as Chest)?.Tint ?? Color.White;
        var frame = new Rectangle(
            Math.Min(startingLidFrame + lastLidFrame - 1, Math.Max(0, currentLidFrame - startingLidFrame))
          * storage.Width,
            colored ? storage.Height : 0,
            storage.Width,
            storage.Height);

        // Draw Base Layer
        b.Draw(
            storageCache.Texture,
            pos + (obj.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            (color ?? tint) * alpha,
            0f,
            origin ?? Vector2.Zero,
            Game1.pixelZoom * scaleSize,
            SpriteEffects.None,
            layerDepth);
        if (frame.Y == 0)
        {
            return;
        }

        frame.Y = storage.Height * 2;

        // Draw Top Layer
        b.Draw(
            storageCache.Texture,
            pos + (obj.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            tint * alpha,
            0f,
            origin ?? Vector2.Zero,
            Game1.pixelZoom * scaleSize,
            SpriteEffects.None,
            layerDepth + 1E-05f);
    }

    /// <summary>
    ///     Gets the frame count of a custom storage's lid opening animation .
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the frame count.</returns>
    public static int GetFrames(this ICustomStorage storage)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        return storageCache.Frames;
    }

    /// <summary>
    ///     Gets the scale multiplier of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the scale multiplier.</returns>
    public static float GetScaleMultiplier(this ICustomStorage storage)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        return storageCache.ScaleMultiplier;
    }

    /// <summary>
    ///     Gets the tile depth of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile depth.</returns>
    public static int GetTileDepth(this ICustomStorage storage)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        return storageCache.TileDepth;
    }

    /// <summary>
    ///     Gets the tile height of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile height.</returns>
    public static int GetTileHeight(this ICustomStorage storage)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        return storageCache.TileHeight;
    }

    /// <summary>
    ///     Gets the tile width of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile width.</returns>
    public static int GetTileWidth(this ICustomStorage storage)
    {
        if (!Extensions.StorageCache.TryGetValue(storage.Image, out var storageCache))
        {
            storageCache = new(storage);
        }

        return storageCache.TileWidth;
    }

    /// <summary>
    ///     Initialized <see cref="Extensions" />.
    /// </summary>
    /// <param name="storageCache">Cached storage textures and attributes.</param>
    public static void Init(IDictionary<string, CachedStorage> storageCache)
    {
        Extensions.StorageCache = storageCache;
    }
}