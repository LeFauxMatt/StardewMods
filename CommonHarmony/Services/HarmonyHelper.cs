namespace CommonHarmony.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Common.Helpers;
using CommonHarmony.Enums;
using CommonHarmony.Models;
using HarmonyLib;
using StardewModdingAPI;

/// <summary>
///     Saves a list of <see cref="SavedPatch" /> which can be applied or reversed at any time.
/// </summary>
internal class HarmonyHelper
{
    private readonly IDictionary<string, Harmony> _harmony = new Dictionary<string, Harmony>();
    private readonly IDictionary<string, List<SavedPatch>> _savedPatches = new Dictionary<string, List<SavedPatch>>();

    /// <summary>
    ///     Adds a <see cref="SavedPatch" /> to an id.
    /// </summary>
    /// <param name="id">
    ///     The id should concatenate your Mod Unique ID from the <see cref="IManifest" /> and a group id for the
    ///     patches.
    /// </param>
    /// <param name="original">The original method/constructor.</param>
    /// <param name="type">The patch class/type.</param>
    /// <param name="name">The patch method name.</param>
    /// <param name="patchType">One of postfix, prefix, or transpiler.</param>
    public void AddPatch(string id, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix)
    {
        this.AddPatches(
            id,
            new[]
            {
                new SavedPatch(original, type, name, patchType),
            });
    }

    /// <summary>
    ///     Adds multiple <see cref="SavedPatch" /> to an id.
    /// </summary>
    /// <param name="id">
    ///     The id should concatenate your Mod Unique ID from the <see cref="IManifest" /> and a group id for the
    ///     patches.
    /// </param>
    /// <param name="patches">A list of <see cref="SavedPatch" /> to add to this group of patches.</param>
    public void AddPatches(string id, IEnumerable<SavedPatch> patches)
    {
        if (!this._savedPatches.TryGetValue(id, out var savedPatches))
        {
            savedPatches = new();
            this._savedPatches.Add(id, savedPatches);
        }

        savedPatches.AddRange(patches);
    }

    /// <summary>
    ///     Applies all <see cref="SavedPatch" /> added to an id.
    /// </summary>
    /// <param name="id">The id that the patches were added to.</param>
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
            try
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
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append($"This mod failed in {patch.Method.Name}");
                if (patch.Method.DeclaringType?.Name is not null)
                {
                    sb.Append($" of {patch.Method.DeclaringType.Name}. Technical details:\n");
                }

                sb.Append(ex.Message);
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                if (frame?.GetFileName() is { } fileName)
                {
                    var line = frame.GetFileLineNumber().ToString();
                    sb.Append($" at {fileName}:line {line}");
                }

                Log.Error(sb.ToString());
            }
        }
    }

    /// <summary>
    ///     Reverses all <see cref="SavedPatch" /> added to an id.
    /// </summary>
    /// <param name="id">The id that the patches were added to.</param>
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