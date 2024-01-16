namespace StardewMods.SpritePatcher.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Models;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Extension methods for Sprite Patcher.</summary>
internal static class Extensions
{
    /// <summary>Tries to build a texture by combining multiple texture layers.</summary>
    /// <param name="layers">The list of texture layers to combine.</param>
    /// <param name="baseTexture">The base texture to use as a background.</param>
    /// <param name="sourceRect">The source rectangle from the base texture to build the final texture from.</param>
    /// <param name="texture">When this method returns, contains the built texture if successful, otherwise null.</param>
    /// <returns>True if the texture was successfully built, otherwise false.</returns>
    public static bool TryBuildTexture(
        this List<ITextureModel> layers,
        Texture2D baseTexture,
        Rectangle sourceRect,
        [NotNullWhen(true)] out Texture2D? texture)
    {
        texture = null;
        if (!layers.Any())
        {
            return false;
        }

        var data = new Color[sourceRect.Width * sourceRect.Height];
        if (layers.First().PatchMode == PatchMode.Overlay)
        {
            baseTexture.GetData(0, sourceRect, data, 0, data.Length);
        }

        foreach (var layer in layers)
        {
            if (layer.Texture == null || layer.Area == null)
            {
                continue;
            }

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
                        if (layer.Texture.Data[index].A <= 0)
                        {
                            continue;
                        }

                        var baseTint = new Color(
                            layer.Texture.Data[index].R / 255f * layer.Tint.Value.R / 255f,
                            layer.Texture.Data[index].G / 255f * layer.Tint.Value.G / 255f,
                            layer.Texture.Data[index].B / 255f * layer.Tint.Value.B / 255f);

                        layer.Texture.Data[index] = Color.Lerp(baseTint, boostedTint, 0.3f);
                    }
                }
            }

            // Apply layer to data
            for (var y = 0; y < layer.Area.Value.Height; ++y)
            {
                for (var x = 0; x < layer.Area.Value.Width; ++x)
                {
                    var sourceIndex = ((layer.Area.Value.Y + y) * layer.Texture.Width) + layer.Area.Value.X + x;
                    var targetIndex = (y * layer.Area.Value.Width) + x;
                    if (layer.PatchMode == PatchMode.Replace || layer.Texture.Data[sourceIndex].A > 0)
                    {
                        data[targetIndex] = layer.Texture.Data[sourceIndex];
                    }
                }
            }
        }

        texture = new Texture2D(baseTexture.GraphicsDevice, sourceRect.Width, sourceRect.Height);
        texture.SetData(data);
        return true;
    }
}