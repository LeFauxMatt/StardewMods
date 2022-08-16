namespace StardewMods.CommonHarmony.Enums;

/// <summary>
///     The type of Harmony Patch.
/// </summary>
internal enum PatchType
{
    /// <summary>Patches before the existing method.</summary>
    Prefix,

    /// <summary>Patches after the existing method.</summary>
    Postfix,

    /// <summary>Transpiles the existing method.</summary>
    Transpiler,

    /// <summary>Reverse patch an existing method.</summary>
    Reverse,
}