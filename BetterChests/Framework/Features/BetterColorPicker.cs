namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>Adds a chest color picker that support hue, saturation, and lightness.</summary>
internal sealed class BetterColorPicker : BaseFeature
{
    private static readonly MethodBase DiscreteColorPickerGetColorFromSelection = AccessTools.DeclaredMethod(
        typeof(DiscreteColorPicker),
        nameof(DiscreteColorPicker.getColorFromSelection));

    private static readonly MethodBase DiscreteColorPickerGetSelectionFromColor = AccessTools.DeclaredMethod(
        typeof(DiscreteColorPicker),
        nameof(DiscreteColorPicker.getSelectionFromColor));

    private static readonly MethodBase ItemGrabMenuGameWindowSizeChanged = AccessTools.DeclaredMethod(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.gameWindowSizeChanged));

    private static readonly MethodBase ItemGrabMenuSetSourceItem = AccessTools.DeclaredMethod(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.setSourceItem));

#nullable disable
    private static BetterColorPicker instance;
#nullable enable

    private readonly PerScreen<HslColorPicker> colorPicker = new(() => new());
    private readonly ModConfig config;
    private readonly IModEvents events;
    private readonly Harmony harmony;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="BetterColorPicker" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public BetterColorPicker(IMonitor monitor, ModConfig config, IModEvents events, Harmony harmony, IInputHelper input)
        : base(monitor, nameof(BetterColorPicker), () => config.CustomColorPicker is not FeatureOption.Disabled)
    {
        BetterColorPicker.instance = this;
        this.config = config;
        this.events = events;
        this.harmony = harmony;
        this.input = input;
    }

    private static IColorable? Colorable => BetterItemGrabMenu.Context?.Data as IColorable;

    [MemberNotNullWhen(true, nameof(BetterColorPicker.Colorable))]
    private static bool ShouldBeActive =>
        BetterItemGrabMenu.Context?.CustomColorPicker == FeatureOption.Enabled
        && BetterItemGrabMenu.Context.Data is IColorable
        && BetterItemGrabMenu.Context.Data is Storage storageObject
        && (!storageObject.ModData.TryGetValue("AlternativeTextureOwner", out var atOwner)
            || string.IsNullOrWhiteSpace(atOwner)
            || atOwner == "Stardew.Default");

    private HslColorPicker ColorPicker => this.colorPicker.Value;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterItemGrabMenu.Constructed += this.OnConstructed;
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this.harmony.Patch(
            BetterColorPicker.DiscreteColorPickerGetColorFromSelection,
            postfix: new(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.DiscreteColorPicker_getColorFromSelection_postfix)));

        this.harmony.Patch(
            BetterColorPicker.DiscreteColorPickerGetSelectionFromColor,
            postfix: new(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix)));

        this.harmony.Patch(
            BetterColorPicker.ItemGrabMenuGameWindowSizeChanged,
            postfix: new(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.ItemGrabMenu_gameWindowSizeChanged_postfix)));

        this.harmony.Patch(
            BetterColorPicker.ItemGrabMenuSetSourceItem,
            postfix: new(typeof(BetterColorPicker), nameof(BetterColorPicker.ItemGrabMenu_setSourceItem_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterItemGrabMenu.Constructed -= this.OnConstructed;
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this.harmony.Unpatch(
            BetterColorPicker.DiscreteColorPickerGetColorFromSelection,
            AccessTools.Method(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.DiscreteColorPicker_getColorFromSelection_postfix)));

        this.harmony.Unpatch(
            BetterColorPicker.DiscreteColorPickerGetSelectionFromColor,
            AccessTools.Method(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix)));

        this.harmony.Unpatch(
            BetterColorPicker.ItemGrabMenuGameWindowSizeChanged,
            AccessTools.Method(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.ItemGrabMenu_gameWindowSizeChanged_postfix)));

        this.harmony.Unpatch(
            BetterColorPicker.ItemGrabMenuSetSourceItem,
            AccessTools.Method(
                typeof(BetterColorPicker),
                nameof(BetterColorPicker.ItemGrabMenu_setSourceItem_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getColorFromSelection_postfix(int selection, ref Color __result)
    {
        if (!BetterColorPicker.ShouldBeActive)
        {
            return;
        }

        if (selection == 0)
        {
            __result = Color.Black;
            return;
        }

        var rgb = BitConverter.GetBytes(selection);
        __result = new(rgb[0], rgb[1], rgb[2]);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getSelectionFromColor_postfix(Color c, ref int __result)
    {
        if (!BetterColorPicker.ShouldBeActive)
        {
            return;
        }

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
        if (__instance is not
            {
                chestColorPicker: not null,
            }
            || !BetterColorPicker.ShouldBeActive)
        {
            return;
        }

        BetterColorPicker.instance.SetupColorPicker(__instance, BetterColorPicker.Colorable);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
    {
        if (__instance is not
            {
                chestColorPicker: not null,
            }
            || !BetterColorPicker.ShouldBeActive)
        {
            return;
        }

        BetterColorPicker.instance.SetupColorPicker(__instance, BetterColorPicker.Colorable);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || !BetterColorPicker.ShouldBeActive
            || Game1.activeClickableMenu is not ItemGrabMenu
            {
                colorPickerToggleButton: not null,
            } itemGrabMenu)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!itemGrabMenu.colorPickerToggleButton.containsPoint(x, y))
        {
            return;
        }

        Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
        Game1.playSound("drumkit6");
        this.input.Suppress(e.Button);
    }

    private void OnConstructed(object? sender, ItemGrabMenu itemGrabMenu)
    {
        if (!BetterColorPicker.ShouldBeActive)
        {
            return;
        }

        this.SetupColorPicker(itemGrabMenu, BetterColorPicker.Colorable);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!BetterColorPicker.ShouldBeActive || Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        this.ColorPicker.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!BetterColorPicker.ShouldBeActive || Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        this.ColorPicker.Update(this.input);
        BetterColorPicker.Colorable.Color = this.ColorPicker.Color;
    }

    private void SetupColorPicker(ItemGrabMenu itemGrabMenu, IColorable colorable)
    {
        itemGrabMenu.chestColorPicker = null;
        itemGrabMenu.discreteColorPickerCC = null;
        var x = this.config.CustomColorPickerArea switch
        {
            ComponentArea.Left => itemGrabMenu.xPositionOnScreen
                - (2 * Game1.tileSize)
                - (IClickableMenu.borderWidth / 2),
            _ => itemGrabMenu.xPositionOnScreen + itemGrabMenu.width + 96 + (IClickableMenu.borderWidth / 0x2),
        };

        var y = (itemGrabMenu.yPositionOnScreen - 56) + (IClickableMenu.borderWidth / 2);
        this.ColorPicker.Init(x, y, colorable);
    }
}
