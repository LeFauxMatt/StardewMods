namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Adds a chest color picker that support hue, saturation, and lightness.
/// </summary>
internal class BetterColorPicker : IFeature
{
    private const string Id = "furyx639.BetterChests/BetterColorPicker";

    private static BetterColorPicker? Instance;

    private readonly PerScreen<HslColorPicker> _colorPicker = new(() => new());
    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private bool _isActivated;

    private BetterColorPicker(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        HarmonyHelper.AddPatches(
            BetterColorPicker.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(DiscreteColorPicker),
                        nameof(DiscreteColorPicker.getColorFromSelection)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.DiscreteColorPicker_getColorFromSelection_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(
                        typeof(DiscreteColorPicker),
                        nameof(DiscreteColorPicker.getSelectionFromColor)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_gameWindowSizeChanged_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_setSourceItem_postfix),
                    PatchType.Postfix),
            });
    }

    private HslColorPicker ColorPicker => this._colorPicker.Value;

    /// <summary>
    ///     Initializes <see cref="BetterColorPicker" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterColorPicker" /> class.</returns>
    public static BetterColorPicker Init(IModHelper helper, ModConfig config)
    {
        return BetterColorPicker.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(BetterColorPicker.Id);
        BetterItemGrabMenu.Constructed += this.OnConstructed;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(BetterColorPicker.Id);
        BetterItemGrabMenu.Constructed -= this.OnConstructed;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getColorFromSelection_postfix(int selection, ref Color __result)
    {
        if (selection == 0)
        {
            __result = Color.Black;
            return;
        }

        var rgb = BitConverter.GetBytes(selection);
        __result = new(rgb[0], rgb[1], rgb[2]);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getSelectionFromColor_postfix(Color c, ref int __result)
    {
        if (c == Color.Black)
        {
            __result = 0;
            return;
        }

        __result = (c.R << 0) | (c.G << 8) | (c.B << 16);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_gameWindowSizeChanged_postfix(ItemGrabMenu __instance)
    {
        if (__instance is not { chestColorPicker: not null, context: Chest chest })
        {
            return;
        }

        BetterColorPicker.Instance?.SetupColorPicker(__instance, chest);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
    {
        if (__instance is not { chestColorPicker: not null, context: Chest chest })
        {
            return;
        }

        BetterColorPicker.Instance?.SetupColorPicker(__instance, chest);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
         || Game1.activeClickableMenu is not ItemGrabMenu
            {
                chestColorPicker: not null, colorPickerToggleButton: var toggleButton,
            })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!toggleButton.containsPoint(x, y))
        {
            return;
        }

        Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
        Game1.playSound("drumkit6");
        this._helper.Input.Suppress(e.Button);
    }

    private void OnConstructed(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (itemGrabMenu is not { colorPickerToggleButton: not null, context: Chest chest })
        {
            return;
        }

        this.SetupColorPicker(itemGrabMenu, chest);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { colorPickerToggleButton: not null })
        {
            return;
        }

        this.ColorPicker.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                colorPickerToggleButton: not null, context: Chest chest,
            })
        {
            return;
        }

        this.ColorPicker.Update(this._helper.Input);
        chest.playerChoiceColor.Value = this.ColorPicker.Color;
    }

    private void SetupColorPicker(ItemGrabMenu itemGrabMenu, Chest chest)
    {
        itemGrabMenu.chestColorPicker = null;
        itemGrabMenu.discreteColorPickerCC = null;
        var x = this._config.CustomColorPickerArea switch
        {
            ComponentArea.Left => itemGrabMenu.xPositionOnScreen - 2 * Game1.tileSize - IClickableMenu.borderWidth / 2,
            _ => itemGrabMenu.xPositionOnScreen + itemGrabMenu.width + 96 + IClickableMenu.borderWidth / 2,
        };
        var y = itemGrabMenu.yPositionOnScreen - 56 + IClickableMenu.borderWidth / 2;
        this.ColorPicker.Init(x, y, chest.playerChoiceColor.Value);
    }
}