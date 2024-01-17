namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents a conditional patch.</summary>
public interface IPatchModel
{
    /// <summary>Gets the unique identifier for this mod.</summary>
    string Id { get; }

    /// <summary>Gets the content pack associated with this mod.</summary>
    IContentPack? ContentPack { get; }

    /// <summary>Gets the target sprite sheet being patched.</summary>
    string Target { get; }

    /// <summary>Gets the draw methods where the texture can be applied.</summary>
    List<DrawMethod> DrawMethods { get; }

    /// <summary>Gets the net fields that will be used to invalidate the cache.</summary>
    List<string> NetFields { get; }

    /// <summary>Gets the path to the texture.</summary>
    IRawTextureData? Texture { get; }

    /// <summary>Gets the area of the texture.</summary>
    Rectangle? Area { get; }

    /// <summary>Gets the source rectangle of the sprite sheet being patched.</summary>
    Rectangle? SourceArea { get; }

    /// <summary>Gets the tint of the texture.</summary>
    Color? Tint { get; }

    /// <summary>Gets the mode that the patch will be applied.</summary>
    PatchMode PatchMode { get; }

    /// <summary>Retrieves a unique identifier for the current patch.</summary>
    /// <returns>The current ID as a string.</returns>
    string GetCurrentId();

    /// <summary>Runs code necessary to update the texture.</summary>
    /// <param name="managedObject">The managed object requesting the patch.</param>
    /// <returns>True if the texture should be applied.</returns>
    bool Run(IManagedObject managedObject);
}