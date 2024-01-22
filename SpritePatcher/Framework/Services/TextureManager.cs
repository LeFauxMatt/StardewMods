namespace StardewMods.SpritePatcher.Framework.Services;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Services.Transient;

/// <inheritdoc cref="StardewMods.SpritePatcher.Framework.Interfaces.ITextureManager" />
internal sealed class TextureManager : BaseService, ITextureManager
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly Dictionary<string, IRawTextureData> baseTextures = [];
    private readonly Dictionary<string, Color[]> cachedData = [];
    private readonly Dictionary<string, IManagedTexture> cachedTextures = [];

    /// <summary>Initializes a new instance of the <see cref="TextureManager" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public TextureManager(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
    }

    /// <inheritdoc />
    public bool TryGetTexture(string path, [NotNullWhen(true)] out IRawTextureData? texture)
    {
        var assetName = this.gameContentHelper.ParseAssetName(path);
        if (this.baseTextures.TryGetValue(assetName.BaseName, out texture))
        {
            return true;
        }

        texture = new BaseTexture(assetName.BaseName);
        this.baseTextures[assetName.BaseName] = texture;
        return true;
    }

    /// <inheritdoc />
    public bool TryBuildTexture(
        TextureKey key,
        Texture2D texture,
        List<IPatchModel> layers,
        [NotNullWhen(true)] out IManagedTexture? managedTexture)
    {
        managedTexture = null;
        if (!layers.Any())
        {
            return false;
        }

        // Calculate the expanded texture based on the layers offset, area, and scale
        TextureManager.InitializeTextureDimensions(
            key,
            layers,
            out var origin,
            out var scaledWidth,
            out var scaledHeight);

        // Expanded the texture to match the highest resolution layer
        var scale = 1 / layers.Min(layer => layer.Scale);
        scaledWidth *= (int)scale;
        scaledHeight *= (int)scale;

        // Expand the texture for any animates frames
        var animatedLayers = layers.Where(layer => layer.Animate != Animate.None && layer.Frames > 1).ToList();

        // Calculate the total duration in ticks based on any animated layers
        var totalDuration = !animatedLayers.Any()
            ? 1
            : animatedLayers.Select(layer => layer.Frames * (int)layer.Animate).Aggregate(1, TextureManager.Lcm);

        // Find the layer with the shortest duration per frame
        var fastestAnimation = !animatedLayers.Any()
            ? 1
            : layers.Where(layer => layer.Animate != Animate.None).Min(layer => (int)layer.Animate);

        var totalFrames = !animatedLayers.Any() ? 1 : totalDuration / fastestAnimation;

        var frameWidth = scaledWidth;
        scaledWidth *= totalFrames;

        // Return from cache if available
        var cacheKey = TextureManager.GetCachedTextureKey(layers, texture, origin, scaledWidth, scaledHeight);
        if (this.cachedTextures.TryGetValue(cacheKey, out managedTexture))
        {
            return true;
        }

        var textureData = new Color[scaledWidth * scaledHeight];
        if (layers.First().PatchMode != PatchMode.Replace)
        {
            this.CopyBaseTextureData(
                key,
                texture.Name,
                origin,
                scaledWidth,
                scaledHeight,
                scale,
                frameWidth,
                textureData);
        }

        // Apply each layer
        foreach (var layer in layers)
        {
            this.ApplyLayer(
                layer,
                key,
                origin,
                scaledWidth,
                scaledHeight,
                scale,
                frameWidth,
                fastestAnimation,
                textureData);
        }

        var generatedTexture = new Texture2D(texture.GraphicsDevice, scaledWidth, scaledHeight);
        generatedTexture.SetData(textureData);
        generatedTexture.Name = cacheKey;
        managedTexture = new ManagedTexture(generatedTexture, scale, origin, totalFrames, fastestAnimation);
        this.cachedTextures[cacheKey] = managedTexture;
        return true;
    }

    private static string GetCachedTextureKey(
        List<IPatchModel> layers,
        GraphicsResource baseTexture,
        Vector2 origin,
        int width,
        int height)
    {
        var sb = new StringBuilder();
        sb.Append(baseTexture.Name);
        sb.Append('_');
        sb.Append(new Rectangle((int)origin.X, (int)origin.Y, width, height).ToString());
        foreach (var layer in layers)
        {
            sb.Append('_');
            sb.Append(layer.GetCurrentId());
        }

        return sb.ToString();
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

    private static int Lcm(int a, int b) => a * b / TextureManager.Gcf(a, b);

    private static void InitializeTextureDimensions(
        TextureKey key,
        List<IPatchModel> layers,
        out Vector2 origin,
        out int scaledWidth,
        out int scaledHeight)
    {
        // Assign originX based on layer with the largest offset
        var originX = Math.Max(0, layers.Max(layer => key.Area.X - layer.SourceArea.X - layer.Offset.X));

        // Assign originY based on layer with the largest offset
        var originY = Math.Max(0, layers.Max(layer => key.Area.Y - layer.SourceArea.Y - layer.Offset.Y));

        origin = new Vector2(originX, originY);

        scaledWidth = Math.Max(
            key.Area.Width,
            layers.Max(
                layer => (int)(originX
                    + layer.Offset.X
                    + layer.SourceArea.X
                    - key.Area.X
                    + (layer.Area.Width * layer.Scale / layer.Frames))));

        scaledHeight = Math.Max(
            key.Area.Height,
            layers.Max(
                layer => (int)(originY
                    + layer.Offset.Y
                    + layer.SourceArea.Y
                    - key.Area.Y
                    + (layer.Area.Height * layer.Scale))));
    }

    private static void ApplyTintIfApplicable(IPatchModel layer, Color[] layerData)
    {
        if (layer.Texture == null || layer.Tint == null)
        {
            return;
        }

        var hsl = HslColor.FromColor(layer.Tint.Value);
        var boostedTint = new HslColor(hsl.H, 2f * hsl.S, 2f * hsl.L).ToRgbColor();
        for (var y = layer.Area.Y; y < layer.Area.Y + layer.Area.Height; ++y)
        {
            for (var x = layer.Area.X; x < layer.Area.X + layer.Area.Width; ++x)
            {
                var index = (y * layer.Texture.Width) + x;
                if (layerData[index].A <= 0)
                {
                    continue;
                }

                var baseTint = new Color(
                    layerData[index].R / 255f * layer.Tint.Value.R / 255f,
                    layerData[index].G / 255f * layer.Tint.Value.G / 255f,
                    layerData[index].B / 255f * layer.Tint.Value.B / 255f);

                layerData[index] = Color.Lerp(baseTint, boostedTint, 0.3f);
            }
        }
    }

    private void CopyBaseTextureData(
        TextureKey key,
        string textureName,
        Vector2 origin,
        int scaledWidth,
        int scaledHeight,
        float scale,
        int frameWidth,
        IList<Color> textureData)
    {
        // Load base texture from cache if available
        if (!this.baseTextures.TryGetValue(textureName, out var baseTexture))
        {
            baseTexture = new BaseTexture(textureName);
            this.baseTextures[textureName] = baseTexture;
        }

        // Copy base texture data into each frame of the expanded texture
        var baseTextureData = ((BaseTexture)baseTexture).GetData(key.Area);

        for (var y = 0; y < scaledHeight; ++y)
        {
            for (var x = 0; x < scaledWidth; ++x)
            {
                // Map source index to target index
                var sourceX = (x % frameWidth / scale) - origin.X;
                var sourceY = (y / scale) - origin.Y;

                // Ensure sourceX and sourceY are within the sourceData
                if (sourceX < 0 || sourceX >= key.Area.Width || sourceY < 0 || sourceY >= key.Area.Height)
                {
                    continue;
                }

                var sourceIndex = (int)sourceX + ((int)sourceY * key.Area.Width);
                var targetIndex = x + (y * scaledWidth);
                textureData[targetIndex] = baseTextureData[sourceIndex];
            }
        }
    }

    private void ApplyLayer(
        IPatchModel layer,
        TextureKey key,
        Vector2 origin,
        int scaledWidth,
        int scaledHeight,
        float scale,
        int frameWidth,
        int fastestAnimation,
        IList<Color> textureData)
    {
        if (layer.Texture == null)
        {
            return;
        }

        var layerId = layer.GetCurrentId();
        if (!this.cachedData.TryGetValue(layerId, out var layerData))
        {
            layerData = (Color[])layer.Texture.Data.Clone();
            TextureManager.ApplyTintIfApplicable(layer, layerData);
            this.cachedData[layerId] = layerData;
        }

        var scaleFactor = 1f / (scale * layer.Scale);
        var offsetX = (int)(scale * (layer.SourceArea.X - key.Area.X + (int)origin.X + (int)layer.Offset.X));
        var offsetY = (int)(scale * (layer.SourceArea.Y - key.Area.Y + (int)origin.Y + (int)layer.Offset.Y));

        for (var y = offsetY; y < scaledHeight; ++y)
        {
            for (var x = 0; x < scaledWidth; ++x)
            {
                var frame = layer.Animate == Animate.None
                    ? 0
                    : x / frameWidth * fastestAnimation / (int)layer.Animate % layer.Frames;

                // Map layer index to target index
                var patchX = layer.Area.X
                    + ((x - offsetX) % (frameWidth * layer.Frames) * scaleFactor)
                    + (frame / layer.Frames * layer.Area.Width);

                var patchY = layer.Area.Y + ((y - offsetY) * scaleFactor);

                // Ensure patchX and patchY are withing the layer's area
                if (!layer.Area.Contains(patchX, patchY))
                {
                    continue;
                }

                var sourceIndex = (int)patchX + ((int)patchY * layer.Texture.Width);
                var targetIndex = x + (y * scaledWidth);
                if (sourceIndex >= 0
                    && sourceIndex < layerData.Length
                    && (layer.PatchMode == PatchMode.Replace || layerData[sourceIndex].A > 0))
                {
                    textureData[targetIndex] = layerData[sourceIndex];
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
                ((BaseTexture)baseTexture).ClearCache();
            }
        }
    }
}