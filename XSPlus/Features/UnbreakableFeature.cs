namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class UnbreakableFeature : FeatureWithParam<bool>
    {
        private static UnbreakableFeature Instance;

        /// <summary>Initializes a new instance of the <see cref="UnbreakableFeature"/> class.</summary>
        public UnbreakableFeature()
            : base("Unbreakable")
        {
            UnbreakableFeature.Instance = this;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                prefix: new HarmonyMethod(typeof(UnbreakableFeature), nameof(UnbreakableFeature.Chest_performToolAction_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                patch: AccessTools.Method(typeof(UnbreakableFeature), nameof(UnbreakableFeature.Chest_performToolAction_prefix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_performToolAction_prefix(Chest __instance, ref bool __result)
        {
            if (!UnbreakableFeature.Instance.IsEnabledForItem(__instance))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}