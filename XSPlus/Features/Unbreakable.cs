using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class Unbreakable : FeatureWithParam<bool>
    {
        private static Unbreakable _feature;
        public Unbreakable(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                prefix: new HarmonyMethod(typeof(Unbreakable), nameof(Unbreakable.Chest_performToolAction_prefix))
            );
        }
        protected override void DisableFeature()
        {
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                patch: AccessTools.Method(typeof(Unbreakable), nameof(Unbreakable.Chest_performToolAction_prefix))
            );
        }
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_performToolAction_prefix(Chest __instance, ref bool __result)
        {
            if (!_feature.IsEnabled(__instance))
                return true;
            __result = false;
            return false;
        }
    }
}