namespace StardewMods.ChestProducers;

using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Objects;

/// <inheritdoc />
public class ChestProducers : Mod
{
    private static Chest? TargetChest;

    private bool _isInit;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;

        // Events
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

        // Patches
        HarmonyHelper.AddPatches(
            $"{this.ModManifest.UniqueID}/Early",
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_checkForAction_transpiler),
                    PatchType.Transpiler),
            });

        HarmonyHelper.AddPatches(
            $"{this.ModManifest.UniqueID}/Reverse",
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_checkForAction),
                    PatchType.Reverse),
                new(
                    AccessTools.Method(
                        typeof(SObject),
                        nameof(SObject.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(int),
                            typeof(int),
                            typeof(float),
                        }),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_draw),
                    PatchType.Reverse),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.performObjectDropInAction)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_performObjectDropInAction),
                    PatchType.Reverse),
            });

        HarmonyHelper.AddPatches(
            $"{this.ModManifest.UniqueID}/Late",
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(Chest),
                        nameof(Chest.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(int),
                            typeof(int),
                            typeof(float),
                        }),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Chest_draw_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Chest_GetItemsForPlayer_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.DayUpdate)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_DayUpdate_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_minutesElapsed_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                    typeof(ChestProducers),
                    nameof(ChestProducers.Object_placementAction_postfix),
                    PatchType.Postfix),
            });
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null || ChestProducers.TargetChest is null)
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        item.resetState();
        ChestProducers.TargetChest.clearNulls();
        item = ChestProducers.TargetChest.addItem(item);

        return item is null || farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_draw_prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        if (!__instance.modData.ContainsKey("furyx639.ChestProducers/IsProducer"))
        {
            return true;
        }

        ChestProducers.Object_draw(__instance, spriteBatch, x, y, alpha);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_GetItemsForPlayer_postfix(Chest __instance, long id, ref NetObjectList<Item> __result)
    {
        if (__instance.heldObject.Value is Chest chest)
        {
            __result = chest.GetItemsForPlayer(id);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_checkForAction(SObject __instance, Farmer who, bool justCheckingForActivity)
    {
        throw new NotImplementedException("This is a stub.");
    }

    private static IEnumerable<CodeInstruction> Object_checkForAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return instructions.MethodReplacer(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
            AccessTools.Method(typeof(ChestProducers), nameof(ChestProducers.AddItemToInventoryBool)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_DayUpdate_postfix(SObject __instance)
    {
        if (__instance is not Chest chest)
        {
            return;
        }

        // Store output item
        if (__instance.MinutesUntilReady == 0
         && __instance.readyForHarvest.Value
         && __instance.heldObject.Value is not null
         && __instance.heldObject.Value is not Chest)
        {
            ChestProducers.TargetChest = chest;
            ChestProducers.Object_checkForAction(__instance, Game1.player, false);
            ChestProducers.TargetChest = null;
        }

        // Load input item
        if (__instance.Type?.Equals("Crafting") != true && __instance.Type?.Equals("interactive") != true)
        {
            return;
        }

        var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
        for (var index = 0; index < items.Count; index++)
        {
            if (!ChestProducers.Object_performObjectDropInAction(__instance, items[index], false, Game1.player))
            {
                continue;
            }

            items[index].Stack--;
            if (items[index].Stack <= 0)
            {
                items[index] = null;
            }

            break;
        }

        chest.clearNulls();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_draw(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        throw new NotImplementedException("This is a stub.");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_minutesElapsed_postfix(SObject __instance)
    {
        if (__instance is not Chest chest)
        {
            return;
        }

        // Store output item
        if (__instance.MinutesUntilReady == 0
         && __instance.readyForHarvest.Value
         && __instance.heldObject.Value is not null
         && __instance.heldObject.Value is not Chest)
        {
            ChestProducers.TargetChest = chest;
            ChestProducers.Object_checkForAction(__instance, Game1.player, false);
            ChestProducers.TargetChest = null;
        }

        // Load input item
        if (__instance.Type?.Equals("Crafting") != true && __instance.Type?.Equals("interactive") != true)
        {
            return;
        }

        var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
        for (var index = 0; index < items.Count; index++)
        {
            if (!ChestProducers.Object_performObjectDropInAction(__instance, items[index], false, Game1.player))
            {
                continue;
            }

            items[index].Stack--;
            if (items[index].Stack <= 0)
            {
                items[index] = null;
            }

            break;
        }

        chest.clearNulls();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_performObjectDropInAction(SObject __instance, Item dropInItem, bool probe, Farmer who)
    {
        throw new NotImplementedException("This is a stub.");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        GameLocation location,
        int x,
        int y,
        Farmer who,
        ref bool __result)
    {
        var pos = new Vector2(x / Game1.tileSize, y / Game1.tileSize);
        if (!__result
         || __instance is Chest
         || !location.Objects.TryGetValue(pos, out var obj)
         || obj is (Chest or Furniture) and not { bigCraftable.Value: false }
         || obj.GetType() != typeof(SObject))
        {
            return;
        }

        var chest = new Chest(true, Vector2.Zero, obj.ParentSheetIndex)
        {
            DisplayName = obj.DisplayName,
            Name = obj.Name,
            SpecialVariable = obj.SpecialVariable,
        };

        chest._GetOneFrom(obj);

        // Copy modData
        foreach (var (key, value) in obj.modData.Pairs)
        {
            chest.modData[key] = value;
        }

        chest.modData["furyx639.ChestProducers/IsProducer"] = true.ToString();
        chest.performDropDownAction(who);

        // Replace object with Chest
        location.Objects[pos] = chest;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (this._isInit)
        {
            return;
        }

        this._isInit = true;
        HarmonyHelper.ApplyPatches($"{this.ModManifest.UniqueID}/Early");
        HarmonyHelper.ApplyPatches($"{this.ModManifest.UniqueID}/Reverse");
        HarmonyHelper.ApplyPatches($"{this.ModManifest.UniqueID}/Late");
    }
}