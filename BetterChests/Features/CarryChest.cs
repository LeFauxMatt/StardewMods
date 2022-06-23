namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

// TODO: Prevent losing chests if passed out
// TODO: Patch addToStack

/// <summary>
///     Allows a placed chest full of items to be picked up by the farmer.
/// </summary>
internal class CarryChest : IFeature
{
    private const string Id = "BetterChests.CarryChest";
    private const int WhichBuff = 69420;

    private CarryChest(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        HarmonyHelper.AddPatches(
            CarryChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                    typeof(CarryChest),
                    nameof(CarryChest.Chest_drawInMenu_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
                    typeof(CarryChest),
                    nameof(CarryChest.InventoryMenu_rightClick_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
                    typeof(CarryChest),
                    nameof(CarryChest.InventoryMenu_rightClick_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Item), nameof(Item.canBeDropped)),
                    typeof(CarryChest),
                    nameof(CarryChest.Item_canBeDropped_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                    typeof(CarryChest),
                    nameof(CarryChest.Item_canStackWith_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                    typeof(CarryChest),
                    nameof(CarryChest.Object_drawWhenHeld_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                    typeof(CarryChest),
                    nameof(CarryChest.Object_placementAction_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                    typeof(CarryChest),
                    nameof(CarryChest.Utility_iterateChestsAndStorage_postfix),
                    PatchType.Postfix),
            });
    }

    private static CarryChest? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Checks if the player should be overburdened while carrying a chest.
    /// </summary>
    /// <param name="excludeCurrent">Whether to exclude the current item.</param>
    public static void CheckForOverburdened(bool excludeCurrent = false)
    {
        if (CarryChest.Instance!.Config.CarryChestSlowAmount == 0)
        {
            Game1.buffsDisplay.removeOtherBuff(CarryChest.WhichBuff);
            return;
        }

        if (Game1.player.Items.OfType<Chest>().Any(chest => !excludeCurrent || Game1.player.CurrentItem != chest))
        {
            Game1.buffsDisplay.addOtherBuff(CarryChest.GetOverburdened(CarryChest.Instance.Config.CarryChestSlowAmount));
            return;
        }

        Game1.buffsDisplay.removeOtherBuff(CarryChest.WhichBuff);
    }

    /// <summary>
    ///     Initializes <see cref="CarryChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="CarryChest" /> class.</returns>
    public static CarryChest Init(IModHelper helper, ModConfig config)
    {
        return CarryChest.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(CarryChest.Id);
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.GameLoop.DayStarted += CarryChest.OnDayStarted;
        this.Helper.Events.Player.InventoryChanged += CarryChest.OnInventoryChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(CarryChest.Id);
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.GameLoop.DayStarted -= CarryChest.OnDayStarted;
        this.Helper.Events.Player.InventoryChanged -= CarryChest.OnInventoryChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Chest_drawInMenu_postfix(Chest __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, Color color)
    {
        // Draw Items count
        var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
        if (items > 0)
        {
            Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize), 3f * scaleSize, 1f, color);
        }
    }

    private static Buff GetOverburdened(int speed)
    {
        return new(0, 0, 0, 0, 0, 0, 0, 0, 0, -speed, 0, 0, int.MaxValue / 700, string.Empty, string.Empty)
        {
            description = string.Format(I18n.Effect_CarryChestSlow_Description(), speed.ToString()),
            which = CarryChest.WhichBuff,
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void InventoryMenu_rightClick_postfix(InventoryMenu __instance, ref Item? __result, ref ItemSlot? __state)
    {
        if (__state is null)
        {
            return;
        }

        var (item, slotNumber) = __state;

        if (item is null || __result is null)
        {
            return;
        }

        if (item.ParentSheetIndex != __result.ParentSheetIndex)
        {
            return;
        }

        if (__instance.actualInventory.ElementAtOrDefault(slotNumber) is not null)
        {
            return;
        }

        __result = item;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void InventoryMenu_rightClick_prefix(InventoryMenu __instance, int x, int y, ref ItemSlot? __state)
    {
        var slot = __instance.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null)
        {
            return;
        }

        var slotNumber = int.Parse(slot.name);
        var item = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (item is not null)
        {
            __state = new(item, slotNumber);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Item_canBeDropped_postfix(Item __instance, ref bool __result)
    {
        if (!__result || __instance is not Chest chest)
        {
            return;
        }

        if (chest is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
            && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result)
        {
            return;
        }

        if (__instance is not Chest chest || other is not Chest otherChest)
        {
            return;
        }

        // Block if mismatched data
        if (chest.SpecialChestType != otherChest.SpecialChestType
            || chest.fridge.Value != otherChest.fridge.Value
            || chest.playerChoiceColor.Value.PackedValue != otherChest.playerChoiceColor.Value.PackedValue)
        {
            __result = false;
            return;
        }

        // Block if either chest has any items
        if (chest is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
            && (chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any() || otherChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any()))
        {
            __result = false;
            return;
        }

        foreach (var key in __instance.modData.Keys.Concat(otherChest.modData.Keys).Distinct())
        {
            var hasValue = __instance.modData.TryGetValue(key, out var value);
            var otherHasValue = otherChest.modData.TryGetValue(key, out var otherValue);
            if (hasValue)
            {
                // Block if mismatched modData
                if (otherHasValue && value != otherValue)
                {
                    __result = false;
                    return;
                }

                continue;
            }

            if (otherHasValue)
            {
                __instance.modData[key] = otherValue;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
    {
        if (__instance is not Chest chest)
        {
            return true;
        }

        var (x, y) = objectPosition;
        chest.draw(spriteBatch, (int)x, (int)y + 64, 1f, true);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Intentional to match game code")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Parameter is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Object_placementAction_postfix(SObject __instance, GameLocation location, int x, int y, ref bool __result)
    {
        if (!__result || __instance is not Chest held || !location.Objects.TryGetValue(new(x / 64, y / 64), out var obj) || obj is not Chest placed)
        {
            return;
        }

        // Only copy items from regular chest types
        if (held is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } && !placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).CopyFrom(held.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
        }

        // Copy modData
        foreach (var (key, value) in held.modData.Pairs)
        {
            placed.modData[key] = value;
        }

        // Copy properties
        placed.Name = held.Name;
        placed.SpecialChestType = held.SpecialChestType;
        placed.fridge.Value = held.fridge.Value;
        placed.lidFrameCount.Value = held.lidFrameCount.Value;
        placed.playerChoiceColor.Value = held.playerChoiceColor.Value;

        CarryChest.CheckForOverburdened(true);
    }

    private static void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        CarryChest.CheckForOverburdened();
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        CarryChest.CheckForOverburdened();
    }

    private static void RecursiveIterate(Farmer player, Chest chest, Action<Item> action, ISet<Chest> exclude)
    {
        var items = chest.GetItemsForPlayer(player.UniqueMultiplayerID);
        if (!exclude.Contains(chest) && chest.SpecialChestType is not Chest.SpecialChestTypes.JunimoChest)
        {
            exclude.Add(chest);
            foreach (var item in items)
            {
                if (item is Chest otherChest)
                {
                    CarryChest.RecursiveIterate(player, otherChest, action, exclude);
                }

                if (item is not null)
                {
                    action(item);
                }
            }
        }
    }

    private static void Utility_iterateChestsAndStorage_postfix(Action<Item> action)
    {
        Log.Verbose("Recursively iterating chests in farmer inventory.");
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var chest in farmer.Items.OfType<Chest>())
            {
                CarryChest.RecursiveIterate(farmer, chest, action, new HashSet<Chest>());
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button) || Game1.player.CurrentItem is not null || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        // Object exists at pos, is within reach of player, and is a Chest
        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player) || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        // Disabled for object
        if (!StorageHelper.TryGetOne(obj, out var storage) || storage.CarryChest == FeatureOption.Disabled)
        {
            return;
        }

        // Already carrying the limit
        var limit = this.Config.CarryChestLimit;
        if (limit > 0)
        {
            foreach (var item in Game1.player.Items.OfType<Chest>())
            {
                if (item.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
                {
                    limit--;
                }

                if (limit <= 0)
                {
                    Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
                    this.Helper.Input.Suppress(e.Button);
                    return;
                }
            }
        }

        // Cannot add to inventory
        if (!Game1.player.addItemToInventoryBool(obj, true))
        {
            return;
        }

        Game1.currentLocation.Objects.Remove(pos);
        this.Helper.Input.Suppress(e.Button);
        CarryChest.CheckForOverburdened();
    }

    private record ItemSlot(Item? Item, int SlotNumber);
}