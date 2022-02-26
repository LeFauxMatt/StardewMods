namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class CarryChest : Feature
{
    private const int WhichBuff = 69420;
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CarryChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CarryChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        CarryChest.Instance ??= this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatches(
                    this.Id,
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
            });
    }

    private static CarryChest Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <summary>
    ///     Checks if the player should be overburdened while carrying a chest.
    /// </summary>
    /// <param name="excludeCurrent">Whether to exclude the current item.</param>
    public void CheckForOverburdened(bool excludeCurrent = false)
    {
        if (this.Config.CarryChestSlow == 0)
        {
            Game1.buffsDisplay.removeOtherBuff(CarryChest.WhichBuff);
            return;
        }

        if (this.ManagedObjects.InventoryStorages.Any(inventoryStorage => inventoryStorage.Value.Items.Any() && (!excludeCurrent || !ReferenceEquals(inventoryStorage.Value.Context, Game1.player.CurrentItem))))
        {
            Game1.buffsDisplay.addOtherBuff(CarryChest.GetOverburdened(this.Config.CarryChestSlow));
            return;
        }

        Game1.buffsDisplay.removeOtherBuff(CarryChest.WhichBuff);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.GameLoop.DayStarted -= this.OnDayStarted;
        this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
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
    private static void InventoryMenu_rightClick_postfix(InventoryMenu __instance, ref Item __result, ref ItemSlot __state)
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
    private static void InventoryMenu_rightClick_prefix(InventoryMenu __instance, int x, int y, ref ItemSlot __state)
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
        if (!__result
            || !location.Objects.TryGetValue(new(x / 64, y / 64), out var obj)
            || !CarryChest.Instance.ManagedObjects.TryGetManagedStorage(__instance, out var fromStorage))
        {
            return;
        }

        if (!CarryChest.Instance.ManagedObjects.TryGetManagedStorage(obj, out var toStorage))
        {
            return;
        }

        Log.Trace($"Placed storage {fromStorage.QualifiedItemId} from inventory to {location.NameOrUniqueName} at ({(x / 64).ToString()}, {(y / 64).ToString()}).");
        obj.Name = __instance.Name;

        // Only copy items from regular chest types
        if (!toStorage.Items.Any() && __instance is not Chest { SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
        {
            foreach (var item in fromStorage.Items)
            {
                toStorage.AddItem(item);
            }
        }

        foreach (var (key, value) in fromStorage.ModData.Pairs)
        {
            toStorage.ModData[key] = value;
        }

        // Initialize ItemMatcher
        foreach (var item in toStorage.FilterItemsList)
        {
            toStorage.ItemMatcher.Add(item);
        }

        if (fromStorage.Context is Chest fromChest && toStorage.Context is Chest toChest)
        {
            toChest.SpecialChestType = fromChest.SpecialChestType;
            toChest.fridge.Value = fromChest.fridge.Value;
            toChest.lidFrameCount.Value = fromChest.lidFrameCount.Value;
            toChest.playerChoiceColor.Value = fromChest.playerChoiceColor.Value;
        }

        CarryChest.Instance.CheckForOverburdened(true);
    }

    private static void RecursiveIterate(Farmer player, Chest chest, Action<Item> action, ICollection<Chest> exclude)
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
                CarryChest.RecursiveIterate(farmer, chest, action, new List<Chest>());
            }
        }
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button) || Game1.player.CurrentItem is GenericTool)
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player) || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        // Object is Chest and supports Carry Chest
        if (!this.ManagedObjects.TryGetManagedStorage(obj, out var managedChest) || managedChest.CarryChest == FeatureOption.Disabled)
        {
            return;
        }

        // Already carrying the limit
        if (this.Config.CarryChestLimit > 0 && Game1.player.Items.Count(item => item is Chest chest && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count > 0) >= this.Config.CarryChestLimit)
        {
            Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
            this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (!Game1.player.addItemToInventoryBool(obj, true))
        {
            return;
        }

        Log.Trace($"Picked up chest {managedChest.QualifiedItemId} from {Game1.currentLocation.NameOrUniqueName} at ({x.ToString()}, {y.ToString()}).");
        Game1.currentLocation.Objects.Remove(pos);
        this.Helper.Input.Suppress(e.Button);
        this.CheckForOverburdened();
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        this.CheckForOverburdened();
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        this.CheckForOverburdened();
    }

    private record ItemSlot(Item Item, int SlotNumber);
}