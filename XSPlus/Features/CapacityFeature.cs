namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CapacityFeature : FeatureWithParam<int>
    {
        private readonly ModConfigService _modConfigService;
        private MixInfo _capacityPatch;

        private CapacityFeature(ModConfigService modConfigService)
            : base("Capacity", modConfigService)
        {
            this._modConfigService = modConfigService;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="CapacityFeature" />.
        /// </summary>
        private static CapacityFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="CapacityFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="CapacityFeature" /> class.</returns>
        public static async Task<CapacityFeature> Create(ServiceManager serviceManager)
        {
            return CapacityFeature.Instance ??= new(await serviceManager.Get<ModConfigService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Patches
            this._capacityPatch = Mixin.Postfix(
                AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                typeof(CapacityFeature),
                nameof(CapacityFeature.Chest_GetActualCapacity_postfix));
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Patches
            Mixin.Unpatch(this._capacityPatch);
        }

        /// <inheritdoc />
        protected override bool TryGetValueForItem(Item item, out int param)
        {
            if (base.TryGetValueForItem(item, out param))
            {
                return true;
            }

            param = this._modConfigService.ModConfig.Capacity;
            return param == 0;
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
        {
            if (!CapacityFeature.Instance.IsEnabledForItem(__instance) || !CapacityFeature.Instance.TryGetValueForItem(__instance, out var capacity))
            {
                return;
            }

            __result = capacity switch
            {
                -1 => int.MaxValue,
                > 0 => capacity,
                _ => __result,
            };
        }
    }
}