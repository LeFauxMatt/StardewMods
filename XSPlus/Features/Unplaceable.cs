using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using SObject = StardewValley.Object;

namespace XSPlus.Features
{
    internal class Unplaceable : FeatureWithParam<bool>
    {
        private static Unplaceable _feature;
        public Unplaceable(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(Unplaceable), nameof(Unplaceable.Object_placementAction_prefix))
            );
        }
        protected override void DisableFeature()
        {
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                patch: AccessTools.Method(typeof(Unplaceable), nameof(Unplaceable.Object_placementAction_prefix))
            );
        }
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [HarmonyPriority(Priority.High)]
        private static bool Object_placementAction_prefix(SObject __instance, ref bool __result)
        {
            if (!_feature.IsEnabled(__instance))
                return true;
            __result = false;
            return false;
        }
    }
}