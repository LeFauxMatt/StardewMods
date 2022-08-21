namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Objects;

/// <summary>
///     Allows a chest to be opened while in the farmer's inventory.
/// </summary>
internal class OpenHeldChest : IFeature
{
    private const string Id = "furyx639.BetterChests/OpenHeldChest";

    private static OpenHeldChest? Instance;

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
            });
    }

    /// <summary>
    ///     Initializes <see cref="OpenHeldChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OpenHeldChest" /> class.</returns>
    public static OpenHeldChest Init(IModHelper helper)
    {
        return OpenHeldChest.Instance ??= new(helper);
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
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this._helper.Events.GameLoop.UpdateTicking += OpenHeldChest.OnUpdateTicking;
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

        // Copy modData
        foreach (var (key, value) in chest.modData.Pairs)
        {
            newChest.modData[key] = value;
        }

        return new(objectIndex, debrisOrigin, playerPosition)
        {
            item = newChest,
        };
    }

    private static void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        foreach (var obj in Game1.player.Items.Take(12).OfType<SObject>())
        {
            obj.updateWhenCurrentLocation(Game1.currentGameTime, Game1.currentLocation);
        }
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
         || !e.Button.IsActionButton()
         || StorageHelper.CurrentItem is null or { OpenHeldChest: not FeatureOption.Enabled })
        {
            return;
        }

        Game1.player.currentLocation.localSound("openChest");
        StorageHelper.CurrentItem.ShowMenu();
        this._helper.Input.Suppress(e.Button);
    }
}