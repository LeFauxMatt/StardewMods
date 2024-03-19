namespace StardewMods.SpritePatcher.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Services.Transient;
using StardewValley.Extensions;

/// <inheritdoc cref="ISpriteSheetManager" />
internal sealed class SpriteSheetManager : BaseService, ISpriteSheetManager
{
    private readonly Dictionary<int, Color[]> cachedData = [];
    private readonly Dictionary<SpriteKey, ISpriteSheet> cachedSpriteSheets = [];
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="SpriteSheetManager" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public SpriteSheetManager(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventSubscriber.Subscribe<DayStartedEventArgs>(this.OnDayStarted);
        eventSubscriber.Subscribe<DayEndingEventArgs>(this.OnDayEnding);
    }

    /// <inheritdoc />
    public bool TryGetTexture(string path, [NotNullWhen(true)] out IRawTextureData? texture)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            texture = null;
            return false;
        }

        var assetName = this.gameContentHelper.ParseAssetName(path);
        if (this.baseTextures.TryGetValue(assetName.BaseName, out texture))
        {
            return true;
        }

        texture = new VanillaTexture(assetName.BaseName);
        this.baseTextures[assetName.BaseName] = texture;
        return true;
    }

    /// <inheritdoc />
    public bool TryBuildSpriteSheet(
        ISprite sprite,
        SpriteKey spriteKey,
        Texture2D texture,
        IList<ISpritePatch> patches,
        [NotNullWhen(true)] out ISpriteSheet? spriteSheet)
    {
        spriteSheet = null;
        if (!patches.Any())
        {
            return false;
        }

        // Get base sprite sheet
        if (!this.cachedSpriteSheets.TryGetValue(spriteKey, out spriteSheet))
        {
            if (!this.TryGetTexture(texture.Name, out var baseTexture))
            {
                return false;
            }

            spriteSheet = new SpriteSheet(spriteKey, texture, baseTexture);
        }

        foreach (var patch in patches)
        {
            try
            {
                patch.Run(sprite, spriteSheet);
            }
            catch (InapplicableContextException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                this.Log.WarnOnce("Patch {0} failed to run.\nError: {1}", patch.Id, e.Message);
                continue;
            }

            // Check if the texture is already cached
            if (this.cachedSpriteSheets.TryGetValue(spriteSheet.GetCurrentId(), out var cachedData))
            {
                spriteSheet.SetData(cachedData);
                continue;
            }

            // Build the texture and store in cache
        }

        // Return from cache if available
        var cacheKey = new SpriteSheetKey(spriteKey, patches.Select(layer => layer.GetCurrentKey()).ToList());
        if (this.cachedSpriteSheets.TryGetValue(cacheKey, out spriteSheet))
        {
            return true;
        }

        // Calculate the expanded texture based on the layers offset, area, and scale
        SpriteSheetManager.InitializeTextureDimensions(
            spriteKey,
            patches,
            out var origin,
            out var scaledWidth,
            out var scaledHeight);

        // Expanded the texture to match the highest resolution layer
        var scale = 1 / patches.Min(layer => layer.Scale);
        scaledWidth *= (int)scale;
        scaledHeight *= (int)scale;

        // Expand the texture for any animates frames
        var animatedLayers = patches.Where(layer => layer.Animate != Animate.None && layer.Frames > 1).ToList();

        // Calculate the total duration in ticks based on any animated layers
        var totalDuration = !animatedLayers.Any()
            ? 1
            : animatedLayers.Select(layer => layer.Frames * (int)layer.Animate).Aggregate(1, SpriteSheetManager.Lcm);

        // Find the layer with the shortest duration per frame
        var fastestAnimation = !animatedLayers.Any()
            ? 1
            : patches.Where(layer => layer.Animate != Animate.None).Min(layer => (int)layer.Animate);

        var totalFrames = !animatedLayers.Any() ? 1 : totalDuration / fastestAnimation;

        var frameWidth = scaledWidth;
        scaledWidth *= totalFrames;

        var textureData = new Color[scaledWidth * scaledHeight];
        this.CopyBaseTextureData(spriteKey, texture.Name, origin, scaledWidth, scaledHeight, scale, frameWidth, textureData);

        // Apply each layer
        foreach (var layer in patches)
        {
            this.ApplyLayer(
                layer,
                spriteKey,
                origin,
                scaledWidth,
                scaledHeight,
                scale,
                frameWidth,
                fastestAnimation,
                textureData);
        }

        spriteSheet = new SpriteSheet(
            spriteKey,
            texture,
            textureData,
            scaledWidth,
            scaledHeight,
            scale,
            origin,
            totalFrames,
            fastestAnimation);

        this.cachedSpriteSheets[cacheKey] = spriteSheet;
        return true;
    }

    private static int Gcf(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    private static int Lcm(int a, int b) => a * b / SpriteSheetManager.Gcf(a, b);

    private static void InitializeTextureDimensions(
        SpriteKey key,
        IList<ISpritePatch> layers,
        out Vector2 origin,
        out int scaledWidth,
        out int scaledHeight)
    {
        // Assign originX based on layer with the largest offset
        var originX = (int)Math.Max(0, layers.Max(layer => key.SourceRectangle.X - layer.SourceArea.X - layer.Offset.X));

        // Assign originY based on layer with the largest offset
        var originY = (int)Math.Max(0, layers.Max(layer => key.SourceRectangle.Y - layer.SourceArea.Y - layer.Offset.Y));

        origin = new Vector2(originX, originY);

        scaledWidth = Math.Max(
            originX + key.SourceRectangle.Width,
            layers.Max(
                layer => (int)(originX
                    + layer.Offset.X
                    + layer.SourceArea.X
                    - key.SourceRectangle.X
                    + (layer.Area.Width * layer.Scale / layer.Frames))));

        scaledHeight = Math.Max(
            originY + key.SourceRectangle.Height,
            layers.Max(
                layer => (int)(originY
                    + layer.Offset.Y
                    + layer.SourceArea.Y
                    - key.SourceRectangle.Y
                    + (layer.Area.Height * layer.Scale))));
    }

    private static void ApplyTintIfApplicable(ISpritePatch layer, Color[] layerData)
    {
        if (layer.Texture == null || layer.Tint == null)
        {
            return;
        }

        Utility.RGBtoHSL(layer.Tint.Value.R, layer.Tint.Value.G, layer.Tint.Value.B, out var h, out var s, out var l);
        Utility.HSLtoRGB(h, s * 2f, l * 2f, out var r, out var g, out var b);
        var boostedTint = new Color(r, g, b);
        for (var index = 0; index < layer.Area.Width * layer.Area.Height; ++index)
        {
            if (layerData[index].A <= 0)
            {
                continue;
            }

            var baseTint = Utility.MultiplyColor(layerData[index], layer.Tint.Value);
            layerData[index] = Color.Lerp(baseTint, boostedTint, 0.3f);
        }
    }

    private void OnDayEnding(DayEndingEventArgs e)
    {
        var count = this.cachedSpriteSheets.Count;
        this.cachedSpriteSheets.RemoveWhere(kvp => !kvp.Value.WasAccessed);
        var removed = count - this.cachedSpriteSheets.Count;
        if (removed > 0)
        {
            this.Log.Trace("Removed {0} cached textures.", removed);
        }
    }

    private void OnDayStarted(DayStartedEventArgs e)
    {
        foreach (var sprite in this.cachedSpriteSheets.Values)
        {
            sprite.WasAccessed = false;
        }
    }

    private void CopyBaseTextureData(
        SpriteKey key,
        string textureName,
        Vector2 origin,
        int scaledWidth,
        int scaledHeight,
        float scale,
        int frameWidth,
        Color[] textureData)
    {
        // Load base texture from cache if available
        if (!this.baseTextures.TryGetValue(textureName, out var baseTexture))
        {
            baseTexture = new VanillaTexture(textureName);
            this.baseTextures[textureName] = baseTexture;
        }

        // Copy base texture data into each frame of the expanded texture
        var baseTextureData = new Span<Color>(baseTexture.Data);

        for (var x = 0; x < scaledWidth; ++x)
        {
            for (var y = 0; y < scaledHeight; ++y)
            {
                var targetIndex = (y * scaledWidth) + x;
                var sourceX = (x % frameWidth / scale) - origin.X;
                var sourceY = (y / scale) - origin.Y;

                var sourceIndex = ((int)(sourceY + key.SourceRectangle.Y) * baseTexture.Width) + (int)(sourceX + key.SourceRectangle.X);

                // Ensure sourceX and sourceY are within the sourceData
                if (sourceX < 0 || sourceX >= key.SourceRectangle.Width || sourceY < 0 || sourceY >= key.SourceRectangle.Height)
                {
                    continue;
                }

                textureData[targetIndex] = baseTextureData[sourceIndex];
            }
        }
    }

    private void ApplyLayer(
        ISpritePatch layer,
        SpriteKey key,
        Vector2 origin,
        int scaledWidth,
        int scaledHeight,
        float scale,
        int frameWidth,
        int fastestAnimation,
        Color[] textureData)
    {
        if (layer.Texture == null)
        {
            return;
        }

        var layerId = layer.GetCurrentKey();
        if (!this.cachedData.TryGetValue(layerId, out var layerData))
        {
            layerData = (Color[])layer.Texture.Data.Clone();
            SpriteSheetManager.ApplyTintIfApplicable(layer, layerData);
            this.cachedData[layerId] = layerData;
        }

        var scaleFactor = 1f / (scale * layer.Scale);
        var offsetX = (int)(scale * (layer.SourceArea.X - key.SourceRectangle.X + (int)origin.X + (int)layer.Offset.X));
        var offsetY = (int)(scale * (layer.SourceArea.Y - key.SourceRectangle.Y + (int)origin.Y + (int)layer.Offset.Y));
        var endX = Math.Min(scaledWidth, offsetX + layer.Area.Width);
        var endY = Math.Min(scaledHeight, offsetY + layer.Area.Height);

        for (var x = offsetX; x < endX; ++x)
        {
            for (var y = offsetY; y < endY; ++y)
            {
                var targetIndex = (y * scaledWidth) + x;
                var frame = layer.Animate == Animate.None
                    ? 0
                    : x / frameWidth * fastestAnimation / (int)layer.Animate % layer.Frames;

                // Map layer index to target index
                var patchX = layer.Area.X
                    + ((x - offsetX) % (frameWidth * layer.Frames) * scaleFactor)
                    + (frame / layer.Frames * layer.Area.Width);

                var patchY = layer.Area.Y + ((y - offsetY) * scaleFactor);

                var sourceIndex = (int)patchX + ((int)patchY * layer.Texture.Width);
                if (sourceIndex >= 0
                    && sourceIndex < layerData.Length
                    && (layer.PatchMode == PatchMode.Replace || layerData[sourceIndex].A > 0))
                {
                    textureData[targetIndex] = layer.Alpha >= 0.99f
                        ? layerData[sourceIndex]
                        : Color.Lerp(textureData[targetIndex], layerData[sourceIndex], layer.Alpha);
                }
            }
        }
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        foreach (var assetName in e.NamesWithoutLocale)
        {
            if (this.baseTextures.TryGetValue(assetName.BaseName, out var baseTexture))
            {
                ((VanillaTexture)baseTexture).ClearCache();
            }
        }
    }
}