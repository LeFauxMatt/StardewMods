namespace StardewMods.BetterChests.UI;

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewMods.Common.Extensions;
using StardewMods.Common.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     A component for picking a color using HSL sliders.
/// </summary>
internal class HslColorPicker
{
    private const int BarHeight = (HslColorPicker.Height - HslColorPicker.Gap - 36) / 2;
    private const int BarWidth = HslColorPicker.Width / 2 - HslColorPicker.Gap;
    private const int Cells = 16;
    private const int CellSize = HslColorPicker.BarHeight / HslColorPicker.Cells;
    private const int Gap = 6;
    private const int Height = 558;
    private const int Width = 58;

    private static readonly Lazy<HslColor[]> ColorsLazy = new(HslColorPicker.GetColorsHsl);
    private static readonly Range<int> HslTrack = new();

    private static readonly Lazy<Texture2D> HueBarLazy = new(
        () => Game1.content.Load<Texture2D>("furyx639.BetterChests/HueBar"));

    private static readonly Rectangle SelectRect = new(412, 495, 5, 4);
    private static readonly Range<float> UnitRange = new(0, 1);

    private readonly Range<int> _hueTrack = new();
    private readonly Rectangle[] _lightnessBar = new Rectangle[HslColorPicker.Cells];
    private readonly Color[] _lightnessShade = new Color[HslColorPicker.Cells];
    private readonly Range<int> _lightnessTrack = new();

    private readonly ClickableTextureComponent _noColor = new(
        new(0, 0, 7, 7),
        Game1.mouseCursors,
        new(295, 503, 7, 7),
        Game1.pixelZoom);

    private readonly Rectangle[] _saturationBar = new Rectangle[HslColorPicker.Cells];
    private readonly Color[] _saturationShade = new Color[HslColorPicker.Cells];
    private readonly Range<int> _saturationTrack = new();

    private Thumb _held = Thumb.None;
    private HslColor _hslColor;
    private Rectangle _hueBarArea = new(0, 0, HslColorPicker.BarWidth, HslColorPicker.Height - 36);
    private int _hueCoord;
    private bool _lastDown;
    private int _lightnessCoord;
    private Rectangle _noColorArea = new(0, 0, 36, 36);
    private int _saturationCoord;
    private int _x;
    private int _y;

    private enum Thumb
    {
        None,
        Hue,
        Saturation,
        Lightness,
        NoColor,
    }

    /// <summary>
    ///     Gets the current <see cref="Color" />.
    /// </summary>
    public Color Color { get; private set; }

    private static HslColor[] Colors => HslColorPicker.ColorsLazy.Value;

    private static Texture2D HueBar => HslColorPicker.HueBarLazy.Value;

    /// <summary>
    ///     Draws the <see cref="HslColorPicker" /> to the screen.
    /// </summary>
    /// <param name="b">The <see cref="SpriteBatch" /> to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        if (!Game1.player.showChestColorPicker)
        {
            return;
        }

        // Background
        IClickableMenu.drawTextureBox(
            b,
            this._x - IClickableMenu.borderWidth / 2,
            this._y - IClickableMenu.borderWidth / 2,
            HslColorPicker.Width + IClickableMenu.borderWidth,
            HslColorPicker.Height + IClickableMenu.borderWidth,
            Color.LightGray);

        // No Color Button
        this._noColor.draw(b);

        // Hue Bar
        b.Draw(HslColorPicker.HueBar, this._hueBarArea, Color.White);

        for (var i = 0; i < HslColorPicker.Cells; i++)
        {
            // Lightness Bar
            b.Draw(Game1.staminaRect, this._lightnessBar[i], this._lightnessShade[i]);

            // Saturation Bar
            b.Draw(Game1.staminaRect, this._saturationBar[i], this._saturationShade[i]);
        }

        // Item

