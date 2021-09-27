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

        private readonly Dictionary<string, Texture2D> _textures = new();
        private readonly string _path;

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

            if (this._textures.Count == 0 || !this._textures.TryGetValue("big-craftable.png", out Texture2D baseTexture) || baseTexture.Width != TextureMigrator.Width || baseTexture.Height != TextureMigrator.Height)
            {
                return;
            }

            int layers = this._textures.Count switch
            {
                >= 15 => 3,
                _ => 1,
            };

            int totalHeight = layers * TextureMigrator.Height;
            var pixels = new Color[TextureMigrator.TotalWidth * totalHeight];

            for (int frame = 0; frame < TextureMigrator.Frames; frame++)
            {
                for (int layer = 0; layer < layers; layer++)
                {
                    int baseOffset = (frame * TextureMigrator.Width) + (layer * TextureMigrator.TotalWidth * TextureMigrator.Height);

                    // Base Layer
                    if (!this._textures.TryGetValue($"big-craftable-{(1 + (layer * 6)).ToString()}", out Texture2D sourceTexture) || sourceTexture.Width != TextureMigrator.Width || sourceTexture.Height != TextureMigrator.Height)
                    {
                        sourceTexture = baseTexture;
                    }

                    var subPixels = new Color[TextureMigrator.Width * TextureMigrator.Height];
                    sourceTexture.GetData(subPixels);
                    for (int i = 0; i < subPixels.Length; i++)
                    {
                        int targetOffset = baseOffset + (i % TextureMigrator.Width) + (i / TextureMigrator.Width * TextureMigrator.TotalWidth);
                        pixels[targetOffset] = subPixels[i];
                    }

                    // Lid Layer
                    if (!this._textures.TryGetValue($"big-craftable-{(2 + frame + (layer * 6)).ToString()}.png", out sourceTexture) || sourceTexture.Width != TextureMigrator.Width || sourceTexture.Height != TextureMigrator.Height)
                    {
                        continue;
                    }

                    sourceTexture.GetData(subPixels);
                    for (int i = 0; i < subPixels.Length; i++)
                    {
                        if (subPixels[i] == Color.Transparent)
                        {
                            continue;
                        }

                        int targetOffset = baseOffset + (i % TextureMigrator.Width) + (i / TextureMigrator.Width * TextureMigrator.TotalWidth);
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