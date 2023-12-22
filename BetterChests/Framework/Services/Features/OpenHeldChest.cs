namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Allows a chest to be opened while in the farmer's inventory.</summary>
internal sealed class OpenHeldChest : BaseFeature
{
    private static readonly MethodBase ChestAddItem = AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem));

    private static readonly MethodBase InventoryMenuHighlightAllItems = AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.highlightAllItems));

    private readonly ContainerFactory containers;

    private readonly IModEvents events;

    private readonly Harmony harmony;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="OpenHeldChest" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="containers">Dependency used for accessing containers.</param>
    public OpenHeldChest(ILogging logging, ModConfig modConfig, IModEvents events, Harmony harmony, IInputHelper input, ContainerFactory containers)
        : base(logging, modConfig)
    {
        this.events = events;
        this.harmony = harmony;
        this.input = input;
        this.containers = containers;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.Default.OpenHeldChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this.harmony.Patch(OpenHeldChest.ChestAddItem, new HarmonyMethod(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));

        this.harmony.Patch(OpenHeldChest.InventoryMenuHighlightAllItems, postfix: new HarmonyMethod(typeof(OpenHeldChest), nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this.harmony.Unpatch(OpenHeldChest.ChestAddItem, AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));

        this.harmony.Unpatch(OpenHeldChest.InventoryMenuHighlightAllItems, AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix)));
    }

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (__instance != item)
        {
            return true;
        }

        __result = item;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_highlightAllItems_postfix(ref bool __result, Item i)
    {
        if (!__result || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        __result = itemGrabMenu.context != i;
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsActionButton()
            || Game1.player.CurrentItem is null
            || !this.containers.TryGetOne(Game1.player.CurrentItem, out var storage)
            || storage.Options.OpenHeldChest != FeatureOption.Enabled
            || storage is not ChestContainer chestStorage)
        {
            return;
        }

        Game1.player.currentLocation.localSound("openChest");
        chestStorage.Chest.ShowMenu();
        this.input.Suppress(e.Button);
    }
}
