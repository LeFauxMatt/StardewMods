namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ManagedTexture(Texture2D texture, float scale, Vector2 offset) : IManagedTexture
{
    /// <inheritdoc />
    public Texture2D Texture { get; } = texture;

    /// <inheritdoc />
    public float Scale { get; } = scale;

    /// <inheritdoc />
    public Vector2 Offset { get; } = offset;
}