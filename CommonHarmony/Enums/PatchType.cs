namespace CommonHarmony.Enums;

internal enum PatchType
{
    /// <summary>Patches before the existing method.</summary>
    Prefix,

    /// <summary>Patches after the existing method.</summary>
    Postfix,

    /// <summary>Transpiles the existing method.</summary>
    Transpiler,
}