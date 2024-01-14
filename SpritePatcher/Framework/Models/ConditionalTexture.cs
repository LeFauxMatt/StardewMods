namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ConditionalTexture(
    string path,
    Dictionary<string, string> conditions,
    Rectangle? fromArea,
    Color? tint) : IConditionalTexture
{
    /// <inheritdoc />
    public string Path { get; set; } = path;

    /// <inheritdoc />
    public Dictionary<string, string> Conditions { get; set; } = conditions;

    /// <inheritdoc />
    public Rectangle? FromArea { get; set; } = fromArea;

    /// <inheritdoc />
    public Color? Tint { get; set; } = tint;
}