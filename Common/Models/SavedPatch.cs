namespace StardewMods.Common.Models;

using System.Reflection;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class SavedPatch : ISavedPatch
{
    /// <summary>Initializes a new instance of the <see cref="SavedPatch" /> class.</summary>
    /// <param name="original">The original method represented by a MethodBase object.</param>
    /// <param name="patch">The patch method represented by a MethodInfo object.</param>
    /// <param name="patchType">The type of the patch, represented by the PatchType enumeration.</param>
    /// <param name="logId">The log ID for the saved patch. This parameter is optional and defaults to null.</param>
    public SavedPatch(MethodBase original, MethodInfo patch, PatchType patchType, string? logId = default)
    {
        this.Original = original;
        this.Patch = patch;
        this.Type = patchType;
        this.LogId = logId;
    }

    /// <inheritdoc />
    public string? LogId { get; }

    /// <inheritdoc />
    public MethodBase Original { get; }

    /// <inheritdoc />
    public MethodInfo Patch { get; }

    /// <inheritdoc />
    public PatchType Type { get; }
}