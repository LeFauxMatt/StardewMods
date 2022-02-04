namespace XSPlus.Features;

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class UnplaceableFeature : FeatureWithParam<bool>
{
    private static UnplaceableFeature Instance;
    private HarmonyHelper _harmony;

    private UnplaceableFeature(ServiceLocator serviceLocator)
        : base("Unplaceable", serviceLocator)
    {
        // Init
        UnplaceableFeature.Instance ??= this;

        // Dependencies
        this.AddDependency<HarmonyHelper>(
            service =>
            {
                // Init
                this._harmony = service as HarmonyHelper;

                // Patches
                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                    typeof(UnplaceableFeature),
                    nameof(UnplaceableFeature.Object_placementAction_prefix));
            });
    }

    /// <inheritdoc />
    public override void Activate()
    {
        // Patches
        this._harmony.ApplyPatches(this.ServiceName);
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        // Patches
        this._harmony.UnapplyPatches(this.ServiceName);
    }

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