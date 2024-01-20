namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ContentModel(
    string target,
    Rectangle area,
    List<DrawMethod> drawMethods,
    PatchMode patchMode,
    int priority,
    string code) : IContentModel
{
    /// <inheritdoc />
    public Rectangle SourceArea { get; } = area;

    /// <inheritdoc />
    public string Target { get; set; } = target;

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; set; } = drawMethods;

    /// <inheritdoc />
    public PatchMode PatchMode { get; set; } = patchMode;

    /// <inheritdoc />
    public int Priority { get; set; } = priority;

    /// <inheritdoc />
    public string Code { get; set; } = code;
}