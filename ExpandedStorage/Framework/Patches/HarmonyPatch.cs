using System.Diagnostics.CodeAnalysis;
using Harmony;
using StardewModdingAPI;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class HarmonyPatch
    {
        internal static IMonitor Monitor;
        internal static ModConfig Config;
        internal HarmonyPatch(IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
        }
        protected internal abstract void Apply(HarmonyInstance harmony);
    }
}