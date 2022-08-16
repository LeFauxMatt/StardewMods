namespace StardewMods.CommonHarmony.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using HarmonyLib;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Models;

/// <summary>
///     Saves a list of <see cref="SavedPatch" /> which can be applied or reversed at any time.
/// </summary>
internal static class HarmonyHelper
{
    private static readonly IDictionary<string, Harmony> Instances = new Dictionary<string, Harmony>();

    private static readonly IDictionary<string, List<SavedPatch>> SavedPatches =
        new Dictionary<string, List<SavedPatch>>();

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
    public static void AddPatch(
        string id,
        MethodBase original,
        Type type,
        string name,
        PatchType patchType = PatchType.Prefix)
    {
        HarmonyHelper.AddPatches(id, new[] { new SavedPatch(original, type, name, patchType) });
    }

    /// <summary>
    ///     Adds multiple <see cref="SavedPatch" /> to an id.
    /// </summary>
    /// <param name="id">
    ///     The id should concatenate your Mod Unique ID from the <see cref="IManifest" /> and a group id for the
    ///     patches.
    /// </param>
    /// <param name="patches">A list of <see cref="SavedPatch" /> to add to this group of patches.</param>
    public static void AddPatches(string id, IEnumerable<SavedPatch> patches)
    {
        if (!HarmonyHelper.SavedPatches.TryGetValue(id, out var savedPatches))
        {
            savedPatches = new();
            HarmonyHelper.SavedPatches.Add(id, savedPatches);
        }

        savedPatches.AddRange(patches);
    }

    /// <summary>
    ///     Applies all <see cref="SavedPatch" /> added to an id.
    /// </summary>
    /// <param name="id">The id that the patches were added to.</param>
    public static void ApplyPatches(string id)
    {
        if (!HarmonyHelper.SavedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        if (!HarmonyHelper.Instances.TryGetValue(id, out var harmony))
        {
            harmony = new(id);
            HarmonyHelper.Instances.Add(id, harmony);
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
                    case PatchType.Reverse:
                        harmony.CreateReversePatcher(patch.Original, patch.Patch)
                               .Patch(HarmonyReversePatchType.Snapshot);
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
    public static void UnapplyPatches(string id)
    {
        if (!HarmonyHelper.SavedPatches.TryGetValue(id, out var patches))
        {
            return;
        }

        if (!HarmonyHelper.Instances.TryGetValue(id, out var harmony))
        {
            harmony = new(id);
            HarmonyHelper.Instances.Add(id, harmony);
        }

        foreach (var patch in patches)
        {
            harmony.Unpatch(patch.Original, patch.Method);
        }
    }
}