using System;
using StardewModdingAPI;

namespace CommonHarmony
{
    internal class Patcher
    {
        private readonly IMod _mod;
        private readonly string _uniqueId;

        internal Patcher(IMod mod)
        {
            _mod = mod;
            _uniqueId = mod.ModManifest.UniqueID;
        }

        internal void ApplyAll(params Type[] patchTypes)
        {
            var harmony = new HarmonyLib.Harmony(_uniqueId);
            foreach (var patchType in patchTypes)
            {
                Activator.CreateInstance(patchType, _mod, harmony);
            }
        }
    }
}