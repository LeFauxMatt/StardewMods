namespace StardewMods.Common.Models;

using System.Reflection;
using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class SavedPatch(MethodBase original, MethodInfo patch, PatchType patchType, string? logId = default) : ISavedPatch
{
    /// <inheritdoc />
    public string? LogId { get; } = logId;

    /// <inheritdoc />
    public MethodBase Original { get; } = original;

    /// <inheritdoc />
    public MethodInfo Patch { get; } = patch;

    /// <inheritdoc />
    public PatchType Type { get; } = patchType;
}