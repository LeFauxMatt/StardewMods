namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Services;
using HarmonyLib;
using Models;
using StardewModdingAPI;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;

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
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    typeof(ResizeChest),
                    nameof(ResizeChest.Chest_GetActualCapacity_postfix),
                    PatchType.Postfix);
            });
    }

    private static ResizeChest Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
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