using System;
using Harmony;
using StardewModdingAPI;
using StardewValley.Objects;

namespace ExpandedStorage.Framework
{
    internal class ChestPatches
    {
        private static IMonitor _monitor;
        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;

            if (!config.AllowModdedCapacity)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                prefix: new HarmonyMethod(typeof(ChestPatches), nameof(GetActualCapacity_Prefix)));
        }
        
        /// <summary>Returns modded capacity for storage.</summary>
        public static bool GetActualCapacity_Prefix(Chest __instance, ref int __result)
        {
            try
            {
                if (!__instance.modData.TryGetValue("ImJustMatt.ExpandedStorage/actual-capacity",
                    out var actualCapacity))
                    return true;
                __result = Convert.ToInt32(actualCapacity);
                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetActualCapacity_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
    }
}