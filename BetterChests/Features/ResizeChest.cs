namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using BetterChests.Interfaces;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public ResizeChest(IConfigModel config, IModHelper helper, IServiceLocator services)
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
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (ResizeChest.Instance.ManagedChests.FindChest(__instance, out var managedChest) && managedChest.ResizeChestCapacity != 0)
        {
            __result = managedChest.ResizeChestCapacity > 0
                ? managedChest.ResizeChestCapacity
                : int.MaxValue;
        }
    }
}