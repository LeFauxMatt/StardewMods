namespace StardewMods.CommonHarmony.Models;

using System;
using System.Reflection;
using HarmonyLib;
using StardewMods.CommonHarmony.Enums;

/// <summary>
///     Stores info about Harmony patches.
/// </summary>
internal sealed class SavedPatch
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SavedPatch" /> class.
    /// </summary>
    /// <param name="original">The original method/constructor.</param>
    /// <param name="type">The patch class/type.</param>
    /// <param name="name">The patch method name.</param>
    /// <param name="patchType">One of postfix, prefix, or transpiler.</param>
    public SavedPatch(MethodBase original, Type type, string name, PatchType patchType)
    {
        this.Original = original;
        this.Type = type;
        this.Name = name;
        this.PatchType = patchType;
    }

    /// <summary>
    ///     Gets the original method to patch.
    /// </summary>
    public MethodInfo Method => AccessTools.Method(this.Type, this.Name);

    /// <summary>
    ///     Gets the patch method name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the original method/constructor.
    /// </summary>
    public MethodBase Original { get; }

    /// <summary>
    ///     Gets the HarmonyMethod to patch with.
    /// </summary>
    public HarmonyMethod Patch => new(this.Type, this.Name);

    /// <summary>
    ///     Gets whether is patch is a postfix, prefix, or transpiler.
    /// </summary>
    public PatchType PatchType { get; }

    /// <summary>
    ///     Gets the patch class/type.
    /// </summary>
    public Type Type { get; }
}