namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Data for an icon overlay.</summary>
public interface IContentModel
{
    /// <summary>Gets the target sprite sheet being patched.</summary>
    string Target { get; }

    /// <summary>Gets the source rectangle of the sprite sheet being patched.</summary>
    Rectangle SourceArea { get; }

    /// <summary>Gets the draw methods where the texture can be applied.</summary>
    List<DrawMethod> DrawMethods { get; }

    /// <summary>Gets the mode that the patch will be applied.</summary>
    PatchMode PatchMode { get; }

    /// <summary>Gets the priority of the patch which determines the order in which patches are applied.</summary>
    int Priority { get; }

    /// <summary>Gets the code for the patch which is used to determine if the patch should be applied.</summary>
    string Code { get; }
}