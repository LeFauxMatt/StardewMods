namespace StardewMods.ExpandedStorage.Models;

using System;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;

/// <summary>
///     Cached texture and attributes for a custom storage.
/// </summary>
internal sealed class CachedStorage
{
    private readonly ICustomStorage _storage;

    private Texture2D? _texture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachedStorage" /> class.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    public CachedStorage(ICustomStorage storage)
    {
        this._storage = storage;
        this.Frames = this.Texture.Width / this._storage.Width;
        this.TileDepth = (int)Math.Ceiling(this._storage.Depth / 16f);
        this.TileHeight = (int)Math.Ceiling(this._storage.Height / 16f);
        this.TileWidth = (int)Math.Ceiling(this._storage.Width / 16f);
        this.ScaleMultiplier = Math.Min(1f / this.TileWidth, 2f / this.TileHeight);
    }

    /// <summary>
    ///     Gets the animation frames.
    /// </summary>
    public int Frames { get; }

    /// <summary>
    ///     Gets scale multiplier for oversizes objects.
    /// </summary>
    public float ScaleMultiplier { get; }

    /// <summary>
    ///     Gets the sprite sheet texture.
    /// </summary>
    public Texture2D Texture => this._texture ??= Game1.content.Load<Texture2D>(this._storage.Image);

    /// <summary>
    ///     Gets the tile depth.
    /// </summary>
    public int TileDepth { get; }

    /// <summary>
    ///     Gets the tile height.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    ///     Gets the tile width.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    ///     Resets the cached texture.
    /// </summary>
    public void ResetCache()
    {
        this._texture = null;
    }
}