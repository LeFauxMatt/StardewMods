namespace StardewMods.FuryCore.UI;

using System;
using System.Linq;
using Common.Extensions;
using Common.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Enums;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     A widget for choosing a color using HSL sliders.
/// </summary>
public class HslColorPicker : DiscreteColorPicker
{
    private const int Gap = 6;
    private const int Height = 558;
    private const int Width = 58;

    private static readonly Rectangle SelectRect = new(412, 495, 5, 4);
    private static readonly Range<float> UnitRange = new(0, 1);

    private HslColor _hslColor;
    private int _hueCoord;
    private int _lightnessCoord;
    private int _saturationCoord;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HslColorPicker" /> class.
    /// </summary>
    /// <param name="contentHelper">Load assets from mod folder.</param>
    /// <param name="xPosition">The X coordinate to draw the HslColorPicker at.</param>
    /// <param name="yPosition">The Y coordinate to draw the HslColorPicker at.</param>
    /// <param name="initColor">The initial color to set the color picker to.</param>
    /// <param name="itemToDrawColored">The item to draw next to the color picker.</param>
    public HslColorPicker(IContentHelper contentHelper, int xPosition, int yPosition, Color initColor = default, Item itemToDrawColored = null)
        : base(xPosition, yPosition, 0, itemToDrawColored)
    {
        if (HslColorPicker.HueBar is null)
        {
            HslColorPicker.HueBar = contentHelper.Load<Texture2D>("assets/hue.png");
            HslColorPicker.ColorValues = new Color[HslColorPicker.HueBar.Width * HslColorPicker.HueBar.Height];
            HslColorPicker.HueBar.GetData(HslColorPicker.ColorValues);
            HslColorPicker.HslValues = HslColorPicker.ColorValues.Select(HslColor.FromColor).Distinct().ToArray();
            HslColorPicker.HslTrack = new(0, HslColorPicker.HslValues.Length);
        }

        this.width = HslColorPicker.Width;
        this.height = HslColorPicker.Height;

        var barWidth = this.width / 2 - HslColorPicker.Gap;
        var barHeight = (this.height - HslColorPicker.Gap - 36) / 2;
        var centerX = this.xPositionOnScreen + this.width / 2;

        this.HueBarArea = new(this.xPositionOnScreen, this.yPositionOnScreen + 36, barWidth, this.height - 36);
        this.LightnessBar = new(Axis.Vertical, new(centerX + HslColorPicker.Gap / 2, this.yPositionOnScreen + 36, barWidth, barHeight), this.GetLightnessShade);
        this.SaturationBar = new(Axis.Vertical, new(centerX + HslColorPicker.Gap / 2, this.yPositionOnScreen + 36 + barHeight + HslColorPicker.Gap, barWidth, barHeight), this.GetSaturationShade);

        this.HueTrack = new(this.HueBarArea.Top, this.HueBarArea.Bottom);
        this.LightnessTrack = new(this.LightnessBar.Area.Top, this.LightnessBar.Area.Bottom);
        this.SaturationTrack = new(this.SaturationBar.Area.Top, this.SaturationBar.Area.Bottom);

        this.NoColorButton = new(new(this.xPositionOnScreen - 2, this.yPositionOnScreen, 7, 7), Game1.mouseCursors, new(295, 503, 7, 7), Game1.pixelZoom);
        this.NoColorButtonArea = new(this.xPositionOnScreen - 6, this.yPositionOnScreen - 4, 36, 36);

        this.IsBlack = initColor.Equals(Color.Black);
        if (!this.IsBlack)
        {
            var initHsl = HslColor.FromColor(initColor);
            var hueIndex = HslColorPicker.HslValues.Select((hsl, i) => (h: Math.Abs(hsl.H - initHsl.H), i)).OrderBy(item => item.h).First().i;
            this.HueCoord = hueIndex.Remap(HslColorPicker.HslTrack, HslColorPicker.UnitRange).Remap(HslColorPicker.UnitRange, this.HueTrack);
            this.SaturationCoord = initHsl.S.Remap(HslColorPicker.UnitRange, this.SaturationTrack);
            this.LightnessCoord = initHsl.L.Remap(HslColorPicker.UnitRange, this.LightnessTrack);
        }

        this.colorSelection = HslColorPicker.GetSelectionFromColor(this.getCurrentColor());
    }

    private enum TrackThumb
    {
        None,
        Hue,
        Saturation,
        Luminance,
        Transparent,
    }

