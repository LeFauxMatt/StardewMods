using System;
using System.Diagnostics.CodeAnalysis;
using ExpandedStorage.Framework.Extensions;
using Harmony;
using StardewModdingAPI;
using StardewValley;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class DebrisPatch : HarmonyPatch
    {
        private readonly Type _type = typeof(Debris);
        internal DebrisPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowVacuumItems)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(Debris.collect)),
                    new HarmonyMethod(GetType(), nameof(collect_Prefix)));
            }
        }

        /// <summary>Collect debris directly into carried chest.</summary>
        public static bool collect_Prefix(Debris __instance, ref bool __result, Farmer farmer, Chunk chunk)
        {
            if (chunk == null
                || __instance.Chunks.Count <= 0
                || __instance.debrisType.Value.Equals(Debris.DebrisType.ARCHAEOLOGY)
                || __instance.item is not { } item
                || item.specialItem)
                return true;
            
            __instance.item = farmer.AddItemToInventory(item);
            if (__instance.item != null)
                return true;
            
            __result = true;
            return false;
        }
    }
}