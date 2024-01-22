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

        var origin = Vector2.Zero;
        var scaledWidth = key.Area.Width;
        var scaledHeight = key.Area.Height;

        // Calculate the expanded texture based on the layers offset, area, and scale
        foreach (var layer in layers)
        {
            if (layer.Offset.X < -origin.X)
            {
                scaledWidth += (int)(origin.X - layer.Offset.X);
                origin.X = -layer.Offset.X;
            }

            if (layer.Offset.Y < -origin.Y)
            {
                scaledHeight += (int)(origin.Y - layer.Offset.Y);
                origin.Y = -layer.Offset.Y;
            }

            if (layer.Offset.X + (layer.Area.Width * layer.Scale) > origin.X + key.Area.Width)
            {
                scaledWidth += (int)(layer.Offset.X + (layer.Area.Width * layer.Scale) - (origin.X + key.Area.Width));
            }

            if (layer.Offset.Y + (layer.Area.Height * layer.Scale) > origin.Y + key.Area.Height)
            {
                scaledHeight +=
                    (int)(layer.Offset.Y + (layer.Area.Height * layer.Scale) - (origin.Y + key.Area.Height));
            }
        }

        // Expanded the texture to match the highest resolution layer
        var scale = 1 / layers.Min(layer => layer.Scale);
        scaledWidth *= (int)scale;
        scaledHeight *= (int)scale;
        var textureData = new Color[scaledWidth * scaledHeight];

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

        // Load base texture from cache if available
        if (!this.baseTextures.TryGetValue(texture.Name, out var baseTexture))
        {
            baseTexture = new BaseTexture(texture.Name);
            this.baseTextures[texture.Name] = baseTexture;
        }

        // Copy the base texture data if the first layer does not replace the texture
        if (layers.First().PatchMode != PatchMode.Replace)
        {
            var baseTextureData = ((BaseTexture)baseTexture).GetData(key.Area);
            var top = (int)(origin.Y * scale);
            var left = (int)(origin.X * scale);
            var bottom = top + (int)(key.Area.Height * scale);
            var right = left + (int)(key.Area.Width * scale);
            for (var frame = 0; frame < totalFrames; ++frame)
            {
                for (var y = top; y < bottom; ++y)
                {
                    for (var x = left; x < right; ++x)
                    {
                        var sourceX = (int)((x - left) / scale);
                        var sourceY = (int)((y - top) / scale);
                        var sourceIndex = (sourceY * key.Area.Width) + sourceX;
                        var targetIndex = (y * scaledWidth) + x + (frame * frameWidth);
                        textureData[targetIndex] = baseTextureData[sourceIndex];
                    }
                }
            }
        }

        // Apply each layer
        foreach (var layer in layers)
        {
            if (layer.Texture == null)
            {
                continue;
            }

            var layerId = layer.GetCurrentId();
            if (!this.cachedData.TryGetValue(layerId, out var layerData))
            {
                layerData = (Color[])layer.Texture.Data.Clone();

                // Apply tinting if applicable
                if (layer.Tint != null)
                {
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

                this.cachedData[layerId] = layerData;
            }

            var scaleFactor = 1f / (scale * layer.Scale);
            var offsetX = scale * (layer.SourceArea.X - key.Area.X + (int)origin.X + (int)layer.Offset.X);
            var offsetY = scale * (layer.SourceArea.Y - key.Area.Y + (int)origin.Y + (int)layer.Offset.Y);

            for (var tick = 0; tick < totalDuration; tick += fastestAnimation)
            {
                var frame = tick / (int)layer.Animate % layer.Frames;

                for (var y = 0; y < scaledHeight; ++y)
                {
                    for (var x = 0; x < scaledWidth; ++x)
                    {
                        // Map to the source index in layerData
                        var patchX = layer.Area.X
                            + (x * scaleFactor)
                            - offsetX
                            + (frame / layer.Frames * layer.Area.Width);

                        var patchY = layer.Area.Y + (y * scaleFactor) - offsetY;

                        // Ensure patchX and patchY are withing the layer's area
                        if (patchX <= layer.Area.Left
                            || patchX >= layer.Area.Right
                            || patchY <= layer.Area.Top
                            || patchY >= layer.Area.Bottom)
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