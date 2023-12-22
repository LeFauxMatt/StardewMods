namespace StardewMods.BetterChests.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Extensions;
using StardewMods.Common.Models;
using StardewValley.Menus;

/// <summary>A component for picking a color using HSL sliders.</summary>
internal sealed class HslColorPicker
{
    private const int BarHeight = (HslColorPicker.Height - HslColorPicker.Gap - 36) / 2;
    private const int BarWidth = (HslColorPicker.Width / 2) - HslColorPicker.Gap;
    private const int Cells = 16;
    private const int CellSize = HslColorPicker.BarHeight / HslColorPicker.Cells;
    private const int Gap = 6;
    private const int Height = 558;
    private const int Width = 58;

    private static readonly Lazy<HslColor[]> ColorsLazy = new(HslColorPicker.GetColorsHsl);
    private static readonly Range<int> HslTrack = new();

    private static readonly Lazy<Texture2D> HueBarLazy = new(() => Game1.content.Load<Texture2D>("furyx639.BetterChests/HueBar"));

    private static readonly Rectangle SelectRect = new(412, 495, 5, 4);
    private static readonly Range<float> UnitRange = new(0, 1);

    private readonly Range<int> hueTrack = new();
    private readonly Rectangle[] lightnessBar = new Rectangle[HslColorPicker.Cells];
    private readonly Color[] lightnessShade = new Color[HslColorPicker.Cells];
    private readonly Range<int> lightnessTrack = new();

    private readonly ClickableTextureComponent noColor = new(new Rectangle(0, 0, 7, 7), Game1.mouseCursors, new Rectangle(295, 503, 7, 7), Game1.pixelZoom);

    private readonly Rectangle[] saturationBar = new Rectangle[HslColorPicker.Cells];
    private readonly Color[] saturationShade = new Color[HslColorPicker.Cells];
    private readonly Range<int> saturationTrack = new();
    private HslColor currentHslColor;
    private Thumb held = Thumb.None;
    private Rectangle hueBarArea = new(0, 0, HslColorPicker.BarWidth, HslColorPicker.Height - 36);
    private int hueCoord;

    private bool lastDown;
    private int lightnessCoord;
    private int menuX;
    private int menuY;
    private Rectangle noColorArea = new(0, 0, 36, 36);
    private int saturationCoord;

    /// <summary>Gets the current <see cref="Color" />.</summary>
    public Color Color { get; private set; }

    private static HslColor[] Colors => HslColorPicker.ColorsLazy.Value;

    private static Texture2D HueBar => HslColorPicker.HueBarLazy.Value;

