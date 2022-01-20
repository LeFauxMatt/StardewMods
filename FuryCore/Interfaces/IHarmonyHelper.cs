namespace FuryCore.Interfaces;

using System;
using System.Collections.Generic;
using System.Reflection;
using FuryCore.Enums;
using FuryCore.Models;

public interface IHarmonyHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="original"></param>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <param name="patchType"></param>
    public void AddPatch(string id, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="patches"></param>
    public void AddPatches(string id, IEnumerable<SavedPatch> patches);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void ApplyPatches(string id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void UnapplyPatches(string id);
}