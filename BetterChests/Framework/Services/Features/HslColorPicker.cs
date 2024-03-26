namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

// TODO: Draw farmer nearby using cursor distance

/// <summary>Adds a color picker that support hue, saturation, and lightness.</summary>
internal sealed class HslColorPicker : BaseFeature<HslColorPicker>
{
    private static HslColorPicker instance = null!;

    private readonly PerScreen<ClickableTextureComponent> copyButton = new();
    private readonly PerScreen<Rectangle> copyButtonArea = new();
    private readonly Harmony harmony;
    private readonly PerScreen<Slider?> holding = new();
    private readonly PerScreen<HslColor> hslColor = new(() => default(HslColor));
    private readonly PerScreen<Slider> hue;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<Slider> lightness;
    private readonly PerScreen<ClickableTextureComponent> noColorButton = new();
    private readonly PerScreen<Rectangle> noColorButtonArea = new();
    private readonly PerScreen<Slider> saturation;
    private readonly PerScreen<HslColor> savedColor = new();
    private readonly PerScreen<int> xPosition = new();
    private readonly PerScreen<int> yPosition = new();

    /// <summary>Initializes a new instance of the <see cref="HslColorPicker" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public HslColorPicker(
        AssetHandler assetHandler,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        HslColorPicker.instance = this;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;

        var hslTexture = gameContentHelper.Load<Texture2D>(assetHandler.HslTexturePath);
        var colors = new Color[hslTexture.Width * hslTexture.Height];
        hslTexture.GetData(colors);
        var hslColors = colors.Select(HslColor.FromColor).Distinct().ToArray();

