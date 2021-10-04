namespace CommonHarmony.Services
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using StardewModdingAPI;

    /// <summary>
    ///     Provides Harmony Patching across mods.
    /// </summary>
    internal static class Mixin
    {
        /// <inheritdoc cref="Harmony" />
        private static Harmony Harmony { get; set; } = null!;

        /// <summary>Initializes the <see cref="Mixin" /> class.</summary>
        /// <param name="manifest">The ModManifest to create the harmony instance with.</param>
        public static void Init(IManifest manifest)
        {
            Mixin.Harmony = new(manifest.UniqueID);
        }

        public static MixInfo Prefix(MethodBase original, Type type, string name)
        {
            var patchInfo = new MixInfo(original, type, name);
            Mixin.Harmony.Patch(patchInfo.Original, patchInfo.Patch);
            return patchInfo;
        }

        public static MixInfo Postfix(MethodBase original, Type type, string name)
        {
            var patchInfo = new MixInfo(original, type, name);
            Mixin.Harmony.Patch(patchInfo.Original, postfix: patchInfo.Patch);
            return patchInfo;
        }

        public static MixInfo Transpiler(MethodBase original, Type type, string name)
        {
            var patchInfo = new MixInfo(original, type, name);
            Mixin.Harmony.Patch(patchInfo.Original, transpiler: patchInfo.Patch);
            return patchInfo;
        }

        public static void Unpatch(MixInfo mixInfo)
        {
            Mixin.Harmony.Unpatch(mixInfo.Original, mixInfo.Method);
        }
    }
}