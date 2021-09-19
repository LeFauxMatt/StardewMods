namespace XSPlus.Features
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class Capacity : FeatureWithParam<int>
    {
        private static Capacity Instance;
        private readonly Func<int> _getConfigCapacity;

        /// <summary>Initializes a new instance of the <see cref="Capacity"/> class.</summary>
        /// <param name="getConfigCapacity">Get method for configured default capacity.</param>
        public Capacity(Func<int> getConfigCapacity)
            : base("Capacity")
        {
            Capacity.Instance = this;
            this._getConfigCapacity = getConfigCapacity;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                postfix: new HarmonyMethod(typeof(Capacity), nameof(Capacity.Chest_GetActualCapacity_postfix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                patch: AccessTools.Method(typeof(Capacity), nameof(Capacity.Chest_GetActualCapacity_postfix)));
        }

        /// <inheritdoc/>
        protected override bool TryGetValueForItem(Item item, out int param)
        {
            if (base.TryGetValueForItem(item, out param))
            {
                return true;
            }

            param = this._getConfigCapacity();
            return param == 0;
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
        {
            if (!Capacity.Instance.IsEnabledForItem(__instance) || !Capacity.Instance.TryGetValueForItem(__instance, out int capacity))
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