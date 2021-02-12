using Harmony;
using StardewModdingAPI;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global

namespace Common.PatternPatches
{
    public abstract class Patch<T>
    {
        private protected static IMonitor Monitor;
        private protected static T Config;

        internal Patch(IMonitor monitor, T config)
        {
            Monitor = monitor;
            Config = config;
        }

        protected internal abstract void Apply(HarmonyInstance harmony);
    }
}