using System;
using Harmony;
using StardewModdingAPI;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
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
            if (!ExpandedStorage.Objects.TryGetValue(__instance.ParentSheetIndex, out var data))
                return true;
            __result = data.Capacity switch
            {
                -1 => int.MaxValue,
                0 => 36,
                _ => data.Capacity
            };
            return false;
        }
    }
}