namespace StardewMods.SpritePatcher.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Helps build a texture object from patches.</summary>
internal sealed class TextureBuilder : BaseService
{
    /// <summary>Initializes a new instance of the <see cref="TextureBuilder" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public TextureBuilder(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Tries to get a modified texture for the given entity using patches and conditions.</summary>
    /// <param name="baseTexture">The base texture to modify.</param>
    /// <param name="sourceRect">The rectangle defining the area of the texture to modify.</param>
    /// <param name="layers">The layers to build the texture from.</param>
    /// <param name="texture">The modified texture, if successful; otherwise, null.</param>
    /// <returns>True if a modified texture was found and applied; otherwise, false.</returns>
    public bool TryBuildTexture(
        Texture2D baseTexture,
        Rectangle sourceRect,
        IEnumerable<ITextureModel> layers,
        [NotNullWhen(true)] out Texture2D? texture)
    {
        Color[]? data = null;
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

            // Apply patch
            switch (layer.PatchMode)
            {
                case PatchMode.Replace:
                    data = new Color[sourceRect.Width * sourceRect.Height];
                    for (var y = 0; y < layer.Area.Value.Height; ++y)
                    {
                        for (var x = 0; x < layer.Area.Value.Width; ++x)
                        {
                            var sourceIndex = ((layer.Area.Value.Y + y) * layer.Texture.Width) + layer.Area.Value.X + x;
                            var targetIndex = (y * layer.Area.Value.Width) + x;
                            data[targetIndex] = layer.Texture.Data[sourceIndex];
                        }
                    }
                    break;

                default:
                    if (data == null)
                    {
                        data = new Color[sourceRect.Width * sourceRect.Height];
                        baseTexture.GetData(0, sourceRect, data, 0, data.Length);
                    }

                    for (var y = 0; y < layer.Area.Value.Height; ++y)
                    {
                        for (var x = 0; x < layer.Area.Value.Width; ++x)
                        {
                            var sourceIndex = ((layer.Area.Value.Y + y) * layer.Texture.Width) + layer.Area.Value.X + x;
                            var targetIndex = (y * layer.Area.Value.Width) + x;
                            data[targetIndex] = layer.Texture.Data[sourceIndex];
                        }
                    }

                    break;
            }
        }

        texture = new Texture2D(baseTexture.GraphicsDevice, sourceRect.Width, sourceRect.Height);
        texture.SetData(data);
        return true;
    }
}