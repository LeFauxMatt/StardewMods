namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using SObject = StardewValley.Object;

    /// <inheritdoc />
    internal class Unplaceable : FeatureWithParam<bool>
    {
        private static Unplaceable Instance;

        /// <summary>Initializes a new instance of the <see cref="Unplaceable"/> class.</summary>
        public Unplaceable()
            : base("Unplaceable")
        {
            Unplaceable.Instance = this;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(Unplaceable), nameof(Unplaceable.Object_placementAction_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                patch: AccessTools.Method(typeof(Unplaceable), nameof(Unplaceable.Object_placementAction_prefix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Object_placementAction_prefix(SObject __instance, ref bool __result)
        {
            if (!Unplaceable.Instance.IsEnabledForItem(__instance))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}