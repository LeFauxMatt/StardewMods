namespace StardewMods.Common.Helpers;

using System;
using System.Linq;
using System.Reflection;

/// <summary>
///     Helper for identifying an Assembly by its name.
/// </summary>
internal static class ReflectionHelper
{
    /// <summary>
    ///     Get the first Assembly that matches the name.
    /// </summary>
    /// <param name="name">The name of the assembly.</param>
    /// <returns>The first assembly that matches the name.</returns>
    public static Assembly? GetAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName?.StartsWith($"{name},") == true);
    }
}