namespace StardewMods.Common.Helpers.AtraBase;

using System.Runtime.CompilerServices;

/// <summary>
/// A class that contains useful constants.
/// </summary>
public class TKConstants
{
    /// <summary>
    /// For use when asking the compiler to both inline and aggressively optimize.
    /// </summary>
    public const MethodImplOptions Hot = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;
}