namespace StardewMods.SpritePatcher.Framework.Services;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <summary>Build and cache textures from patch layers.</summary>
internal sealed class TextureBuilder : BaseService
{
    private readonly Dictionary<string, Color[]> cachedData = [];
    private readonly Dictionary<string, Texture2D> cachedTextures = [];

    /// <summary>Initializes a new instance of the <see cref="TextureBuilder" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public TextureBuilder(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Tries to build a texture by combining multiple texture layers.</summary>
    /// <param name="key">A key for the original texture method.</param>
    /// <param name="baseTexture">The base texture to use as a background.</param>
    /// <param name="layers">The list of texture layers to combine.</param>
    /// <param name="texture">When this method returns, contains the built texture if successful, otherwise null.</param>
    /// <returns>True if the texture was successfully built, otherwise false.</returns>
    public bool TryBuildTexture(
        TextureKey key,
        Texture2D baseTexture,
        List<IPatchModel> layers,
        [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = null;
        if (!layers.Any())
        {
            return false;
        }

        var sourceRect = key.Area ?? new Rectangle(0, 0, baseTexture.Width, baseTexture.Height);
        var cacheKey = TextureBuilder.GetCachedTextureKey(layers, baseTexture, sourceRect);
        if (this.cachedTextures.TryGetValue(cacheKey, out texture))
        {
            return true;
        }

        var textureData = new Color[sourceRect.Width * sourceRect.Height];
        if (layers.First().PatchMode == PatchMode.Overlay)
        {
            baseTexture.GetData(0, sourceRect, textureData, 0, textureData.Length);
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

            // Apply layer to data
            var targetArea = layer.SourceArea ?? sourceRect;
            var left = Math.Max(sourceRect.Left, targetArea.Left);
            var top = Math.Max(sourceRect.Top, targetArea.Top);
            var right = Math.Min(sourceRect.Right, targetArea.Right);
            var bottom = Math.Min(sourceRect.Bottom, targetArea.Bottom);

            for (var y = top; y < bottom; ++y)
            {
                for (var x = left; x < right; ++x)
                {
                    // Calculate the index in the layer's texture array, aligned to the top-left of layer.Area
                    var patchX = x - targetArea.Left + layer.Area.Value.X;
                    var patchY = y - targetArea.Top + layer.Area.Value.Y;
                    var sourceIndex = (patchY * layer.Texture.Width) + patchX;

                    // Calculate the index in the target data array
                    var targetX = x - sourceRect.X;
                    var targetY = y - sourceRect.Y;
                    var targetIndex = (targetY * sourceRect.Width) + targetX;

                    // Apply the patch if within bounds and according to PatchMode
                    if (sourceIndex >= 0
                        && targetIndex >= 0
                        && sourceIndex < layerData.Length
                        && targetIndex < textureData.Length
                        && (layer.PatchMode == PatchMode.Replace || layerData[sourceIndex].A > 0))
                    {
                        textureData[targetIndex] = layerData[sourceIndex];
                    }
                }
            }
        }

        texture = new Texture2D(baseTexture.GraphicsDevice, sourceRect.Width, sourceRect.Height);
        texture.SetData(textureData);
        texture.Name = cacheKey;
        this.cachedTextures[cacheKey] = texture;
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
}