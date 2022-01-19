namespace Common.Helpers;

using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// 
/// </summary>
internal static class ReflectionHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Assembly GetAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName?.StartsWith($"{name},") == true);
    }
}