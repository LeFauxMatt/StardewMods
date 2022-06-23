namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Enums;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Storages;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Objects;

/// <summary>
///     Allows a chest to be opened while in the farmer's inventory.
/// </summary>
internal class OpenHeldChest : IFeature
{
    private const string Id = "BetterChests.OpenHeldChest";

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
        HarmonyHelper.ApplyPatches(OpenHeldChest.Id);
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        if (Context.IsMainPlayer)
        {
            this.Helper.Events.GameLoop.UpdateTicked += OpenHeldChest.OnUpdateTicked;
        }

        if (IntegrationHelper.BetterCrafting.IsLoaded)
        {
            IntegrationHelper.BetterCrafting.API.RegisterInventoryProvider(typeof(BaseStorage), new StorageProvider());
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(OpenHeldChest.Id);
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

        if (Context.IsMainPlayer)
        {
            this.Helper.Events.GameLoop.UpdateTicked -= OpenHeldChest.OnUpdateTicked;
        }

        if (IntegrationHelper.BetterCrafting.IsLoaded)
        {
            IntegrationHelper.BetterCrafting.API.UnregisterInventoryProvider(typeof(BaseStorage));
        }
    }

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!ReferenceEquals(__instance, item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        foreach (var player in Game1.getOnlineFarmers())
        {
            foreach (var item in player.Items.Take(12).OfType<Object>())
            {
                item.updateWhenCurrentLocation(Game1.currentGameTime, player.currentLocation);
            }
        }
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Object obj)
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