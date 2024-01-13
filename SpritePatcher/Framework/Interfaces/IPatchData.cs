namespace StardewMods.SpritePatcher.Framework.Interfaces;

using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Models;

/// <summary>Data for an icon overlay.</summary>
internal interface IPatchData
{
    /// <summary>Gets the message to log for when the patch is applied.</summary>
    string? LogName { get; }

    /// <summary>Gets the target sprite sheet being patched.</summary>
    string Target { get; }

    /// <summary>Gets the path to the texture.</summary>
    string Path { get; }

    /// <summary>Gets the draw methods where the texture can be applied.</summary>
    List<DrawMethod> DrawMethods { get; }

    /// <summary>Gets the mode that the patch will be applied.</summary>
    PatchMode PatchMode { get; }

    /// <summary>Gets a mapping of property values to path names.</summary>
    Dictionary<string, TokenDefinition> Tokens { get; }

    /// <summary>Gets the textures to use for the patch.</summary>
    List<ConditionalTexture> Textures { get; }

    /// <summary>Gets the priority of the patch which determines the order in which patches are applied.</summary>
    public int Priority { get; }
}