    private static Color[] ColorValues { get; set; }

    private static Range<int> HslTrack { get; set; }

    private static HslColor[] HslValues { get; set; }

    private static Texture2D HueBar { get; set; }

    private TrackThumb HeldThumb { get; set; } = TrackThumb.None;

    private Rectangle HueBarArea { get; }

    private int HueCoord
    {
        get => this._hueCoord;
        set
        {
            this._hueCoord = this.HueTrack.Clamp(value);
            var hueIndex = HslColorPicker.HslTrack.Clamp(this._hueCoord
                                                             .Remap(this.HueTrack, HslColorPicker.UnitRange)
                                                             .Remap(HslColorPicker.UnitRange, HslColorPicker.HslTrack));
            var hslColor = HslColorPicker.HslValues.ElementAtOrDefault(hueIndex);
            this._hslColor.H = hslColor.H;
            if (this.IsBlack)
            {
                this.SaturationCoord = hslColor.S.Remap(HslColorPicker.UnitRange, this.SaturationTrack);
                this.LightnessCoord = hslColor.L.Remap(HslColorPicker.UnitRange, this.LightnessTrack);
            }

            this.IsBlack = false;
        }
    }

    private Range<int> HueTrack { get; }

    private bool IsBlack { get; set; }

    private GradientBar LightnessBar { get; }

    private int LightnessCoord
    {
        get => this._lightnessCoord;
        set
        {
            this._lightnessCoord = this.LightnessTrack.Clamp(value);
            this._hslColor.L = HslColorPicker.UnitRange.Clamp(value.Remap(this.LightnessTrack, HslColorPicker.UnitRange));
            this.IsBlack = false;
        }
    }

    private Range<int> LightnessTrack { get; }

    private ClickableTextureComponent NoColorButton { get; }

    private Rectangle NoColorButtonArea { get; }

    private GradientBar SaturationBar { get; }

    private int SaturationCoord
    {
        get => this._saturationCoord;
        set
        {
            this._saturationCoord = this.SaturationTrack.Clamp(value);
            this._hslColor.S = HslColorPicker.UnitRange.Clamp(value.Remap(this.SaturationTrack, HslColorPicker.UnitRange));
            this.IsBlack = false;
        }
    }

    private Range<int> SaturationTrack { get; }

    /// <summary>Converts an int value to a <see cref="Microsoft.Xna.Framework.Color" /> object.</summary>
    /// <param name="selection">The int value to convert.</param>
    /// <returns>A Color object from the int value.</returns>
    public static Color GetColorFromSelection(int selection)
    {
        if (selection == 0)
        {
            return Color.Black;
        }

        var rgb = BitConverter.GetBytes(selection);
        return new(rgb[0], rgb[1], rgb[2]);
    }

