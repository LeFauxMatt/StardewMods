using StardewModdingAPI;

namespace CommonHarmony
{
    internal abstract class BasePatch<T> where T : IMod
    {
        private protected static T Mod;
        private protected static IMonitor Monitor => Mod.Monitor;
        internal BasePatch(IMod mod, HarmonyLib.Harmony harmony)
        {
            Mod = (T) mod;
        }
    }
}