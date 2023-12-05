namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Objects;

/// <summary>Expand the capacity of chests and add scrolling to access extra items.</summary>
internal sealed class ResizeChest : Feature
{
    private const string Id = "furyx639.BetterChests/ResizeChest";

    private static readonly MethodBase ChestGetActualCapacity = AccessTools.Method(
        typeof(Chest),
        nameof(Chest.GetActualCapacity));

#nullable disable
    private static Feature instance;
#nullable enable

    private readonly Harmony harmony;

    private ResizeChest() => this.harmony = new(ResizeChest.Id);

    /// <summary>Initializes <see cref="ResizeChest" />.</summary>
    /// <returns>Returns an instance of the <see cref="ResizeChest" /> class.</returns>
    public static Feature Init() => ResizeChest.instance ??= new ResizeChest();

    /// <inheritdoc />
    protected override void Activate() =>
        this.harmony.Patch(
            ResizeChest.ChestGetActualCapacity,
            postfix: new(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    /// <inheritdoc />
    protected override void Deactivate() =>
        this.harmony.Unpatch(
            ResizeChest.ChestGetActualCapacity,
            AccessTools.Method(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (!Storages.TryGetOne(__instance, out var storage)
            || storage is not
            {
                Data: Storage storageObject,
                ResizeChest: FeatureOption.Enabled,
                ResizeChestCapacity: not 0,
            })
        {
            return;
        }

        __result = storageObject.ActualCapacity;
    }
}
