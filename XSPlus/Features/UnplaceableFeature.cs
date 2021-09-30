namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using SObject = StardewValley.Object;

    /// <inheritdoc />
    internal class UnplaceableFeature : FeatureWithParam<bool>
    {
        private MixInfo _placementActionPatch;

        private UnplaceableFeature(ModConfigService modConfigService)
            : base("Unplaceable", modConfigService)
        {
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="UnplaceableFeature"/>.
        /// </summary>
        private static UnplaceableFeature Instance { get; set; }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="UnplaceableFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="UnplaceableFeature"/> class.</returns>
        public static UnplaceableFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            return UnplaceableFeature.Instance ??= new UnplaceableFeature(modConfigService);
        }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Patches
            this._placementActionPatch = Mixin.Prefix(
                AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                typeof(UnplaceableFeature),
                nameof(UnplaceableFeature.Object_placementAction_prefix));
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Patches
            Mixin.Unpatch(this._placementActionPatch);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Object_placementAction_prefix(SObject __instance, ref bool __result)
        {
            if (!UnplaceableFeature.Instance.IsEnabledForItem(__instance))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }
}