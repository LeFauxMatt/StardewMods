namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using FuryCore.Enums;
using FuryCore.Services;
using HarmonyLib;
using Models;
using StardewModdingAPI;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public ResizeChest(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        ResizeChest.Instance = this;
        this._harmony = services.Lazy<HarmonyHelper>(ResizeChest.AddPatches);
    }

    private static ResizeChest Instance { get; set; }

    private HarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(ResizeChest));
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(ResizeChest));
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        harmony.AddPatch(
            nameof(ResizeChest),
            AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
            typeof(ResizeChest),
            nameof(ResizeChest.Chest_GetActualCapacity_postfix),
            PatchType.Postfix);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (ResizeChest.Instance.ManagedChests.FindChest(__instance, out var managedChest) && managedChest.Config.Capacity != 0)
        {
            __result = managedChest.Config.Capacity > 0
                ? managedChest.Config.Capacity
                : int.MaxValue;
        }
    }
}