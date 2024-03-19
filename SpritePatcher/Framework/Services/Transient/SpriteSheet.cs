namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <inheritdoc cref="StardewMods.SpritePatcher.Framework.Interfaces.ISpriteSheet" />
internal sealed class SpriteSheet : ISpriteSheet, IDisposable
{
    private const int Limit = 10;

    private static int lastTicks;
    private static int counter;

    private readonly Dictionary<PatchLayer, ISpriteSheet> children = new();
    private readonly int frames;
    private readonly SpriteKey key;
    private readonly Vector2 offset;
    private readonly float scale;
    private readonly double tickMultiplier = (Game1.random.NextDouble() * 0.1f) + 0.95f;
    private readonly int tickOffset = Game1.random.Next(0, 20);
    private readonly int ticksPerFrame;

    private bool initialized;
    private Texture2D? texture;

    public SpriteSheet(SpriteKey key, Texture2D texture, IRawTextureData data)
    {
        this.key = key;
        this.texture = texture;
        this.Data = data;
    }

    /// <summary>Initializes a new instance of the <see cref="SpriteSheet" /> class.</summary>
    /// <param name="key">A key for the original texture method.</param>
    /// <param name="data">The generated texture data.</param>
    /// <param name="width">The width of the generated texture.</param>
    /// <param name="height">The height of the generated texture.</param>
    /// <param name="scale">The scale factor to apply to the sprite sheet.</param>
    /// <param name="offset">The offset position to apply to the sprite sheet.</param>
    /// <param name="frames">The number of frames in the sprite sheet.</param>
    /// <param name="ticksPerFrame">The number of game ticks per frame.</param>
    public SpriteSheet(
        SpriteKey key,
        IRawTextureData data,
        int width,
        int height,
        float scale,
        Vector2 offset,
        int frames,
        int ticksPerFrame)
    {
        this.key = key;
        this.Data = data;
        this.Width = width;
        this.Height = height;
        this.scale = scale;
        this.offset = offset;
        this.frames = frames;
        this.ticksPerFrame = ticksPerFrame;
        this.WasAccessed = true;
    }

    /// <inheritdoc/>
    public IAssetName Target => this.key.Target;

    /// <inheritdoc/>
    public Rectangle SourceRectangle => this.key.SourceRectangle;

    /// <inheritdoc />
    public Texture2D Texture
    {
        get
        {
            if (SpriteSheet.lastTicks != Game1.ticks)
            {
                SpriteSheet.lastTicks = Game1.ticks;
                SpriteSheet.counter = 0;
            }

            if (this.initialized || SpriteSheet.counter >= SpriteSheet.Limit)
            {
                return this.texture;
            }

            SpriteSheet.counter++;
            this.texture = new Texture2D(this.texture.GraphicsDevice, this.Width, this.Height);
            this.texture.SetData(this.Data);
            this.initialized = true;
            return this.texture;
        }
    }

    /// <inheritdoc />
    public float Scale => this.initialized ? this.scale : 1f;

    /// <inheritdoc />
    public Vector2 Offset => this.initialized ? this.offset : Vector2.Zero;

    /// <inheritdoc />
    public Rectangle SourceArea =>
        this.initialized
            ? this.frames == 0
                ? new Rectangle(0, 0, this.Texture.Width, this.Texture.Height)
                : new Rectangle(
                    this.Texture.Width
                    / this.frames
                    * (int)((Game1.ticks + this.tickOffset) * this.tickMultiplier / this.ticksPerFrame % this.frames),
                    0,
                    this.Texture.Width / this.frames,
                    this.Texture.Height)
            : this.key.SourceRectangle;

    /// <inheritdoc/>
    public Color Color { get; set; }

    /// <inheritdoc/>
    public float Rotation { get; set; }

    /// <inheritdoc/>
    public SpriteEffects Effects { get; set; }

    /// <inheritdoc />
    public bool WasAccessed { get; set; }

    /// <inheritdoc/>
    public IRawTextureData Data { get; }

    public ISpriteSheet AddLayer(IRawTextureData data, ISpritePatch patch)
    {
        var layerKey = new PatchLayer(
            patch.Path,
            patch.Area,
            patch.Offset,
            patch.Tint,
            patch.Animate,
            patch.Frames,
            patch.Scale,
            patch.Alpha,
            patch.Color,
            patch.Rotation,
            patch.Effects);

        if (!this.children.TryGetValue(layerKey, out var layer))
        {
            layer = new SpriteSheet(this.key, data);
            this.children.Add(layerKey, layer);
        }

        return layer;
    }

    /// <inheritdoc />
    public void Dispose() => this.texture?.Dispose();
}