    /// <summary>Draws the <see cref="HslColorPicker" /> to the screen.</summary>
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
            this.menuX - (IClickableMenu.borderWidth / 2),
            this.menuY - (IClickableMenu.borderWidth / 2),
            HslColorPicker.Width + IClickableMenu.borderWidth,
            HslColorPicker.Height + IClickableMenu.borderWidth,
            Color.LightGray);

        // No Color Button
        this.noColor.draw(b);

        // Hue Bar
        b.Draw(HslColorPicker.HueBar, this.hueBarArea, Color.White);

        for (var i = 0; i < HslColorPicker.Cells; ++i)
        {
            // Lightness Bar
            b.Draw(Game1.staminaRect, this.lightnessBar[i], this.lightnessShade[i]);

            // Saturation Bar
            b.Draw(Game1.staminaRect, this.saturationBar[i], this.saturationShade[i]);
        }

        // Item

        // No Color selected
        if (this.Color == Color.Black)
        {
            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(375, 357, 3, 3),
                this.noColorArea.Left,
                this.noColorArea.Top,
                this.noColorArea.Width,
                this.noColorArea.Height,
                Color.Black,
                Game1.pixelZoom,
                false);

            // Colorable object
            //this.itemToColor?.Draw(b, this.menuX, this.menuY - Game1.tileSize - (IClickableMenu.borderWidth / 2));
            return;
        }

        // Hue Selection
        b.Draw(Game1.mouseCursors, new Rectangle(this.hueBarArea.Left - 8, this.hueCoord, 20, 16), HslColorPicker.SelectRect, Color.White, MathHelper.PiOver2, new Vector2(2.5f, 4f), SpriteEffects.None, 1);

        // Lightness Selection
        b.Draw(Game1.mouseCursors, new Rectangle(this.lightnessBar[0].Left - 8, this.lightnessCoord, 20, 16), HslColorPicker.SelectRect, Color.White, MathHelper.PiOver2, new Vector2(2.5f, 4f), SpriteEffects.None, 1);

        // Saturation Selection
        b.Draw(Game1.mouseCursors, new Rectangle(this.saturationBar[0].Left - 8, this.saturationCoord, 20, 16), HslColorPicker.SelectRect, Color.White, MathHelper.PiOver2, new Vector2(2.5f, 4f), SpriteEffects.None, 1);

        // Colorable object
        //this.itemToColor?.Draw(b, this.menuX, this.menuY - Game1.tileSize - (IClickableMenu.borderWidth / 2));
    }

    /// <summary>Displays the <see cref="HslColorPicker" />.</summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Init(int x, int y)
    {
        this.menuX = x;
        this.menuY = y;
        var centerX = this.menuX + (HslColorPicker.Width / 2);
        var top = this.menuY + 36;

        this.hueBarArea.X = this.menuX;
        this.hueBarArea.Y = top;
        this.noColor.bounds.X = this.menuX - 2;
        this.noColor.bounds.Y = this.menuY;
        this.noColorArea.X = this.menuX - 6;
        this.noColorArea.Y = this.menuY - 4;
        this.hueTrack.Minimum = this.hueBarArea.Top;
        this.hueTrack.Maximum = this.hueBarArea.Bottom;
        for (var cell = 0; cell < HslColorPicker.Cells; ++cell)
        {
            this.lightnessBar[cell] = new Rectangle(centerX + (HslColorPicker.Gap / 2), top + (cell * HslColorPicker.CellSize), HslColorPicker.BarWidth, HslColorPicker.CellSize);

            this.saturationBar[cell] = new Rectangle(this.lightnessBar[cell].X, this.lightnessBar[cell].Y + HslColorPicker.BarHeight + HslColorPicker.Gap, HslColorPicker.BarWidth, HslColorPicker.CellSize);
        }

        this.lightnessTrack.Minimum = this.lightnessBar[0].Top;
        this.lightnessTrack.Maximum = this.lightnessBar[HslColorPicker.Cells - 1].Bottom;
        this.saturationTrack.Minimum = this.saturationBar[0].Top;
        this.saturationTrack.Maximum = this.saturationBar[HslColorPicker.Cells - 1].Bottom;

        this.currentHslColor = HslColor.FromColor(this.Color);
        if (this.Color == Color.Black)
        {
            this.hueCoord = this.hueTrack.Minimum;
            this.lightnessCoord = this.lightnessTrack.Minimum;
            this.saturationCoord = this.saturationTrack.Minimum;
        }
        else
        {
            var hueValues = HslColorPicker.Colors.Select((hsl, i) => (Index: i, Diff: Math.Abs(hsl.H - this.currentHslColor.H))).ToList();

            var minDiff = hueValues.Min(item => item.Diff);
            this.hueCoord = hueValues.First(item => Math.Abs(item.Diff - minDiff) == 0).Index.Remap(HslColorPicker.HslTrack, HslColorPicker.UnitRange).Remap(HslColorPicker.UnitRange, this.hueTrack);

            this.lightnessCoord = this.currentHslColor.L.Remap(HslColorPicker.UnitRange, this.lightnessTrack);
            this.saturationCoord = this.currentHslColor.S.Remap(HslColorPicker.UnitRange, this.saturationTrack);
        }

        for (var i = 0; i < HslColorPicker.Cells; ++i)
        {
            var value = (float)i / HslColorPicker.Cells;
            this.lightnessShade[i] = new HslColor
            {
                H = this.currentHslColor.H,
                S = this.Color == Color.Black ? 0 : this.currentHslColor.S,
                L = value,
            }.ToRgbColor();

            this.saturationShade[i] = new HslColor
            {
                H = this.currentHslColor.H,
                S = this.Color == Color.Black ? 0 : value,
                L = this.Color == Color.Black ? value : Math.Max(0.01f, this.currentHslColor.L),
            }.ToRgbColor();
        }

        this.held = Thumb.None;
    }

    /// <summary>Updates the <see cref="HslColorPicker" />.</summary>
    /// <param name="input">SMAPI helper for input.</param>
    public void Update(IInputHelper input)
    {
        if (!Game1.player.showChestColorPicker)
        {
            return;
        }

        var isDown = input.IsDown(SButton.MouseLeft);
        switch (this.lastDown)
        {
            case true when !isDown:
                this.held = Thumb.None;
                this.lastDown = false;
                return;
            case false when isDown:
                this.MouseDown();
                this.lastDown = true;
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
        if (this.noColorArea.Contains(x, y))
        {
            this.held = Thumb.NoColor;
            this.currentHslColor = new HslColor(0, 0, 0);
            this.Color = Color.Black;
            this.hueCoord = this.hueTrack.Minimum;
            this.lightnessCoord = this.lightnessTrack.Minimum;
            this.saturationCoord = this.saturationTrack.Minimum;
            Game1.playSound("coin");
            return;
        }

        if (this.hueBarArea.Contains(x, y))
        {
            this.held = Thumb.Hue;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        if (this.lightnessBar.Any(area => area.Contains(x, y)))
        {
            this.held = Thumb.Lightness;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        if (this.saturationBar.Any(area => area.Contains(x, y)))
        {
            this.held = Thumb.Saturation;
            Game1.playSound("coin");
            this.MouseMove();
            return;
        }

        this.held = Thumb.None;
    }

    private void MouseMove()
    {
        var (_, y) = Game1.getMousePosition(true);
        switch (this.held)
        {
            case Thumb.Hue:
                this.hueCoord = this.hueTrack.Clamp(y);
                var index = this.hueCoord.Remap(this.hueTrack, HslColorPicker.UnitRange).Remap(HslColorPicker.UnitRange, HslColorPicker.HslTrack);

                var hslColor = HslColorPicker.Colors[index];
                this.currentHslColor.H = hslColor.H;
                if (this.Color == Color.Black)
                {
                    this.currentHslColor.L = hslColor.L;
                    this.currentHslColor.S = hslColor.S;
                    this.lightnessCoord = this.currentHslColor.L.Remap(HslColorPicker.UnitRange, this.lightnessTrack);
                    this.saturationCoord = this.currentHslColor.S.Remap(HslColorPicker.UnitRange, this.saturationTrack);
                }

                break;
            case Thumb.Lightness:
                this.lightnessCoord = this.lightnessTrack.Clamp(y);
                this.currentHslColor.L = this.lightnessCoord.Remap(this.lightnessTrack, HslColorPicker.UnitRange);
                break;
            case Thumb.Saturation:
                this.saturationCoord = this.saturationTrack.Clamp(y);
                this.currentHslColor.S = this.saturationCoord.Remap(this.saturationTrack, HslColorPicker.UnitRange);
                break;
            case Thumb.NoColor:
                break;
            case Thumb.None:
            default:
                return;
        }

        this.Color = this.currentHslColor.ToRgbColor();
        for (var i = 0; i < HslColorPicker.Cells; ++i)
        {
            var value = (float)i / HslColorPicker.Cells;
            this.lightnessShade[i] = new HslColor
            {
                H = this.currentHslColor.H,
                S = this.Color == Color.Black ? 0 : this.currentHslColor.S,
                L = value,
            }.ToRgbColor();

            this.saturationShade[i] = new HslColor
            {
                H = this.currentHslColor.H,
                S = this.Color == Color.Black ? 0 : value,
                L = this.Color == Color.Black ? value : Math.Max(0.01f, this.currentHslColor.L),
            }.ToRgbColor();
        }
    }

    private enum Thumb
    {
        None,
        Hue,
        Saturation,
        Lightness,
        NoColor,
    }
}
