namespace CommonHarmony.Models
{
    using System;
    using System.Reflection;
    using Enums;
    using HarmonyLib;

    /// <summary>
    ///     Stores info about Harmony patches.
    /// </summary>
    internal class SavedPatch
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SavedPatch" /> class.
        /// </summary>
        /// <param name="original">The original method to patch.</param>
        /// <param name="type">The class containing the Harmony patch.</param>
        /// <param name="name">The name of the method to patch with.</param>
        /// <param name="patchType">The type of patch to apply.</param>
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
        public MethodBase Original { get; }

        /// <summary>
        ///     Gets the HarmonyMethod to patch with.
        /// </summary>
        public HarmonyMethod Patch
        {
            get => new(this.Type, this.Name);
        }

        /// <summary>
        ///     Gets the method of the patch.
        /// </summary>
        public MethodInfo Method
        {
            get => AccessTools.Method(this.Type, this.Name);
        }

        public PatchType PatchType { get; }

        /// <summary>
        ///     Gets the class containing the Harmony patch.
        /// </summary>
        private Type Type { get; }

        /// <summary>
        ///     Gets the name of the method to patch with.
        /// </summary>
        private string Name { get; }
    }
}