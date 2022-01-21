namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Helpers;
using FuryCore.Interfaces;
using FuryCore.Services;
using HarmonyLib;
using BetterChests.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class OpenHeldChest : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenHeldChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public OpenHeldChest(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                    typeof(OpenHeldChest),
                    nameof(OpenHeldChest.Chest_addItem_prefix));
            });
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicked += OpenHeldChest.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicked -= OpenHeldChest.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!ReferenceEquals(__instance, item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        foreach (var chest in Game1.player.Items.Take(12).OfType<Chest>())
        {
            chest.updateWhenCurrentLocation(Game1.currentGameTime, Game1.player.currentLocation);
        }
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Chest chest)
        {
            return;
        }

        if (!this.ManagedChests.FindChest(chest, out _))
        {
            return;
        }

        Log.Trace($"Opening Menu for Carried ${chest.Name}.");
        if (Context.IsMainPlayer)
        {
            chest.checkForAction(Game1.player);
        }
        else
        {
            chest.ShowMenu();
        }

        this.Helper.Input.Suppress(e.Button);
    }
}