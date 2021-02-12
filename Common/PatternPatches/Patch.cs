using Harmony;
using StardewModdingAPI;

namespace Common.PatternPatches
{
    public abstract class Patch<T>
    {
        internal static IMonitor Monitor;
        internal static T Config;

        internal Patch(IMonitor monitor, T config)
        {
            Monitor = monitor;
            Config = config;
        }

        protected internal abstract void Apply(HarmonyInstance harmony);
    }
}