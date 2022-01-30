namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Reflection;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using HarmonyLib;

/// <inheritdoc cref="FuryCore.Interfaces.IHarmonyHelper" />
[FuryCoreService(true)]
internal class HarmonyHelper : IHarmonyHelper, IModService
{
    private readonly IDictionary<string, Harmony> _harmony = new Dictionary<string, Harmony>();
    private readonly IDictionary<string, List<SavedPatch>> _savedPatches = new Dictionary<string, List<SavedPatch>>();

    /// <inheritdoc />
    public void AddPatch(string id, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix)
    {
        this.AddPatches(
            id,
            new[]
            {
                new SavedPatch(original, type, name, patchType),
            });
    }

    /// <inheritdoc />
    public void AddPatches(string id, IEnumerable<SavedPatch> patches)
    {
        if (!this._savedPatches.TryGetValue(id, out var savedPatches))
        {
            savedPatches = new();
            this._savedPatches.Add(id, savedPatches);
        }

        savedPatches.AddRange(patches);
    }

    /// <inheritdoc />
    public void ApplyPatches(string id)
    {
        if (!this._savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        if (!this._harmony.TryGetValue(id, out var harmony))
        {
            harmony = new(id);
            this._harmony.Add(id, harmony);
        }

        foreach (var patch in patches)
        {
            switch (patch.PatchType)
            {
                case PatchType.Prefix:
                    harmony.Patch(patch.Original, patch.Patch);
                    break;
                case PatchType.Postfix:
                    harmony.Patch(patch.Original, postfix: patch.Patch);
                    break;
                case PatchType.Transpiler:
                    harmony.Patch(patch.Original, transpiler: patch.Patch);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Failed to patch {nameof(patch.Type)}.{patch.Name}");
            }
        }
    }

    /// <inheritdoc />
    public void UnapplyPatches(string id)
    {
        if (!this._savedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        if (!this._harmony.TryGetValue(id, out var harmony))
        {
            harmony = new(id);
            this._harmony.Add(id, harmony);
        }

        foreach (var patch in patches)
        {
            harmony.Unpatch(patch.Original, patch.Method);
        }
    }
}