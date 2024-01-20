namespace StardewMods.SpritePatcher.Framework.Services;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
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
        Texture2D baseTexture,
        List<IPatchModel> layers,
        [NotNullWhen(true)] out IManagedTexture? managedTexture)
    {
        managedTexture = null;
        if (!layers.Any())
        {
            return false;
        }

        var sourceRect = key.Area ?? new Rectangle(0, 0, baseTexture.Width, baseTexture.Height);
        var cacheKey = TextureManager.GetCachedTextureKey(layers, baseTexture, sourceRect);
        if (this.cachedTextures.TryGetValue(cacheKey, out managedTexture))
        {
            return true;
        }

        if (!this.baseTextures.TryGetValue(baseTexture.Name, out var baseTextureData))
        {
            baseTextureData = new BaseTexture(baseTexture.Name);
            this.baseTextures[baseTexture.Name] = baseTextureData;
        }

        var scale = 1 / layers.Min(layer => layer.Scale);
        var scaledWidth = (int)(sourceRect.Width * scale);
        var scaledHeight = (int)(sourceRect.Height * scale);
        var textureData = new Color[scaledWidth * scaledHeight];
        if (layers.First().PatchMode == PatchMode.Overlay)
        {
            for (var y = 0; y < scaledHeight; ++y)
            {
                for (var x = 0; x < scaledWidth; ++x)
                {
                    var sourceX = (int)(x / scale);
                    var sourceY = (int)(y / scale);
                    var sourceIndex = (sourceY * sourceRect.Width) + sourceX;
                    var targetIndex = (y * scaledWidth) + x;
                    textureData[targetIndex] = ((BaseTexture)baseTextureData).GetData(sourceRect)[sourceIndex];
                }
            }
        }

        foreach (var layer in layers)
        {
            if (layer.Texture == null || layer.Area == null)
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
                    for (var y = layer.Area.Value.Y; y < layer.Area.Value.Y + layer.Area.Value.Height; ++y)
                    {
                        for (var x = layer.Area.Value.X; x < layer.Area.Value.X + layer.Area.Value.Width; ++x)
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

            var targetArea = layer.SourceArea ?? sourceRect;
            var scaleFactor = 1f / (scale * layer.Scale);
            var offsetX = scale * (sourceRect.X - targetArea.X);
            var offsetY = scale * (sourceRect.Y - targetArea.Y);
            var area = layer.Area.Value;

            for (var y = 0; y < scaledHeight; ++y)
            {
                for (var x = 0; x < scaledWidth; ++x)
                {
                    var targetIndex = x + (y * scaledWidth);

                    // Map to the source index in layerData
                    var patchX = area.X + (x * scaleFactor) + offsetX;
                    var patchY = area.Y + (y * scaleFactor) + offsetY;

                    // Ensure patchX and patchY are withing the layer's area
                    if (patchX <= area.Left || patchX >= area.Right || patchY <= area.Top || patchY >= area.Bottom)
                    {
                        continue;
                    }

                    var sourceIndex = (int)patchX + ((int)patchY * layer.Texture.Width);

                    if (sourceIndex >= 0
                        && sourceIndex < layerData.Length
                        && (layer.PatchMode == PatchMode.Replace || layerData[sourceIndex].A > 0))
                    {
                        textureData[targetIndex] = layerData[sourceIndex];
                    }
                }
            }
        }

        var texture = new Texture2D(baseTexture.GraphicsDevice, scaledWidth, scaledHeight);
        texture.SetData(textureData);
        texture.Name = cacheKey;
        managedTexture = new ManagedTexture(texture, scale);
        this.cachedTextures[cacheKey] = managedTexture;
        return true;
    }

    private static string GetCachedTextureKey(
        List<IPatchModel> layers,
        GraphicsResource baseTexture,
        Rectangle sourceRect)
    {
        var sb = new StringBuilder();
        sb.Append(baseTexture.Name);
        sb.Append('_');
        sb.Append(sourceRect.ToString());
        foreach (var layer in layers)
        {
            sb.Append('_');
            sb.Append(layer.GetCurrentId());
        }

        return sb.ToString();
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