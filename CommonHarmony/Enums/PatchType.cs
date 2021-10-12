namespace CommonHarmony.Enums
{
    /// <summary>
    ///     Tye type of patch to apply.
    /// </summary>ary>
    public enum PatchType
    { 
        /// <summary>
        ///     Patches before the existing method.
        /// </summary>
        Prefix,

        /// <summary>
        ///     Patches after the existing method.
        /// </summary>
        Postfix,

        /// <summary>
        ///     Transpiles the existing method.
        /// </summary>
        Transpiler,
    }
}