namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly Lazy<IMenuItems> _menuItems;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResizeChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ResizeChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        ResizeChest.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    typeof(ResizeChest),
                    nameof(ResizeChest.Chest_GetActualCapacity_postfix),
                    PatchType.Postfix);
            });
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static ResizeChest Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

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
        if (ResizeChest.Instance.ManagedStorages.FindStorage(__instance, out var managedChest) && managedChest.ResizeChestCapacity != 0)
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