        this.copyButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 8, 8),
                gameContentHelper.Load<Texture2D>(assetHandler.IconTexturePath),
                new Rectangle(116, 4, 8, 8),
                3));

        this.noColorButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, 7, 7),
                Game1.mouseCursors,
                new Rectangle(295, 503, 7, 7),
                Game1.pixelZoom));

        this.hue = new PerScreen<Slider>(
            () => new Slider(
                hslTexture,
                () => this.CurrentColor.H,
                value =>
                {
                    this.CurrentColor = this.ColorSelection == 0
                        ? hslColors[(int)(value * hslColors.Length)]
                        : this.CurrentColor with { H = value };

                    this.UpdateColor();
                },
                new Rectangle(0, 0, 23, 522),
                29));

        this.saturation = new PerScreen<Slider>(
            () => new Slider(
                value => (this.CurrentColor with
                {
                    S = this.ColorSelection == 0 ? 0 : value,
                    L = Math.Max(0.01f, this.ColorSelection == 0 ? value : this.CurrentColor.L),
                }).ToRgbColor(),
                () => this.CurrentColor.S,
                value =>
                {
                    this.CurrentColor = this.ColorSelection == 0
                        ? new HslColor(0, value, 0.5f)
                        : this.CurrentColor with
                        {
                            S = value,
                            L = Math.Max(0.01f, this.CurrentColor.L),
                        };

                    this.UpdateColor();
                },
                new Rectangle(0, 0, 23, 256),
                16));

        this.lightness = new PerScreen<Slider>(
            () => new Slider(
                value => (this.CurrentColor with
                {
                    S = this.ColorSelection == 0 ? 0 : this.CurrentColor.S,
                    L = value,
                }).ToRgbColor(),
                () => this.CurrentColor.L,
                value =>
                {
                    this.CurrentColor = this.CurrentColor with { L = value };
                    this.UpdateColor();
                },
                new Rectangle(0, 0, 23, 256),
                16));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.HslColorPicker != FeatureOption.Disabled;

    private int ColorSelection
    {
        get => this.itemGrabMenuManager.CurrentMenu?.chestColorPicker?.colorSelection ?? 0;
        set
        {
            if (this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not null)
            {
                this.itemGrabMenuManager.CurrentMenu.chestColorPicker.colorSelection = value;
            }
        }
    }

    private HslColor CurrentColor
    {
        get => this.hslColor.Value;
        set => this.hslColor.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<RenderingActiveMenuEventArgs>(this.OnRenderingActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight or SButton.ControllerA or SButton.ControllerB)
            || this.holding.Value is not null
            || this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
            {
                itemToDrawColored: Chest chest,
            } colorPicker
            || this.itemGrabMenuManager.Top.Container is not ChestContainer container)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.itemGrabMenuManager.CurrentMenu.colorPickerToggleButton.containsPoint(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            Game1.playSound("drumkit6");
            Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
            return;
        }

        if (!Game1.player.showChestColorPicker)
        {
            return;
        }

        if (this.noColorButtonArea.Value.Contains(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            Game1.playSound("coin");
            this.CurrentColor = new HslColor(0, 0, 0);
            colorPicker.colorSelection = 0;
            chest.playerChoiceColor.Value = Color.Black;
            container.Chest.playerChoiceColor.Value = Color.Black;
            return;
        }

        if (this.copyButtonArea.Value.Contains(mouseX, mouseY))
        {
            this.inputHelper.Suppress(e.Button);
            Game1.playSound("coin");
            if (e.Button is SButton.MouseLeft or SButton.ControllerA)
            {
                this.savedColor.Value = this.CurrentColor;
            }
            else
            {
                this.CurrentColor = this.savedColor.Value;
                this.UpdateColor();
            }

            return;
        }

        if (this.hue.Value.LeftClick(mouseX, mouseY))
        {
            this.holding.Value = this.hue.Value;
        }
        else if (this.saturation.Value.LeftClick(mouseX, mouseY))
        {
            this.holding.Value = this.saturation.Value;
        }
        else if (this.lightness.Value.LeftClick(mouseX, mouseY))
        {
            this.holding.Value = this.lightness.Value;
        }

        if (this.holding.Value is not null)
        {
            this.inputHelper.Suppress(e.Button);
            Game1.playSound("coin");
        }
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
            {
                itemToDrawColored: Chest chest,
            }
            || this.itemGrabMenuManager.Top.Container is not ChestContainer container
            || container.Options.HslColorPicker != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = true;
        this.itemGrabMenuManager.CurrentMenu.discreteColorPickerCC = null;
        this.xPosition.Value = this.itemGrabMenuManager.CurrentMenu.xPositionOnScreen
            - (2 * Game1.tileSize)
            - (IClickableMenu.borderWidth / 2);

        this.yPosition.Value = this.itemGrabMenuManager.CurrentMenu.yPositionOnScreen
            - 56
            + (IClickableMenu.borderWidth / 2);

        var c = container.Chest.playerChoiceColor.Value;
        chest.playerChoiceColor.Value = c;
        this.CurrentColor = container.Chest.playerChoiceColor.Value == Color.Black
            ? new HslColor(0, 0, 0)
            : HslColor.FromColor(c);

        this.ColorSelection = (c.R << 0) | (c.G << 8) | (c.B << 16);

        this.copyButtonArea.Value = new Rectangle(this.xPosition.Value + 30, this.yPosition.Value - 4, 36, 36);
        this.noColorButtonArea.Value = new Rectangle(this.xPosition.Value - 6, this.yPosition.Value - 4, 36, 36);
        this.noColorButton.Value.bounds.X = this.xPosition.Value - 2;
        this.noColorButton.Value.bounds.Y = this.yPosition.Value;
        this.copyButton.Value.bounds.X = this.xPosition.Value + 34;
        this.copyButton.Value.bounds.Y = this.yPosition.Value + 2;
        this.hue.Value.MoveTo(this.xPosition.Value, this.yPosition.Value + 36);
        this.saturation.Value.MoveTo(this.xPosition.Value + 32, this.yPosition.Value + 300);
        this.lightness.Value.MoveTo(this.xPosition.Value + 32, this.yPosition.Value + 36);
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value
            || !Game1.player.showChestColorPicker
            || this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
            {
                itemToDrawColored: Chest chest,
            } colorPicker)
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
        this.noColorButton.Value.draw(e.SpriteBatch);

        // Copy button
        this.copyButton.Value.draw(e.SpriteBatch);

        // Hue slider
        this.hue.Value.Draw(e.SpriteBatch);

        // Saturation slider
        this.saturation.Value.Draw(e.SpriteBatch);

        // Lightness slider
        this.lightness.Value.Draw(e.SpriteBatch);

        // No color button (selected)
        if (colorPicker.colorSelection == 0)
        {
            IClickableMenu.drawTextureBox(
                e.SpriteBatch,
                Game1.mouseCursors,
                new Rectangle(375, 357, 3, 3),
                this.noColorButtonArea.Value.X,
                this.noColorButtonArea.Value.Y,
                36,
                36,
                Color.Black,
                Game1.pixelZoom,
                false);
        }

        // Chest
        chest.draw(
            e.SpriteBatch,
            this.xPosition.Value,
            this.yPosition.Value - Game1.tileSize - (IClickableMenu.borderWidth / 2),
            local: true);
    }

    private void OnRenderingActiveMenu(RenderingActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || !Game1.player.showChestColorPicker)
        {
            return;
        }

        var isDown = this.inputHelper.IsDown(SButton.MouseLeft) || this.inputHelper.IsSuppressed(SButton.MouseLeft);
        if (!isDown)
        {
            if (this.holding.Value is not null)
            {
                this.holding.Value.Holding = false;
                this.holding.Value = null;
            }
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.hue.Value.Update(mouseX, mouseY);
        this.saturation.Value.Update(mouseX, mouseY);
        this.lightness.Value.Update(mouseX, mouseY);
    }

    private void UpdateColor()
    {
        if (this.itemGrabMenuManager.CurrentMenu?.chestColorPicker is not
            {
                itemToDrawColored: Chest chest,
            }
            || this.itemGrabMenuManager.Top.Container is not ChestContainer container)
        {
            return;
        }

        var c = this.CurrentColor.ToRgbColor();
        container.Chest.playerChoiceColor.Value = c;
        chest.playerChoiceColor.Value = c;
        this.ColorSelection = (c.R << 0) | (c.G << 8) | (c.B << 16);
    }
}