namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <summary>
///     Allows a chest to be opened while in the farmer's inventory.
/// </summary>
internal class OpenHeldChest : IFeature
{
    private const string Id = "furyx639.BetterChests/OpenHeldChest";

    private OpenHeldChest(IModHelper helper)
    {
        this.Helper = helper;
        HarmonyHelper.AddPatches(
            OpenHeldChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                    typeof(OpenHeldChest),
                    nameof(OpenHeldChest.Chest_addItem_prefix),
                    PatchType.Prefix),
            });
    }

    private static OpenHeldChest? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

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
        if (this.IsActivated)
        {
            return;
        }

        this.IsActivated = true;
        HarmonyHelper.ApplyPatches(OpenHeldChest.Id);
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.GameLoop.UpdateTicking += OpenHeldChest.OnUpdateTicking;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this.IsActivated)
        {
            return;
        }

        this.IsActivated = false;
        HarmonyHelper.UnapplyPatches(OpenHeldChest.Id);
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.GameLoop.UpdateTicking -= OpenHeldChest.OnUpdateTicking;
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
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not SObject obj)
        {
            return;
        }

        // Disabled for object
        if (!StorageHelper.TryGetOne(obj, out var storage) || storage.OpenHeldChest == FeatureOption.Disabled)
        {
            return;
        }

        if (Context.IsMainPlayer)
        {
            obj.checkForAction(Game1.player);
        }
        else if (obj is Chest chest)
        {
            Game1.player.currentLocation.localSound("openChest");
            chest.ShowMenu();
        }

        this.Helper.Input.Suppress(e.Button);
    }
}