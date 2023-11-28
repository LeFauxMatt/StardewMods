namespace StardewMods.ExpandedStorage.Models;

using System;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;

/// <summary>
///     Cached texture and attributes for a custom storage.
/// </summary>
internal sealed class CachedStorage
{
    private readonly Lazy<int> frames;
    private readonly Lazy<float> scaleMultiplier;
    private readonly ICustomStorage storage;
    private readonly Lazy<int> tileDepth;
    private readonly Lazy<int> tileHeight;
    private readonly Lazy<int> tileWidth;

    private Texture2D? texture;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CachedStorage" /> class.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    public CachedStorage(ICustomStorage storage)
    {
        this.storage = storage;
        this.frames = new(() => this.Texture.Width / this.storage.Width);
        this.tileDepth = new(() => (int)Math.Ceiling(this.storage.Depth / 16f));
        this.tileHeight = new(() => (int)Math.Ceiling(this.storage.Height / 16f));
        this.tileWidth = new(() => (int)Math.Ceiling(this.storage.Width / 16f));
        this.scaleMultiplier = new(() => Math.Min(1f / this.TileWidth, 2f / this.TileHeight));
    }

    /// <summary>
    ///     Gets the animation frames.
    /// </summary>
    public int Frames => this.frames.Value;

    /// <summary>
    ///     Gets scale multiplier for oversizes objects.
    /// </summary>
    public float ScaleMultiplier => this.scaleMultiplier.Value;

    /// <summary>
    ///     Gets the sprite sheet texture.
    /// </summary>
    public Texture2D Texture => this.texture ??= Game1.content.Load<Texture2D>(this.storage.Image);

    /// <summary>
    ///     Gets the tile depth.
    /// </summary>
    public int TileDepth => this.tileDepth.Value;

    /// <summary>
    ///     Gets the tile height.
    /// </summary>
    public int TileHeight => this.tileHeight.Value;

    /// <summary>
    ///     Gets the tile width.
    /// </summary>
    public int TileWidth => this.tileWidth.Value;

    /// <summary>
    ///     Resets the cached texture.
    /// </summary>
    public void ResetCache()
    {
        this.texture = null;
    }
}