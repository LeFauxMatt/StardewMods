namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Reflection;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using HarmonyLib;
using StardewModdingAPI;

/// <inheritdoc />
public class HarmonyHelper : IService
{
    private readonly Harmony _harmony;
    private readonly Dictionary<string, List<SavedPatch>> _savedPatches = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HarmonyHelper"/> class.
    /// </summary>
    /// <param name="manifest"></param>
    public HarmonyHelper(IManifest manifest)
    {
        this._harmony = new(manifest.UniqueID);
    }

    public void AddPatch(string group, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix)
    {
        this.AddPatches(group,  new [] { new SavedPatch(original, type, name, patchType) });
    }

    public void AddPatches(string group, IEnumerable<SavedPatch> patches)
    {
        if (!this._savedPatches.TryGetValue(group, out var savedPatches))
        {
            savedPatches = new();
            this._savedPatches.Add(group, savedPatches);
        }

        savedPatches.AddRange(patches);
    }

    public void ApplyPatches(string group)
    {
        if (this._savedPatches.TryGetValue(group, out var patches))
        {
            foreach (var patch in patches)
            {
                switch (patch.PatchType)
                {
                    case PatchType.Prefix:
                        this._harmony.Patch(patch.Original, patch.Patch);
                        break;
                    case PatchType.Postfix:
                        this._harmony.Patch(patch.Original, postfix: patch.Patch);
                        break;
                    case PatchType.Transpiler:
                        this._harmony.Patch(patch.Original, transpiler: patch.Patch);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void UnapplyPatches(string group)
    {
        if (this._savedPatches.TryGetValue(group, out var patches))
        {
            foreach (var patch in patches)
            {
                this._harmony.Unpatch(patch.Original, patch.Method);
            }
        }
    }
}