using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using StardewModdingAPI;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ChestPatches : HarmonyPatch
    {
        private readonly Type _chestType = typeof(Chest);
        private static int _vanillaCapacity;

        internal ChestPatches(IMonitor monitor, ModConfig config)
            : base(monitor, config)
        {
            _vanillaCapacity = Config.ExpandVanillaChests ? 72 : Chest.capacity;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (!Config.AllowModdedCapacity)
                return;
            
            harmony.Patch(AccessTools.Method(_chestType, nameof(Chest.GetActualCapacity)),
                prefix: new HarmonyMethod(GetType(), nameof(GetActualCapacity_Prefix)));
        }
        
        /// <summary>Returns modded capacity for storage.</summary>
        public static bool GetActualCapacity_Prefix(Chest __instance, ref int __result)
        {
            var config = ExpandedStorage.GetConfig(__instance.DisplayName);
            if (config == null)
            {
                if (!Config.ExpandVanillaChests || __instance.SpecialChestType != Chest.SpecialChestTypes.None)
                    return true;
                __result = _vanillaCapacity;
                return false;
            }

            __result = config.Capacity switch
            {
                -1 => int.MaxValue,
                0 => _vanillaCapacity,
                _ => config.Capacity
            };
            return false;
        }
    }
}