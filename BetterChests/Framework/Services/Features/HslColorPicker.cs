namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

// TODO: Color copy+paste
// TODO: Draw farmer nearby using cursor distance

/// <summary>Adds a color picker that support hue, saturation, and lightness.</summary>
internal sealed class HslColorPicker : BaseFeature
{
#nullable disable
    private static HslColorPicker instance;
#nullable enable

    private readonly IGameContentHelper gameContentHelper;
    private readonly Harmony harmony;
    private readonly PerScreen<HslColor> hslColor = new(() => default(HslColor));
    private readonly PerScreen<Slider?> hue = new();
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<Slider?> lightness = new();
    private readonly IModEvents modEvents;
    private readonly PerScreen<Slider?> saturation = new();
    private readonly PerScreen<int> xPosition = new();
    private readonly PerScreen<int> yPosition = new();

    /// <summary>Initializes a new instance of the <see cref="HslColorPicker" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public HslColorPicker(
        ILog log,
        ModConfig modConfig,
        IGameContentHelper gameContentHelper,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        HslColorPicker.instance = this;
        this.gameContentHelper = gameContentHelper;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.HslColorPicker != FeatureOption.Disabled;

    private HslColor CurrentColor
    {
        get => this.hslColor.Value;
        set => this.hslColor.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
            new HarmonyMethod(typeof(HslColorPicker), nameof(HslColorPicker.DiscreteColorPicker_draw_prefix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
            postfix: new HarmonyMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_getColorFromSelection_postfix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
            postfix: new HarmonyMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.receiveLeftClick)),
            new HarmonyMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_receiveLeftClick_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
            AccessTools.DeclaredMethod(typeof(HslColorPicker), nameof(HslColorPicker.DiscreteColorPicker_draw_prefix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
            AccessTools.DeclaredMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_getColorFromSelection_postfix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
            AccessTools.DeclaredMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_getSelectionFromColor_postfix)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.receiveLeftClick)),
            AccessTools.DeclaredMethod(
                typeof(HslColorPicker),
                nameof(HslColorPicker.DiscreteColorPicker_receiveLeftClick_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_draw_prefix(DiscreteColorPicker __instance)
    {
        if (HslColorPicker.instance.isActive.Value)
        {
            __instance.visible = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getColorFromSelection_postfix(int selection, ref Color __result)
    {
        if (!HslColorPicker.instance.isActive.Value)
        {
            return;
        }

        if (selection == 0)
        {
            __result = Color.Black;
            return;
        }

        var r = (byte)(selection & 0xFF);
        var g = (byte)((selection >> 8) & 0xFF);
        var b = (byte)((selection >> 16) & 0xFF);
        __result = new Color(r, g, b);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void DiscreteColorPicker_getSelectionFromColor_postfix(Color c, ref int __result)
    {
        if (!HslColorPicker.instance.isActive.Value)
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
    private static void DiscreteColorPicker_receiveLeftClick_prefix(DiscreteColorPicker __instance)
    {
        if (HslColorPicker.instance.isActive.Value)
        {
            __instance.visible = false;
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || this.itemGrabMenuManager.CurrentMenu?.colorPickerToggleButton is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.itemGrabMenuManager.CurrentMenu.colorPickerToggleButton.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
        Game1.playSound("drumkit6");
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
                { } colorPicker
            || this.itemGrabMenuManager.Top.Container?.Options.HslColorPicker != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = true;
        this.itemGrabMenuManager.CurrentMenu.discreteColorPickerCC = null;
        this.xPosition.Value = this.ModConfig.ColorPickerArea switch
        {
            ColorPickerArea.Left => this.itemGrabMenuManager.CurrentMenu.xPositionOnScreen
                - (2 * Game1.tileSize)
                - (IClickableMenu.borderWidth / 2),
            _ => this.itemGrabMenuManager.CurrentMenu.xPositionOnScreen
                + this.itemGrabMenuManager.CurrentMenu.width
                + 96
                + (IClickableMenu.borderWidth / 0x2),
        };

        this.yPosition.Value = this.itemGrabMenuManager.CurrentMenu.yPositionOnScreen
            - 56
            + (IClickableMenu.borderWidth / 2);

        var hsl = this.CurrentColor;

        this.hue.Value = new Slider(
            this.gameContentHelper.Load<Texture2D>(AssetHandler.HslTexturePath),
            () => hsl.H,
            value => hsl.H = value,
            new Rectangle(this.xPosition.Value, this.yPosition.Value + 36, 23, 522),
            29);

        this.lightness.Value = new Slider(
            value => new HslColor(hsl.H, colorPicker.colorSelection == 0 ? 0 : hsl.S, Math.Max(0.01f, value)).ToRgbColor(),
            () => hsl.L,
            value => hsl.L = value,
            new Rectangle(this.xPosition.Value + 32, this.yPosition.Value + 36, 23, 256),
            16);

        this.saturation.Value = new Slider(
            value => new HslColor(
                hsl.H,
                colorPicker.colorSelection == 0 ? 0 : value,
                Math.Max(0.01f, colorPicker.colorSelection == 0 ? value : hsl.L)).ToRgbColor(),
            () => hsl.S,
            value => hsl.S = value,
            new Rectangle(this.xPosition.Value + 32, this.yPosition.Value + 300, 23, 256),
            16);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || !Game1.player.showChestColorPicker)
        {
            return;
        }

        // Background
        IClickableMenu.drawTextureBox(
            e.SpriteBatch,
            this.xPosition.Value - (IClickableMenu.borderWidth / 2),
            this.yPosition.Value - (IClickableMenu.borderWidth / 2),
            58 + IClickableMenu.borderWidth,
            558 + IClickableMenu.borderWidth,
            Color.LightGray);

        // No color button

        // Hue slider
        this.hue.Value?.Draw(e.SpriteBatch);

        // Saturation slider
        this.saturation.Value?.Draw(e.SpriteBatch);

        // Lightness slider
        this.lightness.Value?.Draw(e.SpriteBatch);

        // No color button (selected)

        // Chest
    }
}