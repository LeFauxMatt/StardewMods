namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class ContentModel : IContentModel
{
    /// <summary>Initializes a new instance of the <see cref="ContentModel" /> class.</summary>
    /// <param name="target">The target asset.</param>
    /// <param name="area">The target area.</param>
    /// <param name="drawMethods">The draw methods.</param>
    /// <param name="priority">The priority.</param>
    /// <param name="code">The code.</param>
    public ContentModel(
        string target,
        Rectangle area,
        List<DrawMethod> drawMethods,
        int priority,
        string code)
    {
        this.Target = target;
        this.SourceArea = area;
        this.DrawMethods = drawMethods;
        this.Priority = priority;
        this.Code = code;
    }

    /// <inheritdoc />
    public string Target { get; set; }

    /// <inheritdoc />
    public Rectangle SourceArea { get; }

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; set; }

    /// <inheritdoc />
    public int Priority { get; set; }

    /// <inheritdoc />
    public string Code { get; set; }
}