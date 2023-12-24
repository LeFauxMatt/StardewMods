namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>Allows a placed chest full of items to be picked up by the farmer.</summary>
internal sealed class CarryChest : BaseFeature
{
#nullable disable
    private static CarryChest instance;
#nullable enable

    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ProxyChestFactory proxyChestFactory;
    private readonly StatusEffectManager statusEffectManager;

    /// <summary>Initializes a new instance of the <see cref="CarryChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    /// <param name="statusEffectManager">Dependency used for adding and removing custom buffs.</param>
    public CarryChest(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        Harmony harmony,
        IInputHelper inputHelper,
        IModEvents modEvents,
        ProxyChestFactory proxyChestFactory,
        StatusEffectManager statusEffectManager)
        : base(log, modConfig)
    {
        CarryChest.instance = this;
        this.modEvents = modEvents;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.proxyChestFactory = proxyChestFactory;
        this.statusEffectManager = statusEffectManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.CarryChest != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
            postfix: new HarmonyMethod(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.GameLoop.OneSecondUpdateTicked -= this.OnOneSecondUpdateTicked;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
            AccessTools.DeclaredMethod(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        GameLocation location,
        int x,
        int y,
        ref bool __result)
    {
        if (!__result
            || !CarryChest.instance.proxyChestFactory.TryGetProxy(__instance, out var chest)
            || !location.Objects.TryGetValue(
                new Vector2((int)(x / (float)Game1.tileSize), (int)(y / (float)Game1.tileSize)),
                out var obj)
            || obj is not Chest placedChest)
        {
            return;
        }

        // Copy data from chest
        placedChest.GlobalInventoryId = chest.GlobalInventoryId;
        placedChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
        foreach (var (key, value) in chest.modData.Pairs)
        {
            placedChest.modData[key] = value;
        }

        // Restore proxy
        CarryChest.instance.proxyChestFactory.TryRestoreProxy(placedChest);
    }

    private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (this.ModConfig.CarryChestSlowLimit == 0)
        {
            return;
        }

        if (Game1.player.Items.Count(this.proxyChestFactory.IsProxy) >= this.ModConfig.CarryChestSlowLimit)
        {
            this.statusEffectManager.AddEffect(StatusEffect.Overburdened);
            return;
        }

        if (this.statusEffectManager.HasEffect(StatusEffect.Overburdened))
        {
            this.statusEffectManager.RemoveEffect(StatusEffect.Overburdened);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || Game1.player.CurrentItem is Tool
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || (Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || obj is not Chest chest
            || !this.containerFactory.TryGetOne(obj, out var container)
            || container.Options.CarryChest != Option.Enabled)
        {
            return;
        }

        // Check carrying limits
        if (this.ModConfig.CarryChestLimit > 0
            && Game1.player.Items.Count(this.proxyChestFactory.IsProxy) >= this.ModConfig.CarryChestLimit)
        {
            Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
            this.inputHelper.Suppress(e.Button);
            return;
        }

        // Try to create proxy item
        if (!this.proxyChestFactory.TryCreateRequest(chest, out var request))
        {
            return;
        }

        // Try to add to inventory
        if (!Game1.player.addItemToInventoryBool(request.Item, true))
        {
            request.Cancel();
            return;
        }

        // Remove chest from world
        this.Log.Trace(
            "{0}: Grabbed chest from {1} at ({2}, {3})",
            this.Id,
            Game1.player.currentLocation.Name,
            pos.X,
            pos.Y);

        request.Confirm();
        Game1.currentLocation.Objects.Remove(pos);
        Game1.playSound("pickUpItem");
        this.inputHelper.Suppress(e.Button);
    }
}