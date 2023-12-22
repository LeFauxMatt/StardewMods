namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>Allows a placed chest full of items to be picked up by the farmer.</summary>
internal sealed class CarryChest : BaseFeature
{
    private static readonly MethodBase ObjectDrawInMenu = AccessTools.DeclaredMethod(
        typeof(SObject),
        nameof(SObject.drawInMenu),
        new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) });

#nullable disable
    private static CarryChest instance;
#nullable enable

    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ProxyChestManager proxyChestManager;
    private readonly StatusEffectManager statusEffectManager;

    /// <summary>Initializes a new instance of the <see cref="CarryChest" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="proxyChestManager">Dependency used for creating virtualized chests.</param>
    /// <param name="statusEffectManager">Dependency used for adding and removing custom buffs.</param>
    public CarryChest(
        ILogging logging,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        Harmony harmony,
        IInputHelper inputHelper,
        IModEvents modEvents,
        ProxyChestManager proxyChestManager,
        StatusEffectManager statusEffectManager)
        : base(logging, modConfig)
    {
        CarryChest.instance = this;
        this.modEvents = modEvents;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.proxyChestManager = proxyChestManager;
        this.statusEffectManager = statusEffectManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.CarryChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;

        // Patches
        this.harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)), postfix: new HarmonyMethod(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.GameLoop.OneSecondUpdateTicked -= this.OnOneSecondUpdateTicked;

        // Patches
        this.harmony.Unpatch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)), AccessTools.DeclaredMethod(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_placementAction_postfix(SObject __instance, GameLocation location, int x, int y, ref bool __result)
    {
        if (!__result
            || !CarryChest.instance.proxyChestManager.TryGetProxy(__instance, out var chest)
            || !location.Objects.TryGetValue(new Vector2((int)(x / (float)Game1.tileSize), (int)(y / (float)Game1.tileSize)), out var obj)
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
        CarryChest.instance.proxyChestManager.TryRestoreProxy(placedChest);
    }

    private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (this.ModConfig.CarryChestSlowLimit == 0)
        {
            return;
        }

        if (Game1.player.Items.Count(this.proxyChestManager.IsProxy) < this.ModConfig.CarryChestSlowLimit)
        {
            this.statusEffectManager.RemoveEffect(StatusEffect.Overburdened);
            return;
        }

        this.statusEffectManager.AddEffect(StatusEffect.Overburdened);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || Game1.player.CurrentItem is Tool
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || obj is not Chest chest
            || !this.containerFactory.TryGetOne(obj, out var container)
            || container.Options.CarryChest != FeatureOption.Enabled)
        {
            return;
        }

        // Check carrying limits
        if (this.ModConfig.CarryChestLimit > 0 && Game1.player.Items.Count(this.proxyChestManager.IsProxy) >= this.ModConfig.CarryChestLimit)
        {
            Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
            this.inputHelper.Suppress(e.Button);
            return;
        }

        // Try to create proxy item
        if (!this.proxyChestManager.TryCreateRequest(chest, out var request))
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
        request.Confirm();
        Game1.currentLocation.Objects.Remove(pos);
        Game1.playSound("pickUpItem");
        this.inputHelper.Suppress(e.Button);
    }
}
