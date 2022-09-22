namespace StardewMods.ExpandedStorage.Models;

using System;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;

/// <summary>
///     Cached texture and attributes for a custom storage.
/// </summary>
internal sealed class CachedStorage
{
    private readonly Lazy<int> _frames;
    private readonly Lazy<float> _scaleMultiplier;
    private readonly ICustomStorage _storage;
    private readonly Lazy<int> _tileDepth;
    private readonly Lazy<int> _tileHeight;
    private readonly Lazy<int> _tileWidth;

    private Texture2D? _texture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachedStorage" /> class.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    public CachedStorage(ICustomStorage storage)
    {
        this._storage = storage;
        this._frames = new(() => this.Texture.Width / this._storage.Width);
        this._tileDepth = new(() => (int)Math.Ceiling(this._storage.Depth / 16f));
        this._tileHeight = new(() => (int)Math.Ceiling(this._storage.Height / 16f));
        this._tileWidth = new(() => (int)Math.Ceiling(this._storage.Width / 16f));
        this._scaleMultiplier = new(() => Math.Min(1f / this.TileWidth, 2f / this.TileHeight));
    }

    /// <summary>
    ///     Gets the animation frames.
    /// </summary>
    public int Frames => this._frames.Value;

    /// <summary>
    ///     Gets scale multiplier for oversizes objects.
    /// </summary>
    public float ScaleMultiplier => this._scaleMultiplier.Value;

    /// <summary>
    ///     Gets the sprite sheet texture.
    /// </summary>
    public Texture2D Texture => this._texture ??= Game1.content.Load<Texture2D>(this._storage.Image);

    /// <summary>
    ///     Gets the tile depth.
    /// </summary>
    public int TileDepth => this._tileDepth.Value;

    /// <summary>
    ///     Gets the tile height.
    /// </summary>
    public int TileHeight => this._tileHeight.Value;

    /// <summary>
    ///     Gets the tile width.
    /// </summary>
    public int TileWidth => this._tileWidth.Value;

    /// <summary>
    ///     Resets the cached texture.
    /// </summary>
    public void ResetCache()
    {
        this._texture = null;
    }
}