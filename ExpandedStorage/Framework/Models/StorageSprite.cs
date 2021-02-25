using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImJustMatt.ExpandedStorage.Framework.Models
{
    internal class StorageSprite
    {
        internal StorageSprite(Storage storage)
        {
            Texture = !string.IsNullOrWhiteSpace(storage.Image) && ExpandedStorage.AssetLoaders.TryGetValue(storage.ModUniqueId, out var loadTexture)
                ? loadTexture.Invoke($"assets/{storage.Image}")
                : null;
            Width = Texture != null ? Texture.Width / Math.Max(1, storage.Frames) : 16;
            Height = Texture != null ? storage.PlayerColor ? Texture.Height / 3 : Texture.Height : 32;
            TileWidth = Width / 16;
            TileHeight = (storage.Depth is { } depth && depth > 0 ? depth : Height - 16) / 16;
        }

        /// <summary>Property to access the SpriteSheet image.</summary>
        internal Texture2D Texture { get; }

        internal int Width { get; }
        internal int Height { get; }
        internal int TileWidth { get; }
        internal int TileHeight { get; }

        internal float ScaleSize
        {
            get
            {
                var tilesWide = Width / 16f;
                var tilesHigh = Height / 16f;
                return tilesWide switch
                {
                    >= 7 => 0.5f,
                    >= 6 => 0.66f,
                    >= 5 => 0.75f,
                    _ => tilesHigh switch
                    {
                        >= 5 => 0.8f,
                        >= 3 => 1f,
                        _ => tilesWide switch
                        {
                            <= 2 => 2f,
                            <= 4 => 1f,
                            _ => 0.1f
                        }
                    }
                };
            }
        }

        internal void ForEachPos(int x, int y, Action<Vector2> doAction)
        {
            for (var i = 0; i < TileWidth; i++)
            {
                for (var j = 0; j < TileHeight; j++)
                {
                    var pos = new Vector2(x + i, y + j);
                    doAction.Invoke(pos);
                }
            }
        }
    }
}