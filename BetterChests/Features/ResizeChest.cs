namespace StardewMods.BetterChests.Features;

using HarmonyLib;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Objects;

/// <summary>
///     Expand the capacity of chests and add scrolling to access extra items.
/// </summary>
internal sealed class ResizeChest : IFeature
{
    private const string Id = "furyx639.BetterChests/ResizeChest";

    private static ResizeChest? Instance;

    private bool _isActivated;

    private ResizeChest()
    {
        HarmonyHelper.AddPatches(
            ResizeChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    typeof(ResizeChest),
                    nameof(ResizeChest.Chest_GetActualCapacity_postfix),
                    PatchType.Postfix),
            });
    }

    /// <summary>
    ///     Initializes <see cref="ResizeChest" />.
    /// </summary>
    /// <returns>Returns an instance of the <see cref="ResizeChest" /> class.</returns>
    public static ResizeChest Init()
    {
        return ResizeChest.Instance ??= new();
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(ResizeChest.Id);
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(ResizeChest.Id);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (!Storages.TryGetOne(__instance, out var storage)
         || storage.ResizeChest is not FeatureOption.Enabled
         || storage.ResizeChestCapacity == 0)
        {
            return;
        }

        __result = storage.ActualCapacity;
    }
}