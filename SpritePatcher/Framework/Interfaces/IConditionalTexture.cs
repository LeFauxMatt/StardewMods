namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;

/// <summary>Represents a conditional texture.</summary>
public interface IConditionalTexture
{
    /// <summary>Gets the path to the texture.</summary>
    string Path { get; }

    /// <summary>Gets the conditions for applying the texture.</summary>
    Dictionary<string, string> Conditions { get; }

    /// <summary>Gets the area of the texture to draw.</summary>
    public Rectangle? FromArea { get; }

    /// <summary>Gets the tint to apply to the texture.</summary>
    public Color? Tint { get; }
}