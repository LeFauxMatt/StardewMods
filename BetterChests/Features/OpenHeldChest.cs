namespace StardewMods.BetterChests.Features;

using System.Linq;
using HarmonyLib;
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
        if (!StorageHelper.TryGetOne(obj, out var storage) || storage.OpenHeldChest is FeatureOption.Disabled)
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

        this._helper.Input.Suppress(e.Button);
    }
}