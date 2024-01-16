namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents a conditional patch.</summary>
public interface IPatchModel : ITextureModel
{
    /// <summary>Gets the unique identifier for this mod.</summary>
    string ModId { get; }

    /// <summary>Gets the content pack associated with this mod.</summary>
    IContentPack? ContentPack { get; }

    /// <summary>Gets the target sprite sheet being patched.</summary>
    string Target { get; }

    /// <summary>Gets the source rectangle of the sprite sheet being patched.</summary>
    Rectangle? SourceArea { get; }

    /// <summary>Gets the draw methods where the texture can be applied.</summary>
    List<DrawMethod> DrawMethods { get; }

    /// <summary>Gets the net fields that will be used to invalidate the cache.</summary>
    List<string> NetFields { get; }

    /// <summary>Runs code necessary to update the texture..</summary>
    /// <param name="entity">The entity that is being textured.</param>
    /// <returns>True if the texture should be applied.</returns>
    bool Run(IHaveModData entity);
}