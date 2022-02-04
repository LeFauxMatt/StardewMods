namespace StardewMods.FuryCore.Interfaces;

using System;
using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Models;

/// <summary>
///     Saves a list of <see cref="SavedPatch" /> which can be applied or reversed at any time.
/// </summary>
public interface IHarmonyHelper
{
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
    public void AddPatch(string id, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix);

    /// <summary>
    ///     Adds multiple <see cref="SavedPatch" /> to an id.
    /// </summary>
    /// <param name="id">
    ///     The id should concatenate your Mod Unique ID from the <see cref="IManifest" /> and a group id for the
    ///     patches.
    /// </param>
    /// <param name="patches">A list of <see cref="SavedPatch" /> to add to this group of patches.</param>
    public void AddPatches(string id, IEnumerable<SavedPatch> patches);

    /// <summary>
    ///     Applies all <see cref="SavedPatch" /> added to an id.
    /// </summary>
    /// <param name="id">The id that the patches were added to.</param>
    public void ApplyPatches(string id);

    /// <summary>
    ///     Reverses all <see cref="SavedPatch" /> added to an id.
    /// </summary>
    /// <param name="id">The id that the patches were added to.</param>
    public void UnapplyPatches(string id);
}