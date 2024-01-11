namespace StardewMods.Common.Services.Integrations.FuryCore;

using System.Reflection;
using StardewMods.Common.Enums;

/// <summary>
/// Represents a patch for modifying a method using Harmony.
/// </summary>
public interface ISavedPatch
{
    /// <summary>
    /// Gets the unique identifier of the patch.
    /// </summary>
    string? LogId { get; }

    /// <summary>
    /// Gets the original method.
    /// </summary>
    public MethodBase Original { get; }

    /// <summary>
    /// Gets the harmony method.
    /// </summary>
    public MethodInfo Patch { get; }

    /// <summary>
    /// Gets the patch type.
    /// </summary>
    public PatchType Type { get; }
}