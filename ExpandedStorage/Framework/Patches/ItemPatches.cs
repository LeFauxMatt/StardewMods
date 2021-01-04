using System;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    public class ItemPatches
    {
        private static IMonitor _monitor;
        
        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;

            if (!config.AllowCarryingChests)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Item), nameof(Item.canStackWith), new []{typeof(ISalable)}),
                prefix: new HarmonyMethod(typeof(ItemPatches), nameof(canStackWith_Prefix)));
        }

        /// <summary>Disallow chests containing items to be stacked.</summary>
        public static bool canStackWith_Prefix(Item __instance, ISalable other, ref bool __result)
        {
            if (__instance.ParentSheetIndex != 130 &&
                !ExpandedStorage.Objects.ContainsKey(__instance.ParentSheetIndex))
                return true;
            if ((!(__instance is Chest chest) || chest.items.Count == 0) &&
                (!(other is Chest otherChest) || otherChest.items.Count == 0))
                return true;
            __result = false;
            return false;
        }
    }
}