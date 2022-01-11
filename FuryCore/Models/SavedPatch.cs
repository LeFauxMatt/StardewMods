namespace FuryCore.Models;

using System;
using System.Reflection;
using FuryCore.Enums;
using HarmonyLib;

/// <summary>
/// Stores info about Harmony patches.
/// </summary>
public class SavedPatch
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SavedPatch"/> class.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="patchType"></param>
    public SavedPatch(MethodBase original, Type type, string name, PatchType patchType)
    {
        this.Original = original;
        this.Type = type;
        this.Name = name;
        this.PatchType = patchType;
    }

    public MethodBase Original { get; }

    public Type Type { get; }

    public string Name { get; }

    public PatchType PatchType { get; }

    /// <summary>
    ///     Gets the HarmonyMethod to patch with.
    /// </summary>
    public HarmonyMethod Patch
    {
        get => new(this.Type, this.Name);
    }

    /// <summary>
    ///     Gets the original method to patch.
    /// </summary>
    public MethodInfo Method
    {
        get => AccessTools.Method(this.Type, this.Name);
    }
}