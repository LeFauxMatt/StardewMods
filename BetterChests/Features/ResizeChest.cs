namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using Common.Enums;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley.Objects;

/// <summary>
///     Expand the capacity of chests and add scrolling to access extra items.
/// </summary>
internal class ResizeChest : IFeature
{
    private const string Id = "BetterChests.ResizeChest";

    private ResizeChest(IModHelper helper)
    {
        this.Helper = helper;
        HarmonyHelper.AddPatches(
            ResizeChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    typeof(ResizeChest),
                    nameof(ResizeChest.Chest_GetActualCapacity_postfix),
                    PatchType.Postfix),
            });
    }

    private static ResizeChest? Instance { get; set; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="ResizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="ResizeChest" /> class.</returns>
    public static ResizeChest Init(IModHelper helper)
    {
        return ResizeChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(ResizeChest.Id);
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(ResizeChest.Id);
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        // Disabled for object
        if (!StorageHelper.TryGetOne(__instance, out var storage) || storage.ResizeChest == FeatureOption.Disabled || storage.ResizeChestCapacity == 0)
        {
            return;
        }

        __result = storage.Capacity;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        /*if (ResizeChest.MenuItems.Menu is null)
        {
            return;
        }

        if (Config.ControlScheme.ScrollUp.JustPressed())
        {
            ResizeChest.MenuItems.Offset--;
            ResizeChest.Helper.Input.SuppressActiveKeybinds(ResizeChest.Config.ControlScheme.ScrollUp);
            return;
        }

        if (Config.ControlScheme.ScrollDown.JustPressed())
        {
            ResizeChest.MenuItems.Offset++;
            ResizeChest.Helper.Input.SuppressActiveKeybinds(ResizeChest.Config.ControlScheme.ScrollDown);
        }*/
    }
}