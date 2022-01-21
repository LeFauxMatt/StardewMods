namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using BetterChests.Models;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using FuryCore.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CustomColorPicker : Feature
{
    private readonly PerScreen<HslColorPicker> _colorPicker = new();
    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomColorPicker"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CustomColorPicker(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        CustomColorPicker.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatches(
                    this.Id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getCurrentColor)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetCurrentColor_prefix),
                            PatchType.Prefix),
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetColorFromSelection_prefix),
                            PatchType.Prefix),
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetSelectionFromColor_prefix),
                            PatchType.Prefix),
                        new(
                            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.ItemGrabMenu_setSourceItem_postfix),
                            PatchType.Postfix),
                    });
            });
    }

    private static CustomColorPicker Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private HslColorPicker ColorPicker
    {
        get => this._colorPicker.Value;
        set => this._colorPicker.Value = value;
    }

    private ManagedChest ManagedChest
    {
        get => this._managedChest.Value;
        set => this._managedChest.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static bool DiscreteColorPicker_GetCurrentColor_prefix(DiscreteColorPicker __instance, ref Color __result)
    {
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, CustomColorPicker.Instance.ColorPicker))
        {
            return true;
        }

        __result = colorPicker.GetCurrentColor();
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static bool DiscreteColorPicker_GetColorFromSelection_prefix(DiscreteColorPicker __instance, int selection, ref Color __result)
    {
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, CustomColorPicker.Instance.ColorPicker))
        {
            return true;
        }

        __result = HslColorPicker.GetColorFromSelection(selection);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static bool DiscreteColorPicker_GetSelectionFromColor_prefix(DiscreteColorPicker __instance, Color c, ref int __result)
    {
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, CustomColorPicker.Instance.ColorPicker))
        {
            return true;
        }

        __result = HslColorPicker.GetSelectionFromColor(c);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
    {
        if (__instance.context is not Chest chest || !ReferenceEquals(chest, CustomColorPicker.Instance.ManagedChest.Chest))
        {
            return;
        }

        __instance.chestColorPicker = CustomColorPicker.Instance.GetColorPicker(__instance.chestColorPicker, chest);
        __instance.discreteColorPickerCC = null;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (!this.ManagedChests.FindChest(e.Chest, out var managedChest))
        {
            return;
        }

        this.Menu = e.ItemGrabMenu;
        this.Menu.chestColorPicker = this.GetColorPicker(this.Menu.chestColorPicker, e.Chest);
        this.Menu.discreteColorPickerCC = null;
        this.ManagedChest = managedChest;
    }

    private DiscreteColorPicker GetColorPicker(DiscreteColorPicker colorPicker, Chest chest)
    {
        if (ReferenceEquals(this.ColorPicker, colorPicker) && ReferenceEquals(this.ManagedChest.Chest, chest))
        {
            return colorPicker;
        }

        this.ColorPicker?.UnregisterEvents(this.Helper.Events.Input);

        var itemToDisplay = new Chest(true, chest.ParentSheetIndex)
        {
            Name = chest.Name,
            lidFrameCount = { Value = chest.lidFrameCount.Value },
            playerChoiceColor = { Value = chest.playerChoiceColor.Value },
        };

        foreach (var (key, value) in chest.modData.Pairs)
        {
            itemToDisplay.modData.Add(key, value);
        }

        this.ColorPicker = new(
            this.Helper.Content,
            this.Menu.xPositionOnScreen + this.Menu.width + 96 + (IClickableMenu.borderWidth / 2),
            this.Menu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2),
            chest.playerChoiceColor.Value,
            itemToDisplay);
        this.ColorPicker.RegisterEvents(this.Helper.Events.Input);

        return this.ColorPicker;
    }
}