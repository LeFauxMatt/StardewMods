using Harmony;
using StardewModdingAPI;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage.Framework.Patches
{
    public class ObjectPatches
    {
        private static IMonitor _monitor;
        
        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;

            if (!config.AllowCarryingChests)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(SDVObject), nameof(SDVObject.getDescription)),
                postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(getDescription_Postfix)));
        }

        /// <summary>Adds count of chests contents to its description.</summary>
        public static void getDescription_Postfix(SDVObject __instance, ref string __result)
        {
            if (!(__instance is Chest chest))
                return;
            if (chest.ParentSheetIndex == 130 || ExpandedStorage.Objects.ContainsKey(chest.ParentSheetIndex))
                __result += "\n" + $"Contains {chest.items?.Count ?? 0} items.";
        }
    }
}