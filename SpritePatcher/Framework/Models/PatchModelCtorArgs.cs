namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents the constructor arguments for creating a PatchModel instance.</summary>
/// <param name="monitor">Dependency used for monitoring and logging.</param>
/// <param name="modId">The unique id of the mod.</param>
/// <param name="contentPack">The content pack of the mod.</param>
/// <param name="target">The target sprite sheet of the patch.</param>
/// <param name="area">The area of the patch.</param>
/// <param name="drawMethods">The draw method of the patch.</param>
/// <param name="patchMode">The patch mode.</param>
/// <param name="netFields">The net fields that will be used to invalidate the cache.</param>
public sealed class PatchModelCtorArgs(
    IMonitor monitor,
    string modId,
    IContentPack contentPack,
    string target,
    Rectangle? area,
    List<DrawMethod> drawMethods,
    PatchMode patchMode,
    List<string>? netFields)
{
    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    public IMonitor Monitor { get; } = monitor;

    /// <summary>Gets the unique identifier for this mod.</summary>
    public string ModId { get; } = modId;

    /// <summary>Gets the content pack associated with this mod.</summary>
    public IContentPack ContentPack { get; } = contentPack;

    /// <summary>Gets the target sprite sheet being patched.</summary>
    public string Target { get; } = target;

    /// <summary>Gets the source area of the sprite sheet being patched.</summary>
    public Rectangle? Area { get; } = area;

    /// <summary>Gets the draw methods where the texture can be applied.</summary>
    public List<DrawMethod> DrawMethods { get; } = drawMethods;

    /// <summary>Gets the mode that the patch will be applied.</summary>
    public PatchMode PatchMode { get; } = patchMode;

    /// <summary>Gets the net fields that will be used to invalidate the cache.</summary>
    public List<string> NetFields { get; } = netFields ?? [];
}