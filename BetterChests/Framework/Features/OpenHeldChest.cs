namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Allows a chest to be opened while in the farmer's inventory.
/// </summary>
internal sealed class OpenHeldChest : Feature
{
    private const string Id = "furyx639.BetterChests/OpenHeldChest";

    private static readonly MethodBase ChestAddItem = AccessTools.Method(typeof(Chest), nameof(Chest.addItem));

    private static readonly MethodBase ChestPerformToolAction = AccessTools.Method(
        typeof(Chest),
        nameof(Chest.performToolAction));

    private static readonly MethodBase InventoryMenuHighlightAllItems = AccessTools.Method(
        typeof(InventoryMenu),
        nameof(InventoryMenu.highlightAllItems));

#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly Harmony _harmony;
    private readonly IModHelper _helper;

    private OpenHeldChest(IModHelper helper)
    {
        this._helper = helper;
        this._harmony = new(OpenHeldChest.Id);
    }

    /// <summary>
    ///     Initializes <see cref="OpenHeldChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OpenHeldChest" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return OpenHeldChest.Instance ??= new OpenHeldChest(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this._helper.Events.GameLoop.UpdateTicking += OpenHeldChest.OnUpdateTicking;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this._harmony.Patch(
            OpenHeldChest.ChestAddItem,
            new(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));
        this._harmony.Patch(
            OpenHeldChest.ChestPerformToolAction,
            transpiler: new(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_performToolAction_transpiler)));
        this._harmony.Patch(
            OpenHeldChest.InventoryMenuHighlightAllItems,
            postfix: new(typeof(OpenHeldChest), nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.GameLoop.UpdateTicking -= OpenHeldChest.OnUpdateTicking;

        // Patches
        this._harmony.Unpatch(
            OpenHeldChest.ChestAddItem,
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));
        this._harmony.Unpatch(
            OpenHeldChest.ChestPerformToolAction,
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_performToolAction_transpiler)));
        this._harmony.Unpatch(
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
                    AccessTools.Constructor(
                        typeof(Debris),
                        new[]
                        {
                            typeof(int),
                            typeof(Vector2),
                            typeof(Vector2),
                        })))
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

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static Debris GetDebris(Chest chest, int objectIndex, Vector2 debrisOrigin, Vector2 playerPosition)
    {
        var newChest = new Chest(true, Vector2.Zero, chest.ParentSheetIndex)
        {
            Name = chest.Name,
            SpecialChestType = chest.SpecialChestType,
            fridge = { Value = chest.fridge.Value },
            lidFrameCount = { Value = chest.lidFrameCount.Value },
            playerChoiceColor = { Value = chest.playerChoiceColor.Value },
        };

        // Copy properties
        newChest._GetOneFrom(chest);

        // Copy items from regular chest types
        if (chest is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
         && !newChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            newChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)
                    .CopyFrom(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
        }

        return new(objectIndex, debrisOrigin, playerPosition)
        {
            item = newChest,
        };
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
            chest.updateWhenCurrentLocation(Game1.currentGameTime, Game1.currentLocation);
        }
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
         || !e.Button.IsActionButton()
         || Storages.CurrentItem is null or { OpenHeldChest: not FeatureOption.Enabled }
         || Game1.player.CurrentItem.Stack > 1)
        {
            return;
        }

        if (Game1.player.CurrentItem is Chest chest)
        {
            chest.checkForAction(Game1.player);
        }
        else if (Storages.CurrentItem.Data is Storage storageObject)
        {
            Game1.player.currentLocation.localSound("openChest");
            storageObject.ShowMenu();
        }

        this._helper.Input.Suppress(e.Button);
    }
}