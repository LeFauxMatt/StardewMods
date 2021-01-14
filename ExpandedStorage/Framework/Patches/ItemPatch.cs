using System;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    internal class ItemPatch : HarmonyPatch
    {
        private readonly Type _itemType = typeof(Item);
        internal ItemPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (!Config.AllowCarryingChests)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(_itemType, nameof(Item.canStackWith), new []{typeof(ISalable)}),
                prefix: new HarmonyMethod(GetType(), nameof(canStackWith_Prefix)));
        }

        /// <summary>Disallow chests containing items to be stacked.</summary>
        public static bool canStackWith_Prefix(Item __instance, ISalable other, ref bool __result)
        {
            
            if (__instance.ParentSheetIndex != 130 && !ExpandedStorage.HasConfig(__instance.Name))
                return true;
            if ((!(__instance is Chest chest) || chest.items.Count == 0) &&
                (!(other is Chest otherChest) || otherChest.items.Count == 0))
                return true;
            __result = false;
            return false;
        }
    }
}