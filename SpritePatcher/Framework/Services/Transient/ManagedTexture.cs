namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ManagedTexture(Texture2D texture, float scale) : IManagedTexture
{
    /// <inheritdoc />
    public Texture2D Texture { get; } = texture;

    /// <inheritdoc />
    public float Scale { get; } = scale;
}