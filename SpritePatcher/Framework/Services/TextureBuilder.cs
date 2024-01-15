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
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="TextureBuilder" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public TextureBuilder(IGameContentHelper gameContentHelper, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.gameContentHelper = gameContentHelper;

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
            var layerTexture = this.gameContentHelper.Load<Texture2D>(layer.Path);
            var layerArea = layer.Area ?? new Rectangle(0, 0, layerTexture.Width, layerTexture.Height);
            var layerData = new Color[layerArea.Width * layerArea.Height];
            layerTexture.GetData(0, layerArea, layerData, 0, layerData.Length);

            // Apply tinting if applicable
            if (layer.Tint is not null)
            {
                var hsl = HslColor.FromColor(layer.Tint.Value);
                var boostedTint = new HslColor(hsl.H, 2f * hsl.S, 2f * hsl.L).ToRgbColor();
                for (var i = 0; i < layerData.Length; ++i)
                {
                    if (layerData[i].A <= 0)
                    {
                        continue;
                    }

                    var baseTint = new Color(
                        layerData[i].R / 255f * layer.Tint.Value.R / 255f,
                        layerData[i].G / 255f * layer.Tint.Value.G / 255f,
                        layerData[i].B / 255f * layer.Tint.Value.B / 255f);

                    layerData[i] = Color.Lerp(baseTint, boostedTint, 0.3f);
                }
            }

            // Apply patch
            switch (layer.PatchMode)
            {
                case PatchMode.Replace:
                    data = layerData;
                    break;

                default:
                    if (data is null)
                    {
                        data = new Color[sourceRect.Width * sourceRect.Height];
                        baseTexture.GetData(0, sourceRect, data, 0, data.Length);
                    }

                    for (var i = 0; i < data.Length; ++i)
                    {
                        if (layerData[i].A > 0)
                        {
                            data[i] = layerData[i];
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