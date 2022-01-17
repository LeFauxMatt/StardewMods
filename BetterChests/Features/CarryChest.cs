namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BetterChests.Enums;
using Common.Helpers;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class CarryChest : Feature
{
    private readonly PerScreen<Chest> _chest = new();
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CarryChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CarryChest(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        this._harmony = services.Lazy<HarmonyHelper>(CarryChest.AddPatches);
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private HarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(CarryChest));
        this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(CarryChest));
        this.Helper.Events.GameLoop.UpdateTicking -= this.OnUpdateTicking;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged -= this.OnObjectListChanged;
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        harmony.AddPatches(
            nameof(CarryChest),
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                    typeof(CarryChest),
                    nameof(CarryChest.Item_canStackWith_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                    typeof(CarryChest),
                    nameof(CarryChest.Utility_iterateChestsAndStorage_postfix),
                    PatchType.Postfix),
            });
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
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
            this.Chest = Game1.player.CurrentItem as Chest;
        }
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || Game1.player.CurrentTool is not null)
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer((int)(64 * pos.X), (int)(64 * pos.Y), 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        if (!this.ManagedChests.FindChest(obj as Chest, out _))
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
        if (!e.IsCurrentLocation || this.Chest is null)
        {
            return;
        }

        var chest = e.Added.Select(added => added.Value).OfType<Chest>().SingleOrDefault();
        if (chest is null)
        {
            return;
        }

        chest.Name = this.Chest.Name;
        chest.SpecialChestType = this.Chest.SpecialChestType;
        chest.fridge.Value = this.Chest.fridge.Value;
        chest.lidFrameCount.Value = this.Chest.lidFrameCount.Value;
        chest.playerChoiceColor.Value = this.Chest.playerChoiceColor.Value;

        if (this.Chest.items.Any())
        {
            chest.items.CopyFrom(this.Chest.items);
        }

        foreach (var (key, value) in this.Chest.modData.Pairs)
        {
            chest.modData[key] = value;
        }
    }
}