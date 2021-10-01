namespace Common.UI
{
    using System;
    using Enums;
    using Helpers;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using StardewValley;
    using StardewValley.Menus;

    /// <summary>
    ///     A widget for choosing a color using HSL sliders.
    /// </summary>
    internal class HSLSlider
    {
        private const int Gap = 6;
        private const int Cells = 16;
        private static readonly Rectangle SelectRect = new(412, 495, 5, 4);
        private readonly Color[] _colors;
        private readonly Slider _hueSlider = new(Axis.Vertical);
        private readonly GradientBar _luminanceBar = new(Axis.Vertical, HSLSlider.Cells);
        private readonly Slider _luminanceSlider = new(Axis.Vertical);
        private readonly GradientBar _saturationBar = new(Axis.Vertical, HSLSlider.Cells);
        private readonly Slider _saturationSlider = new(Axis.Vertical);
        private readonly Texture2D _texture;
        private readonly ClickableTextureComponent _transparentBox;
        private Rectangle _area;
        private HSLColor _color;
        private Hold _holding = Hold.None;
        private bool _isBlack;
        private Rectangle _transparentArea;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HSLSlider" /> class.
        /// </summary>
        public HSLSlider()
        {
            this._texture = Content.FromMod<Texture2D>("assets/hue.png");
            this._colors = new Color[this._texture.Width * this._texture.Height];
            this._texture.GetData(this._colors);
            this._transparentBox = new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors, new Rectangle(295, 503, 7, 7), Game1.pixelZoom);
        }

        /// <summary>
        ///     Gets or sets the boundary of the HSL widget.
        /// </summary>
        public Rectangle Area
        {
            get => this._area;
            set
            {
                this._area = value;
                var barWidth = value.Width / 2 - HSLSlider.Gap;
                var barHeight = (value.Height - HSLSlider.Gap - 36) / 2;
                this._hueSlider.Area = new Rectangle(value.Left, value.Top + 36, barWidth, value.Height - 36);
                this._saturationSlider.Area = new Rectangle(value.Center.X + HSLSlider.Gap / 2, value.Top + 36, barWidth, barHeight);
                this._luminanceSlider.Area = new Rectangle(value.Center.X + HSLSlider.Gap / 2, value.Top + 36 + barHeight + HSLSlider.Gap, barWidth, barHeight);
                this._saturationBar.Area = this._saturationSlider.Area;
                this._luminanceBar.Area = this._luminanceSlider.Area;
                this._transparentBox.bounds = new Rectangle(value.Left - 2, value.Top, 7, 7);
                this._transparentArea = new Rectangle(value.Left - 6, value.Top - 4, 36, 36);
            }
        }

        /// <summary>
        ///     Gets or sets the currently selected color.
        /// </summary>
        public Color Color
        {
            get => this._isBlack ? Color.Black : this._color.ToRgbColor();
            set
            {
                this._color = HSLColor.FromColor(value);
                this._isBlack = value == Color.Black;
                this.UpdateColor();
            }
        }

        /// <summary>
        ///     Draws the HSL widget to the screen.
        /// </summary>
        /// <param name="b">The sprite batch to draw to.</param>
        public void Draw(SpriteBatch b)
        {
            // Menu Background
            IClickableMenu.drawTextureBox(b, this._area.Left - IClickableMenu.borderWidth / 2, this._area.Top - IClickableMenu.borderWidth / 2, this._area.Width + IClickableMenu.borderWidth, this._area.Height + IClickableMenu.borderWidth, Color.LightGray);

            // Transparent Selection Icon
            this._transparentBox.draw(b);

            // Hue Bar
            b.Draw(this._texture, this._hueSlider.Area, Color.White);

            // Gradient Bars
            this._saturationBar.Draw(b);
            this._luminanceBar.Draw(b);

            if (this._isBlack)
            {
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.mouseCursors,
                    new Rectangle(375, 357, 3, 3),
                    this._transparentArea.Left,
                    this._transparentArea.Top,
                    this._transparentArea.Width,
                    this._transparentArea.Height,
                    Color.Black,
                    4f,
                    false);
            }

            // Hue Selection
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(this._hueSlider.Area.Left - 8, this._hueSlider.Coordinate, 20, 16),
                HSLSlider.SelectRect,
                Color.White,
                MathHelper.PiOver2,
                new Vector2(2.5f, 4f),
                SpriteEffects.None,
                1);

            // Saturation Selection
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(this._saturationSlider.Area.Left - 8, this._saturationSlider.Coordinate, 20, 16),
                HSLSlider.SelectRect,
                Color.White,
                MathHelper.PiOver2,
                new Vector2(2.5f, 4f),
                SpriteEffects.None,
                1);

            // Luminance Selection
            b.Draw(
                Game1.mouseCursors,
                new Rectangle(this._luminanceSlider.Area.Left - 8, this._luminanceSlider.Coordinate, 20, 16),
                HSLSlider.SelectRect,
                Color.White,
                MathHelper.PiOver2,
                new Vector2(2.5f, 4f),
                SpriteEffects.None,
                1);
        }

        /// <summary>
        ///     Pass left mouse button pressed input to the HSL widget.
        /// </summary>
        /// <returns>Returns true if the color was updated.</returns>
        public bool MouseLeftButtonPressed()
        {
            var point = Game1.getMousePosition(true);
            if (this._holding is not Hold.None || !this._area.Contains(point))
            {
                return false;
            }

            if (this._transparentArea.Contains(point))
            {
                this._holding = Hold.Transparent;
                this._isBlack = true;
                this._hueSlider.Coordinate = 0;
                this._saturationSlider.Coordinate = 0;
                this._luminanceSlider.Coordinate = 0;
                return true;
            }

            if (this._hueSlider.Area.Contains(point))
            {
                this._holding = Hold.Hue;
                this._hueSlider.Coordinate = point.Y;
                this.UpdateColor(this._isBlack);
                return true;
            }

            if (this._saturationSlider.Area.Contains(point))
            {
                this._holding = Hold.Saturation;
                this._saturationSlider.Coordinate = point.Y;
                this.UpdateColor();
                return true;
            }

            if (this._luminanceSlider.Area.Contains(point))
            {
                this._holding = Hold.Luminance;
                this._luminanceSlider.Coordinate = point.Y;
                this.UpdateColor();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Releases input from the HSL widget.
        /// </summary>
        /// <returns>Returns true if the color was updated.</returns>
        public bool MouseLeftButtonReleased()
        {
            if (this._holding is not Hold.None)
            {
                this._holding = Hold.None;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Pass mouse movement to the HSL widget.
        /// </summary>
        /// <returns>Returns true if the color was updated.</returns>
        public bool MouseHover()
        {
            var point = Game1.getMousePosition(true);
            switch (this._holding)
            {
                case Hold.Hue:
                    // Hue Selection
                    this._hueSlider.Coordinate = point.Y;
                    this.UpdateColor();
                    return true;
                case Hold.Saturation:
                    // Saturation
                    this._saturationSlider.Coordinate = point.Y;
                    this.UpdateColor();
                    return true;
                case Hold.Luminance:
                    // Luminance
                    this._luminanceSlider.Coordinate = point.Y;
                    this.UpdateColor();
                    return true;
                case Hold.None:
                case Hold.Transparent:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Pass mouse wheel input to the HSL widget.
        /// </summary>
        /// <param name="delta">The scrolling direction.</param>
        /// <returns>Returns true if the color was updated.</returns>
        public bool MouseWheelScroll(int delta)
        {
            var point = Game1.getMousePosition(true);
            if (this._holding is not Hold.None || !this._area.Contains(point))
            {
                return false;
            }

            delta = delta > 0 ? -10 : 10;
            if (this._hueSlider.Area.Contains(point))
            {
                this._hueSlider.Coordinate += delta;
                this.UpdateColor(this._isBlack);
                return true;
            }

            if (this._saturationSlider.Area.Contains(point))
            {
                this._saturationSlider.Coordinate += delta;
                this.UpdateColor();
                return true;
            }

            if (this._luminanceSlider.Area.Contains(point))
            {
                this._luminanceSlider.Coordinate += delta;
                this.UpdateColor();
                return true;
            }

            return false;
        }

        private void UpdateColor(bool updateAll = false)
        {
            if (updateAll)
            {
                this._isBlack = false;
                this._color = HSLColor.FromColor(this._colors[(int)MathHelper.Clamp(this._hueSlider.Value * (this._colors.Length - 1), 0, this._colors.Length - 1)]);
                this._saturationSlider.Value = this._color.S;
                this._luminanceSlider.Value = this._color.L;
            }
            else if (this._isBlack)
            {
                this._hueSlider.Value = 0;
                this._saturationSlider.Value = 0;
                this._luminanceSlider.Value = 0;
            }
            else
            {
                this._isBlack = false;
                this._color = new HSLColor
                {
                    H = this._hueSlider.Value,
                    S = this._saturationSlider.Value,
                    L = this._isBlack ? 0 : Math.Max(0.01f, this._luminanceSlider.Value),
                };
            }

            this._saturationBar.SetColors(this.GetSaturationShade);
            this._luminanceBar.SetColors(this.GetLuminanceShade);
        }

        private Color GetSaturationShade(float value)
        {
            return new HSLColor
            {
                H = this._color.H,
                S = value,
                L = Math.Max(0.01f, this._color.L),
            }.ToRgbColor();
        }

        private Color GetLuminanceShade(float value)
        {
            return new HSLColor
            {
                H = this._color.H,
                S = this._color.S,
                L = value,
            }.ToRgbColor();
        }

        private enum Hold
        {
            None,
            Hue,
            Saturation,
            Luminance,
            Transparent,
        }
    }
}