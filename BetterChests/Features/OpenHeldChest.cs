namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Allows a chest to be opened while in the farmer's inventory.
/// </summary>
internal sealed class OpenHeldChest : IFeature
{
    private const string Id = "furyx639.BetterChests/OpenHeldChest";

#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private bool _isActivated;

    private OpenHeldChest(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            OpenHeldChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                    typeof(OpenHeldChest),
                    nameof(OpenHeldChest.Chest_addItem_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                    typeof(OpenHeldChest),
                    nameof(OpenHeldChest.Chest_performToolAction_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.highlightAllItems)),
                    typeof(OpenHeldChest),
                    nameof(OpenHeldChest.InventoryMenu_highlightAllItems_postfix),
                    PatchType.Postfix),
            });
    }

    /// <summary>
    ///     Initializes <see cref="OpenHeldChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OpenHeldChest" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return OpenHeldChest.Instance ??= new OpenHeldChest(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(OpenHeldChest.Id);
        this._helper.Events.GameLoop.UpdateTicking += OpenHeldChest.OnUpdateTicking;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(OpenHeldChest.Id);
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.GameLoop.UpdateTicking -= OpenHeldChest.OnUpdateTicking;
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
        else
        {
            Game1.player.currentLocation.localSound("openChest");
            Storages.CurrentItem.ShowMenu();
        }

        this._helper.Input.Suppress(e.Button);
    }
}