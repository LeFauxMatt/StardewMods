namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Interfaces;
using StardewValley.Objects;

/// <summary>Expand the capacity of chests and add scrolling to access extra items.</summary>
internal sealed class ResizeChest : BaseFeature
{
    private readonly Harmony harmony;

    /// <summary>Initializes a new instance of the <see cref="ResizeChest" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    public ResizeChest(ILogging logging, ModConfig modConfig, Harmony harmony)
        : base(logging, modConfig) =>
        this.harmony = harmony;

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.Default.ResizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() =>
        this.harmony.Patch(AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)), postfix: new HarmonyMethod(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    /// <inheritdoc />
    protected override void Deactivate() =>
        this.harmony.Unpatch(AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)), AccessTools.DeclaredMethod(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result) { }
}
