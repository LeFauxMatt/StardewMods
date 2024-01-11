namespace StardewMods.ItemIconOverlays.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.ItemIconOverlays.Framework.Interfaces;

/// <inheritdoc />
internal sealed class IconData(string path, string value, string texture, Rectangle sourceRect) : IIconData
{
    /// <inheritdoc />
    public string Path { get; } = path;

    /// <inheritdoc />
    public string Value { get; } = value;

    /// <inheritdoc />
    public string Texture { get; } = texture;

    /// <inheritdoc />
    public Rectangle SourceRect { get; } = sourceRect;
}