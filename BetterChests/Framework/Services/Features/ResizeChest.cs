namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Objects;

/// <summary>Expand the capacity of chests and add scrolling to access extra items.</summary>
internal sealed class ResizeChest : BaseFeature
{
#nullable disable
    private static ResizeChest instance;
#nullable enable

    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;

    /// <summary>Initializes a new instance of the <see cref="ResizeChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    public ResizeChest(ILog log, ModConfig modConfig, ContainerFactory containerFactory, Harmony harmony)
        : base(log, modConfig)
    {
        ResizeChest.instance = this;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.ResizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() =>
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)),
            postfix: new HarmonyMethod(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    /// <inheritdoc />
    protected override void Deactivate() =>
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)),
            AccessTools.DeclaredMethod(typeof(ResizeChest), nameof(ResizeChest.Chest_GetActualCapacity_postfix)));

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (!ResizeChest.instance.containerFactory.TryGetOne(__instance, out var container))
        {
            return;
        }

        __result = Math.Max(
                container.Options.ResizeChestCapacity,
                __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count) switch
            {
                > 36 => 70, > 9 => 36, _ => 9,
            };
    }
}