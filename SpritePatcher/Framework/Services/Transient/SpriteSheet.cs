namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class SpriteSheet(
    Texture2D texture,
    float scale,
    Vector2 offset,
    int frames,
    int ticksPerFrame) : ISpriteSheet
{
    private readonly int tickOffset = Game1.random.Next(0, 20);
    private readonly double tickMultiplier = (Game1.random.NextDouble() * 0.1f) + 0.95f;

    /// <inheritdoc />
    public Texture2D Texture { get; } = texture;

    /// <inheritdoc />
    public float Scale { get; } = scale;

    /// <inheritdoc />
    public Vector2 Offset { get; } = offset;

    /// <inheritdoc />
    public Rectangle SourceRectangle =>
        frames == 0
            ? new Rectangle(0, 0, this.Texture.Width, this.Texture.Height)
            : new Rectangle(
                this.Texture.Width
                / frames
                * (int)((Game1.ticks + this.tickOffset) * this.tickMultiplier / ticksPerFrame % frames),
                0,
                this.Texture.Width / frames,
                this.Texture.Height);
}