namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ManagedTexture(
    Texture2D texture,
    float scale,
    Vector2 offset,
    int frames,
    int ticksPerFrame) : IManagedTexture
{
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
                this.Texture.Width / frames * (Game1.ticks / ticksPerFrame % frames),
                0,
                this.Texture.Width / frames,
                this.Texture.Height);
}