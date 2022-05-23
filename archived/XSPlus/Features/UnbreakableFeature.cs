#nullable disable

namespace XSPlus.Features;

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewValley.Objects;

/// <inheritdoc />
internal class UnbreakableFeature : FeatureWithParam<bool>
{
    private static UnbreakableFeature Instance;
    private HarmonyHelper _harmony;

    private UnbreakableFeature(ServiceLocator serviceLocator)
        : base("Unbreakable", serviceLocator)
    {
        // Init
        UnbreakableFeature.Instance ??= this;

        // Dependencies
        this.AddDependency<HarmonyHelper>(
            service =>
            {
                // Init
                this._harmony = service as HarmonyHelper;

                // Patches
                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                    typeof(UnbreakableFeature),
                    nameof(UnbreakableFeature.Chest_performToolAction_prefix));
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