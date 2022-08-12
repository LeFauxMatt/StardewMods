namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Allows a placed chest full of items to be picked up by the farmer.
/// </summary>
internal class CarryChest : IFeature
{
    private const string Id = "furyx639.BetterChests/CarryChest";
    private const int WhichBuff = 69420;

    private static CarryChest? Instance;

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private bool _isActivated;

    private CarryChest(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        HarmonyHelper.AddPatches(
            CarryChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(Chest),
                        nameof(Chest.drawInMenu),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(Vector2),
                            typeof(float),
                            typeof(float),
                            typeof(float),
                            typeof(StackDrawType),
                            typeof(Color),
                            typeof(bool),
                        }),
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
                    AccessTools.Method(typeof(Item), nameof(Item.canBeTrashed)),
                    typeof(CarryChest),
                    nameof(CarryChest.Item_canBeTrashed_postfix),
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

    /// <summary>
    ///     Checks if the player should be overburdened while carrying a chest.
    /// </summary>
    /// <param name="excludeCurrent">Whether to exclude the current item.</param>
    public static void CheckForOverburdened(bool excludeCurrent = false)
    {
        if (CarryChest.Instance!._config.CarryChestSlowAmount == 0)
        {
            Game1.buffsDisplay.removeOtherBuff(CarryChest.WhichBuff);
            return;
        }

        if (StorageHelper.FromPlayer(Game1.player)
                         .Where(storage => !excludeCurrent || storage.Context != Game1.player.CurrentItem)
                         .Any(storage => storage.Items.OfType<Item>().Any()))
        {
            Game1.buffsDisplay.addOtherBuff(
                CarryChest.GetOverburdened(CarryChest.Instance._config.CarryChestSlowAmount));
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
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(CarryChest.Id);
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this._helper.Events.GameLoop.DayStarted += CarryChest.OnDayStarted;
        this._helper.Events.Player.InventoryChanged += CarryChest.OnInventoryChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(CarryChest.Id);
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.GameLoop.DayStarted -= CarryChest.OnDayStarted;
        this._helper.Events.Player.InventoryChanged -= CarryChest.OnInventoryChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_drawInMenu_postfix(
        Chest __instance,
        SpriteBatch spriteBatch,
        Vector2 location,
        float scaleSize,
        Color color)
    {
        // Draw Items count
        var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
        if (items > 0)
        {
            Utility.drawTinyDigits(
                items,
                spriteBatch,
                location
              + new Vector2(
                    Game1.tileSize - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize,
                    2f * scaleSize),
                3f * scaleSize,
                1f,
                color);
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_postfix(
        InventoryMenu __instance,
        ref Item? __result,
        ref (Item?, int)? __state)
    {
        if (__state is null)
        {
            return;
        }

        var (item, slotNumber) = __state.Value;
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_prefix(
        InventoryMenu __instance,
        int x,
        int y,
        ref (Item?, int)? __state)
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeTrashed_postfix(Item __instance, ref bool __result)
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (__instance is Chest || other is Chest)
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
    {
        if (__instance is not Chest chest)
        {
            return true;
        }

        var (x, y) = objectPosition;
        chest.draw(spriteBatch, (int)x, (int)y + Game1.tileSize, 1f, true);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Intentional to match game code")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
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
         || __instance is not Chest held
         || !location.Objects.TryGetValue(new(x / Game1.tileSize, y / Game1.tileSize), out var obj)
         || obj is not Chest placed)
        {
            return;
        }

        // Only copy items from regular chest types
        if (held is not { SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
         && !placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)
                  .CopyFrom(held.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
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
        if (exclude.Contains(chest) || chest.SpecialChestType is Chest.SpecialChestTypes.JunimoChest)
        {
            return;
        }

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
        if (!Context.IsPlayerFree
         || Game1.player.CurrentItem is Tool
         || !e.Button.IsUseToolButton()
         || this._helper.Input.IsSuppressed(e.Button)
         || (Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine")))
        {
            return;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y)
                / Game1.tileSize;
        if (!Game1.wasMouseVisibleThisFrame
         || Game1.mouseCursorTransparency == 0f
         || !Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player))
        {
            pos = Game1.player.GetGrabTile();
        }

        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
         || !StorageHelper.TryGetOne(obj, out var storage)
         || storage.CarryChest == FeatureOption.Disabled)
        {
            return;
        }

        // Already carrying the limit
        var limit = this._config.CarryChestLimit;
        if (limit > 0)
        {
            foreach (var item in Game1.player.Items.OfType<Chest>())
            {
                if (item.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
                {
                    limit--;
                }

                if (limit > 0)
                {
                    continue;
                }

                Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
                this._helper.Input.Suppress(e.Button);
                return;
            }
        }

        // Cannot add to inventory
        if (!Game1.player.addItemToInventoryBool(obj, true))
        {
            return;
        }

        Game1.currentLocation.Objects.Remove(pos);
        this._helper.Input.Suppress(e.Button);
        CarryChest.CheckForOverburdened();
    }
}