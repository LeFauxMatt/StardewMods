namespace XSLite.Migrations.JsonAsset
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewValley;

    internal class TextureMigrator
    {
        private const int Frames = 5;
        private const int Width = 16;
        private const int Height = 32;
        private const int TotalWidth = 80;
        private readonly string _path;

        private readonly Dictionary<string, Texture2D> _textures = new();

        public TextureMigrator(IContentPack contentPack, string name)
        {
            this._path = Path.Combine(contentPack.DirectoryPath, "assets", $"{name}.png");
        }

        public void AddTexture(string name, Texture2D texture)
        {
            this._textures.Add(name, texture);
        }

        public void UpdateTextureFormat()
        {
            if (File.Exists(this._path))
            {
                return;
            }

            if (this._textures.Count == 0 || !this._textures.TryGetValue("big-craftable.png", out var baseTexture) || baseTexture.Width != TextureMigrator.Width || baseTexture.Height != TextureMigrator.Height)
            {
                return;
            }

            var layers = this._textures.Count switch
            {
                >= 15 => 3,
                _ => 1,
            };

            var totalHeight = layers * TextureMigrator.Height;
            var pixels = new Color[TextureMigrator.TotalWidth * totalHeight];

            for (var frame = 0; frame < TextureMigrator.Frames; frame++)
            {
                for (var layer = 0; layer < layers; layer++)
                {
                    var baseOffset = frame * TextureMigrator.Width + layer * TextureMigrator.TotalWidth * TextureMigrator.Height;

                    // Base Layer
                    if (!this._textures.TryGetValue($"big-craftable-{(1 + layer * 6).ToString()}", out var sourceTexture) || sourceTexture.Width != TextureMigrator.Width || sourceTexture.Height != TextureMigrator.Height)
                    {
                        sourceTexture = baseTexture;
                    }

                    var subPixels = new Color[TextureMigrator.Width * TextureMigrator.Height];
                    sourceTexture.GetData(subPixels);
                    for (var i = 0; i < subPixels.Length; i++)
                    {
                        var targetOffset = baseOffset + i % TextureMigrator.Width + i / TextureMigrator.Width * TextureMigrator.TotalWidth;
                        pixels[targetOffset] = subPixels[i];
                    }

                    // Lid Layer
                    if (!this._textures.TryGetValue($"big-craftable-{(2 + frame + layer * 6).ToString()}.png", out sourceTexture) || sourceTexture.Width != TextureMigrator.Width || sourceTexture.Height != TextureMigrator.Height)
                    {
                        continue;
                    }

                    sourceTexture.GetData(subPixels);
                    for (var i = 0; i < subPixels.Length; i++)
                    {
                        if (subPixels[i] == Color.Transparent)
                        {
                            continue;
                        }

                        var targetOffset = baseOffset + i % TextureMigrator.Width + i / TextureMigrator.Width * TextureMigrator.TotalWidth;
                        pixels[targetOffset] = subPixels[i];
                    }
                }
            }

            var targetTexture = new Texture2D(Game1.graphics.GraphicsDevice, TextureMigrator.TotalWidth, totalHeight);
            targetTexture.SetData(pixels);
            using FileStream stream = new(this._path, FileMode.CreateNew);
            targetTexture.SaveAsPng(stream, TextureMigrator.TotalWidth, totalHeight);
        }
    }
}