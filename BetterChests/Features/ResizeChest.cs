#nullable disable

namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using CommonHarmony.Enums;
using CommonHarmony.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Interfaces;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly Lazy<IMenuItems> _menuItems;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResizeChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    /// <param name="harmony">Helper to apply/reverse harmony patches.</param>
    public ResizeChest(IConfigModel config, IModHelper helper, IModServices services, HarmonyHelper harmony)
        : base(config, helper, services)
    {
        ResizeChest.Instance = this;
        this.Harmony = harmony;
        this.Harmony.AddPatch(
            this.Id,
            AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
            typeof(ResizeChest),
            nameof(ResizeChest.Chest_GetActualCapacity_postfix),
            PatchType.Postfix);
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static ResizeChest Instance { get; set; }

    private HarmonyHelper Harmony { get; }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (ResizeChest.Instance.ManagedObjects.TryGetManagedStorage(__instance, out var managedChest) && managedChest.ResizeChest == FeatureOption.Enabled && managedChest.ResizeChestCapacity != 0)
        {
            __result = managedChest.ResizeChestCapacity > 0
                ? managedChest.ResizeChestCapacity
                : int.MaxValue;
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        if (this.Config.ControlScheme.ScrollUp.JustPressed())
        {
            this.MenuItems.Offset--;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollUp);
            return;
        }

        if (this.Config.ControlScheme.ScrollDown.JustPressed())
        {
            this.MenuItems.Offset++;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollDown);
        }
    }
}