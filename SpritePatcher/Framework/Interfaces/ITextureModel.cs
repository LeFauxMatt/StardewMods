namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;

/// <summary>Represents a texture.</summary>
public interface ITextureModel
{
    /// <summary>Gets the path to the texture.</summary>
    string Path { get; }

    /// <summary>Gets the area of the texture.</summary>
    Rectangle? Area { get; }

    /// <summary>Gets the tint of the texture.</summary>
    Color? Tint { get; }

    /// <summary>Gets the mode that the patch will be applied.</summary>
    public PatchMode PatchMode { get; }
}