using System;
using StardewModdingAPI;
using StardewValley.Objects;

namespace ExpandedStorage.Framework
{
    public class ChestPatches
    {
        private static IMonitor Monitor;
        public static void init(IMonitor monitor)
        {
            Monitor = monitor;
        }
        /// <summary>
        /// Returns modded capacity for storage.
        /// </summary>
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
                Monitor.Log($"Failed in {nameof(GetActualCapacity_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }
    }
}