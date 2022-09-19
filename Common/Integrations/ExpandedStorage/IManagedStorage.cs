namespace StardewMods.Common.Integrations.ExpandedStorage;

using System;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Interface to a chest managed by Expanded Storage.
/// </summary>
public interface IManagedStorage : ICustomStorage
{
    /// <summary>
    ///     Gets the animation frames.
    /// </summary>
    public int Frames => this.Texture.Width / this.Width;

    /// <summary>
    ///     Gets scale multiplier for oversizes objects.
    /// </summary>
    public float ScaleMultiplier => Math.Min(1f / this.TileWidth, 2f / this.TileHeight);

    /// <summary>
    ///     Gets the sprite sheet texture.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    ///     Gets the tile height.
    /// </summary>
    public int TileHeight => (int)Math.Ceiling(this.Height / 16f);

    /// <summary>
    ///     Gets the tile width.
    /// </summary>
    public int TileWidth => (int)Math.Ceiling(this.Width / 16f);
}