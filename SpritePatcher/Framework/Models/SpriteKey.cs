namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents a key used to identify a texture and its drawing parameters.</summary>
public readonly struct SpriteKey
{
    /// <summary>Initializes a new instance of the <see cref="SpriteKey" /> struct.</summary>
    /// <param name="target">The target.</param>
    /// <param name="area">The area.</param>
    /// <param name="drawMethod">The draw method.</param>
    public SpriteKey(string target, Rectangle area, DrawMethod drawMethod)
    {
        this.Target = target;
        this.Area = area;
        this.DrawMethod = drawMethod;
    }

    /// <summary>Gets the target.</summary>
    public string Target { get; }

    /// <summary>Gets the area.</summary>
    public Rectangle Area { get; }

    /// <summary>Gets the draw method.</summary>
    public DrawMethod DrawMethod { get; }
}