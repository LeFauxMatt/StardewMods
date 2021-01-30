using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

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

        /// <summary>Disallow chests containing items to be stacked.</summary>
        public static bool collect_Prefix(Debris __instance, ref bool __result, Farmer farmer, Chunk chunk)
        {
            if (chunk == null
                || __instance.Chunks.Count <= 0
                || __instance.debrisType.Value.Equals(Debris.DebrisType.ARCHAEOLOGY)
                || __instance.item is not { } item
                || item.specialItem)
                return true;
            
            // Find prioritized storage
            var storages = farmer.Items
                .Take(Config.VacuumToFirstRow ? 12 : farmer.Items.Count)
                .Where(i => i is Chest)
                .ToDictionary(i => i as Chest, ExpandedStorage.GetConfig)
                .Where(s =>
                    s.Value != null
                    && s.Value.VacuumItems
                    && s.Value.IsAllowed(item)
                    && !s.Value.IsBlocked(item))
                .Select(s => s.Key)
                .OrderByDescending(s => s.modData.TryGetValue("Pathoschild.ChestsAnywhere/Order", out var order) ? Convert.ToInt32(order) : 0);
            
            // Insert item into storage
            foreach (var storage in storages)
            {
                __instance.item = storage.addItem(item);
                if (__instance.item != null)
                    continue;
                __result = true;
                return false;
            }
            return true;
        }
    }
}