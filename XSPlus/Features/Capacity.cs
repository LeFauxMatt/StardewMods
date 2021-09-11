using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class Capacity : FeatureWithParam<int>
    {
        private static Capacity _feature;
        public Capacity(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                postfix: new HarmonyMethod(typeof(Capacity), nameof(Capacity.Chest_GetActualCapacity_postfix))
            );
        }
        protected override void DisableFeature()
        {
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                patch: AccessTools.Method(typeof(Capacity), nameof(Capacity.Chest_GetActualCapacity_postfix))
            );
        }
        private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
        {
            if (!_feature.IsEnabled(__instance) || XSPlus.Config.Capacity == 0)
                return;
            __result = XSPlus.Config.Capacity switch
            {
                -1 => int.MaxValue,
                > 0 => XSPlus.Config.Capacity,
                _ => __result
            };
        }
    }
}