    /// <summary>Converts a <see cref="Microsoft.Xna.Framework.Color" /> to an int value.</summary>
    /// <param name="c">The Color to convert.</param>
    /// <returns>An int value representing the color object.</returns>
    public static int GetSelectionFromColor(Color c)
    {
        if (c == Color.Black)
        {
            return 0;
        }

        return (c.R << 0) | (c.G << 8) | (c.B << 16);
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        if (!this.visible)
        {
            return;
        }

        // Menu Background
        HslColorPicker.drawTextureBox(
            b,
            this.xPositionOnScreen - HslColorPicker.borderWidth / 2,
            this.yPositionOnScreen - HslColorPicker.borderWidth / 2,
            this.width + HslColorPicker.borderWidth,
            this.height + HslColorPicker.borderWidth,
            Color.LightGray);

        // Transparent Selection Icon
        this.NoColorButton.draw(b);

        // Hue Bar
        b.Draw(HslColorPicker.HueBar, this.HueBarArea, Color.White);

        // Gradient Bars
        this.SaturationBar.Draw(b);
        this.LightnessBar.Draw(b);

        if (this.IsBlack)
        {
            HslColorPicker.drawTextureBox(
                b,
                Game1.mouseCursors,
                new(375, 357, 3, 3),
                this.NoColorButtonArea.Left,
                this.NoColorButtonArea.Top,
                this.NoColorButtonArea.Width,
                this.NoColorButtonArea.Height,
                Color.Black,
                4f,
                false);
        }

        // Hue Selection
        b.Draw(
            Game1.mouseCursors,
            new(this.HueBarArea.Left - 8, this.HueCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);

        // Saturation Selection
        b.Draw(
            Game1.mouseCursors,
            new(this.SaturationBar.Area.Left - 8, this.SaturationCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);

        // Luminance Selection
        b.Draw(
            Game1.mouseCursors,
            new(this.LightnessBar.Area.Left - 8, this.LightnessCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);

        // Colored Item if Chest
        if (this.itemToDrawColored is Chest chest)
        {
            chest.draw(b, this.xPositionOnScreen, this.yPositionOnScreen - HslColorPicker.borderWidth / 2 - Game1.tileSize, 1f, true);
        }
    }

    /// <summary>Gets the currently selected color.</summary>
    /// <returns>The currently selected color.</returns>
    public Color GetCurrentColor()
    {
        return this.IsBlack ? Color.Black : this._hslColor.ToRgbColor();
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
    }

    /// <summary>
    ///     Allows the <see cref="HslColorPicker" /> to register SMAPI events for handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void RegisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed += this.OnButtonPressed;
        inputEvents.ButtonReleased += this.OnButtonReleased;
        inputEvents.CursorMoved += this.OnCursorMoved;
        inputEvents.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <summary>
    ///     Allows the <see cref="HslColorPicker" /> to unregister SMAPI events from handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void UnregisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed -= this.OnButtonPressed;
        inputEvents.ButtonReleased -= this.OnButtonReleased;
        inputEvents.CursorMoved -= this.OnCursorMoved;
        inputEvents.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    private Color GetLightnessShade(float value)
    {
        return new HslColor
        {
            H = this._hslColor.H,
            S = this._hslColor.S,
            L = value,
        }.ToRgbColor();
    }

    private Color GetSaturationShade(float value)
    {
        return new HslColor
        {
            H = this._hslColor.H,
            S = value,
            L = Math.Max(0.01f, this._hslColor.L),
        }.ToRgbColor();
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (e.Button != SButton.MouseLeft || this.HeldThumb is not TrackThumb.None)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.NoColorButtonArea.Contains(x, y))
        {
            this.HeldThumb = TrackThumb.Transparent;
            this.HueCoord = 0;
            this.SaturationCoord = 0;
            this.LightnessCoord = 0;
            this.colorSelection = 0;
            this.IsBlack = true;
        }
        else if (this.HueBarArea.Contains(x, y))
        {
            this.HeldThumb = TrackThumb.Hue;
            this.HueCoord = y;
        }
        else if (this.SaturationBar.Area.Contains(x, y))
        {
            this.HeldThumb = TrackThumb.Saturation;
            this.SaturationCoord = y;
        }
        else if (this.LightnessBar.Area.Contains(x, y))
        {
            this.HeldThumb = TrackThumb.Luminance;
            this.LightnessCoord = y;
        }
        else
        {
            return;
        }

        Game1.playSound("coin");
        this.UpdateItemColor();
    }

    private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft && !e.IsDown(SButton.Left))
        {
            this.HeldThumb = TrackThumb.None;
        }
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        var (_, y) = Game1.getMousePosition(true);
        switch (this.HeldThumb)
        {
            case TrackThumb.Hue:
                this.HueCoord = y;
                break;
            case TrackThumb.Saturation:
                this.SaturationCoord = y;
                break;
            case TrackThumb.Luminance:
                this.LightnessCoord = y;
                break;
            case TrackThumb.None:
            case TrackThumb.Transparent:
            default:
                // Ignore
                return;
        }

        this.UpdateItemColor();
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        const int scrollAmount = 10;

        if (this.HeldThumb is not TrackThumb.None)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var delta = e.Delta > 0 ? -scrollAmount : scrollAmount;
        if (this.HueBarArea.Contains(x, y))
        {
            this.HueCoord += delta;
        }
        else if (this.SaturationBar.Area.Contains(x, y))
        {
            this.SaturationCoord += delta;
        }
        else if (this.LightnessBar.Area.Contains(x, y))
        {
            this.LightnessCoord += delta;
        }
        else
        {
            return;
        }

        this.UpdateItemColor();
    }

    private void UpdateItemColor()
    {
        var color = this.GetCurrentColor();
        this.colorSelection = HslColorPicker.GetSelectionFromColor(color);
        if (this.itemToDrawColored is Chest chest)
        {
            chest.playerChoiceColor.Value = color;
            chest.resetLidFrame();
        }
    }
}