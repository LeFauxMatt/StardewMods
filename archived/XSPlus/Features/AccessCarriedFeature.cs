#nullable disable

namespace XSPlus.Features;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class AccessCarriedFeature : BaseFeature
{
    private HarmonyHelper _harmony;

    private AccessCarriedFeature(ServiceLocator serviceLocator)
        : base("AccessCarried", serviceLocator)
    {
        // Dependencies
        this.AddDependency<HarmonyHelper>(
            service =>
            {
                // Init
                this._harmony = service as HarmonyHelper;

                // Patches
                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                    typeof(AccessCarriedFeature),
                    nameof(AccessCarriedFeature.Chest_addItem_prefix));
            });
    }

    /// <inheritdoc />
    public override void Activate()
    {
        // Events
        this.Helper.Events.GameLoop.UpdateTicked += AccessCarriedFeature.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this._harmony.ApplyPatches(this.ServiceName);
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        // Events
        this.Helper.Events.GameLoop.UpdateTicked -= AccessCarriedFeature.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this._harmony.UnapplyPatches(this.ServiceName);
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
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Chest chest || !this.IsEnabledForItem(chest))
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