        // No Color selected
        if (this.Color == Color.Black)
        {
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new(375, 357, 3, 3),
                this._noColorArea.Left,
                this._noColorArea.Top,
                this._noColorArea.Width,
                this._noColorArea.Height,
                Color.Black,
                Game1.pixelZoom,
                false);
            return;
        }

        // Hue Selection
        b.Draw(
            Game1.mouseCursors,
            new(this._hueBarArea.Left - 8, this._hueCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);

        // Lightness Selection
        b.Draw(
            Game1.mouseCursors,
            new(this._lightnessBar[0].Left - 8, this._lightnessCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);

        // Saturation Selection
        b.Draw(
            Game1.mouseCursors,
            new(this._saturationBar[0].Left - 8, this._saturationCoord, 20, 16),
            HslColorPicker.SelectRect,
            Color.White,
            MathHelper.PiOver2,
            new(2.5f, 4f),
            SpriteEffects.None,
            1);
    }

    /// <summary>
    ///     Displays the <see cref="HslColorPicker" />.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="color">The color to initialize.</param>
    public void Init(int x, int y, Color? color = default)
    {
        this._x = x;
        this._y = y;
        var centerX = this._x + HslColorPicker.Width / 2;
        var top = this._y + 36;

        this._hueBarArea.X = this._x;
        this._hueBarArea.Y = top;
        this._noColor.bounds.X = this._x - 2;
        this._noColor.bounds.Y = this._y;
        this._noColorArea.X = this._x - 6;
        this._noColorArea.Y = this._y - 4;
        this._hueTrack.Minimum = this._hueBarArea.Top;
        this._hueTrack.Maximum = this._hueBarArea.Bottom;
        for (var cell = 0; cell < HslColorPicker.Cells; cell++)
        {
            this._lightnessBar[cell] = new(
                centerX + HslColorPicker.Gap / 2,
                top + cell * HslColorPicker.CellSize,
                HslColorPicker.BarWidth,
                HslColorPicker.CellSize);
            this._saturationBar[cell] = new(
                this._lightnessBar[cell].X,
                this._lightnessBar[cell].Y + HslColorPicker.BarHeight + HslColorPicker.Gap,
                HslColorPicker.BarWidth,
                HslColorPicker.CellSize);
        }

        this._lightnessTrack.Minimum = this._lightnessBar[0].Top;
        this._lightnessTrack.Maximum = this._lightnessBar[HslColorPicker.Cells - 1].Bottom;
        this._saturationTrack.Minimum = this._saturationBar[0].Top;
        this._saturationTrack.Maximum = this._saturationBar[HslColorPicker.Cells - 1].Bottom;

        this.Color = color ?? Color.Black;
        this._hslColor = HslColor.FromColor(this.Color);
        if (this.Color == Color.Black)
        {
            this._hueCoord = this._hueTrack.Minimum;
            this._lightnessCoord = this._lightnessTrack.Minimum;
            this._saturationCoord = this._saturationTrack.Minimum;
        }
        else
        {
            this._hueCoord = HslColorPicker.Colors.Select((hsl, i) => (Hsl: hsl, Index: i))
                                           .OrderBy(item => Math.Abs(item.Hsl.H - this._hslColor.H))
                                           .First()
                                           .Index.Remap(HslColorPicker.HslTrack, HslColorPicker.UnitRange)
                                           .Remap(HslColorPicker.UnitRange, this._hueTrack);
            this._lightnessCoord = this._hslColor.L.Remap(HslColorPicker.UnitRange, this._lightnessTrack);
            this._saturationCoord = this._hslColor.S.Remap(HslColorPicker.UnitRange, this._saturationTrack);
        }

        for (var i = 0; i < HslColorPicker.Cells; i++)
        {
            var value = (float)i / HslColorPicker.Cells;
            this._lightnessShade[i] = new HslColor
            {
                H = this._hslColor.H,
                S = this.Color == Color.Black ? 0 : this._hslColor.S,
                L = value,
            }.ToRgbColor();
            this._saturationShade[i] = new HslColor
            {
                H = this._hslColor.H,
                S = this.Color == Color.Black ? 0 : value,
                L = this.Color == Color.Black ? value : Math.Max(0.01f, this._hslColor.L),
            }.ToRgbColor();
        }

        this._held = Thumb.None;
    }

    /// <summary>
    ///     Updates the <see cref="HslColorPicker" />.
    /// </summary>
    /// <param name="input">SMAPI helper for input.</param>
    public void Update(IInputHelper input)
    {
        if (!Game1.player.showChestColorPicker)
        {
            return;
        }

        var isDown = input.IsDown(SButton.MouseLeft);
        switch (this._lastDown)
        {
            case true when !isDown:
                this._held = Thumb.None;
                this._lastDown = false;
                return;
            case false when isDown:
                this.MouseDown();
                this._lastDown = true;
                return;
            default:
                this.MouseMove();
                return;
        }
    }

    private static HslColor[] GetColorsHsl()
    {
        var colorsRgb = new Color[HslColorPicker.HueBar.Width * HslColorPicker.HueBar.Height];
        HslColorPicker.HueBar.GetData(colorsRgb);
        var colorsHsl = colorsRgb.Select(HslColor.FromColor).Distinct().ToArray();
        HslColorPicker.HslTrack.Minimum = 0;
        HslColorPicker.HslTrack.Maximum = colorsHsl.Length - 1;
        return colorsHsl;
    }

    private void MouseDown()
    {
        var (x, y) = Game1.getMousePosition(true);
        if (this._noColorArea.Contains(x, y))
        {
            this._held = Thumb.NoColor;
            this._hslColor = new(0, 0, 0);
            this.Color = Color.Black;
            this._hueCoord = this._hueTrack.Minimum;
            this._lightnessCoord = this._lightnessTrack.Minimum;
            this._saturationCoord = this._saturationTrack.Minimum;
            Game1.playSound("coin");
            return;
        }

        if (this._hueBarArea.Contains(x, y))
        {
            this._held = Thumb.Hue;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        if (this._lightnessBar.Any(area => area.Contains(x, y)))
        {
            this._held = Thumb.Lightness;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        if (this._saturationBar.Any(area => area.Contains(x, y)))
        {
            this._held = Thumb.Saturation;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        this._held = Thumb.None;
    }

    private void MouseMove()
    {
        var (_, y) = Game1.getMousePosition(true);
        switch (this._held)
        {
            case Thumb.Hue:
                this._hueCoord = this._hueTrack.Clamp(y);
                var index = this._hueCoord.Remap(this._hueTrack, HslColorPicker.UnitRange)
                                .Remap(HslColorPicker.UnitRange, HslColorPicker.HslTrack);
                var hslColor = HslColorPicker.Colors[index];
                this._hslColor.H = hslColor.H;
                if (this.Color == Color.Black)
                {
                    this._hslColor.L = hslColor.L;
                    this._hslColor.S = hslColor.S;
                    this._lightnessCoord = this._hslColor.L.Remap(HslColorPicker.UnitRange, this._lightnessTrack);
                    this._saturationCoord = this._hslColor.S.Remap(HslColorPicker.UnitRange, this._saturationTrack);
                }

                break;
            case Thumb.Lightness:
                this._lightnessCoord = this._lightnessTrack.Clamp(y);
                this._hslColor.L = this._lightnessCoord.Remap(this._lightnessTrack, HslColorPicker.UnitRange);
                break;
            case Thumb.Saturation:
                this._saturationCoord = this._saturationTrack.Clamp(y);
                this._hslColor.S = this._saturationCoord.Remap(this._saturationTrack, HslColorPicker.UnitRange);
                break;
            case Thumb.NoColor:
                break;
            case Thumb.None:
            default:
                return;
        }

        this.Color = this._hslColor.ToRgbColor();
        for (var i = 0; i < HslColorPicker.Cells; i++)
        {
            var value = (float)i / HslColorPicker.Cells;
            this._lightnessShade[i] = new HslColor
            {
                H = this._hslColor.H,
                S = this.Color == Color.Black ? 0 : this._hslColor.S,
                L = value,
            }.ToRgbColor();
            this._saturationShade[i] = new HslColor
            {
                H = this._hslColor.H,
                S = this.Color == Color.Black ? 0 : value,
                L = this.Color == Color.Black ? value : Math.Max(0.01f, this._hslColor.L),
            }.ToRgbColor();
        }
    }
}