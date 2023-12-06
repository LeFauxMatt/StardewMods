namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Objects;

/// <summary>Expand the capacity of chests and add scrolling to access extra items.</summary>
internal sealed class ResizeChest : BaseFeature
{
    private static readonly MethodBase ChestGetActualCapacity = AccessTools.Method(
        typeof(Chest),
        nameof(Chest.GetActualCapacity));

    private readonly Harmony harmony;

    /// <summary>Initializes a new instance of the <see cref="ResizeChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    public ResizeChest(IMonitor monitor, ModConfig config, Harmony harmony)
        : base(monitor, nameof(ResizeChest), () => config.ResizeChest is not FeatureOption.Disabled) =>
        this.harmony = harmony;

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
        if (!StorageHandler.TryGetOne(__instance, out var storage)
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
