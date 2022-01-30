namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Helpers;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class CarryChest : Feature
{
    private readonly PerScreen<Chest> _chest = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarryChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CarryChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
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
                            AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                            typeof(CarryChest),
                            nameof(CarryChest.Utility_iterateChestsAndStorage_postfix),
                            PatchType.Postfix),
                    });
            });
    }

    private Chest CurrentChest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicking -= this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged -= this.OnObjectListChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Chest_drawInMenu_postfix(Chest __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color color)
    {
        // Draw Items count
        var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
        if (items > 0)
        {
            Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - (3f * scaleSize), 2f * scaleSize), 3f * scaleSize, 1f, color);
        }
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
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result)
        {
            return;
        }

        var chest = __instance as Chest;
        var otherChest = other as Chest;

        // Block if either chest has any items
        if ((chest is not null && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
            || (otherChest is not null && otherChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any()))
        {
            __result = false;
            return;
        }

        if (chest is null || otherChest is null)
        {
            return;
        }

        // Block if mismatched data
        if (chest.playerChoiceColor.Value.PackedValue != otherChest.playerChoiceColor.Value.PackedValue)
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

    private static void Utility_iterateChestsAndStorage_postfix(Action<Item> action)
    {
        Log.Verbose("Recursively iterating chests in farmer inventory.");
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var chest in farmer.Items.OfType<Chest>())
            {
                CarryChest.RecursiveIterate(chest, action);
            }
        }
    }

    private static void RecursiveIterate(Chest chest, Action<Item> action)
    {
        if (chest.SpecialChestType is Chest.SpecialChestTypes.None)
        {
            foreach (var item in chest.items.OfType<Chest>())
            {
                CarryChest.RecursiveIterate(item, action);
            }
        }

        action(chest);
    }

    private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
    {
        if (Context.IsPlayerFree)
        {
            this.CurrentChest = Game1.player.CurrentItem as Chest;
        }
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button))
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        // Object is Chest and supports Carry Chest
        if (!this.ManagedChests.FindChest(obj as Chest, out var managedChest) || managedChest.CarryChest == FeatureOption.Disabled)
        {
            return;
        }

        if (!Game1.player.addItemToInventoryBool(obj, true))
        {
            return;
        }

        Game1.currentLocation.Objects.Remove(pos);
        this.Helper.Input.Suppress(e.Button);
    }

    [EventPriority(EventPriority.High)]
    private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        if (!e.IsCurrentLocation || this.CurrentChest is null)
        {
            return;
        }

        var chest = e.Added.Select(added => added.Value).OfType<Chest>().SingleOrDefault();
        if (chest is null)
        {
            return;
        }

        chest.Name = this.CurrentChest.Name;
        chest.SpecialChestType = this.CurrentChest.SpecialChestType;
        chest.fridge.Value = this.CurrentChest.fridge.Value;
        chest.lidFrameCount.Value = this.CurrentChest.lidFrameCount.Value;
        chest.playerChoiceColor.Value = this.CurrentChest.playerChoiceColor.Value;

        if (this.CurrentChest.items.Any())
        {
            chest.items.CopyFrom(this.CurrentChest.items);
        }

        foreach (var (key, value) in this.CurrentChest.modData.Pairs)
        {
            chest.modData[key] = value;
        }
    }

    private record ItemSlot(Item Item, int SlotNumber);
}