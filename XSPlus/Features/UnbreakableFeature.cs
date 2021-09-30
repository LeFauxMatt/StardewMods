namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class UnbreakableFeature : FeatureWithParam<bool>
    {
        private MixInfo _performToolActionPatch;

        private UnbreakableFeature()
            : base("Unbreakable")
        {
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="UnbreakableFeature"/>.
        /// </summary>
        private static UnbreakableFeature Instance { get; set; }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Patches
            this._performToolActionPatch = Mixin.Prefix(
                AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                typeof(UnbreakableFeature),
                nameof(UnbreakableFeature.Chest_performToolAction_prefix));
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Patches
            Mixin.Unpatch(this._performToolActionPatch);
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="UnbreakableFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="UnbreakableFeature"/> class.</returns>
        public static UnbreakableFeature GetSingleton(ServiceManager serviceManager)
        {
            return UnbreakableFeature.Instance ??= new UnbreakableFeature();
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