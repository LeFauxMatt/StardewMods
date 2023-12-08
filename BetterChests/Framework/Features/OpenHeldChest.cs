namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Allows a chest to be opened while in the farmer's inventory.</summary>
internal sealed class OpenHeldChest : BaseFeature
{
    private static readonly MethodBase ChestAddItem = AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem));

    private static readonly MethodBase ChestPerformToolAction = AccessTools.DeclaredMethod(
        typeof(Chest),
        nameof(Chest.performToolAction));

    private static readonly MethodBase InventoryMenuHighlightAllItems = AccessTools.DeclaredMethod(
        typeof(InventoryMenu),
        nameof(InventoryMenu.highlightAllItems));

    private readonly IModEvents events;

    private readonly Harmony harmony;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="OpenHeldChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public OpenHeldChest(IMonitor monitor, ModConfig config, IModEvents events, Harmony harmony, IInputHelper input)
        : base(monitor, nameof(OpenHeldChest), () => config.OpenHeldChest is not FeatureOption.Disabled)
    {
        this.events = events;
        this.harmony = harmony;
        this.input = input;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.GameLoop.UpdateTicking += OpenHeldChest.OnUpdateTicking;
        this.events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this.harmony.Patch(
            OpenHeldChest.ChestAddItem,
            new(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));

        this.harmony.Patch(
            OpenHeldChest.ChestPerformToolAction,
            transpiler: new(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_performToolAction_transpiler)));

        this.harmony.Patch(
            OpenHeldChest.InventoryMenuHighlightAllItems,
            postfix: new(typeof(OpenHeldChest), nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonPressed -= this.OnButtonPressed;
        this.events.GameLoop.UpdateTicking -= OpenHeldChest.OnUpdateTicking;

        // Patches
        this.harmony.Unpatch(
            OpenHeldChest.ChestAddItem,
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));

        this.harmony.Unpatch(
            OpenHeldChest.ChestPerformToolAction,
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_performToolAction_transpiler)));

        this.harmony.Unpatch(
            OpenHeldChest.InventoryMenuHighlightAllItems,
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix)));
    }

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!ReferenceEquals(__instance, item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private static IEnumerable<CodeInstruction> Chest_performToolAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Is(
                OpCodes.Newobj,
                AccessTools.Constructor(typeof(Debris), new[] { typeof(int), typeof(Vector2), typeof(Vector2) })))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(OpenHeldChest), nameof(OpenHeldChest.GetDebris));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static Debris GetDebris(Chest chest, int objectIndex, Vector2 debrisOrigin, Vector2 playerPosition)
    {
        var newChest = new Chest(true, Vector2.Zero, chest.ItemId)
        {
            Name = chest.Name,
            SpecialChestType = chest.SpecialChestType,
            fridge = { Value = chest.fridge.Value },
            lidFrameCount = { Value = chest.lidFrameCount.Value },
            playerChoiceColor = { Value = chest.playerChoiceColor.Value },
        };

        // Copy properties
        newChest.CopyFrom(chest);

        // Copy items from regular chest types
        if (chest is not
            {
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            }
            && !newChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            newChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)
                .OverwriteWith(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
        }

        return new(objectIndex, debrisOrigin, playerPosition) { item = newChest };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_highlightAllItems_postfix(ref bool __result, Item i)
    {
        if (!__result || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            return;
        }

        __result = !ReferenceEquals(itemGrabMenu.context, i);
    }

    private static void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (Game1.player.CurrentItem is Chest chest)
        {
            chest.updateWhenCurrentLocation(Game1.currentGameTime);
        }
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsActionButton()
            || StorageHandler.CurrentItem is null
                or
                {
                    OpenHeldChest: not FeatureOption.Enabled,
                }
            || Game1.player.CurrentItem.Stack > 1)
        {
            return;
        }

        if (Game1.player.CurrentItem is Chest chest)
        {
            chest.checkForAction(Game1.player);
        }
        else if (StorageHandler.CurrentItem.Data is Storage storageObject)
        {
            Game1.player.currentLocation.localSound("openChest");
            storageObject.ShowMenu();
        }

        this.input.Suppress(e.Button);
    }
}
