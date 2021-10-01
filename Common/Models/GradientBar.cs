namespace Common.Models
{
    using System;
    using Enums;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;

    /// <summary>
    ///     Represents a gradient bar as a list of rectangles and colors.
    /// </summary>
    internal class GradientBar
    {
        private readonly Axis _axis;
        private readonly Rectangle[] _bars;
        private readonly Color[] _colors;
        private readonly int _totalCells;
        private Rectangle _area;
        private int _cellSize;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GradientBar" /> class.
        /// </summary>
        /// <param name="axis">The direction to orient the gradient.</param>
        /// <param name="totalCells">The number of rectangles/colors.</param>
        public GradientBar(Axis axis, int totalCells)
        {
            this._axis = axis;
            this._totalCells = totalCells;
            this._bars = new Rectangle[this._totalCells];
            this._colors = new Color[this._totalCells];
        }

        /// <summary>
        ///     Gets or sets the rectangular area of the gradient.
        /// </summary>
        public Rectangle Area
        {
            get => this._area;
            set
            {
                this._area = value;
                this._cellSize = this._area.Height / this._totalCells;
                for (var i = 0; i < this._totalCells; i++)
                {
                    this._bars[i] = new Rectangle(
                        this._area.Left + this._axis switch
                        {
                            Axis.Horizontal => i * this._cellSize,
                            Axis.Vertical => 0,
                            _ => 0,
                        },
                        this._area.Top + this._axis switch
                        {
                            Axis.Horizontal => 0,
                            Axis.Vertical => i * this._cellSize,
                            _ => 0,
                        },
                        this._axis switch
                        {
                            Axis.Horizontal => this._cellSize,
                            Axis.Vertical => this._area.Width,
                            _ => 1,
                        },
                        this._axis switch
                        {
                            Axis.Horizontal => this._area.Height,
                            Axis.Vertical => this._cellSize,
                            _ => 1,
                        });
                }
            }
        }

        /// <summary>
        ///     Assign colors to the gradient.
        /// </summary>
        /// <param name="setColor">
        ///     Setter method that returns a Color based on a value between 0 and 1 representing the cell's
        ///     position in the gradient.
        /// </param>
        public void SetColors(Func<float, Color> setColor)
        {
            for (var i = 0; i < this._totalCells; i++)
            {
                this._colors[i] = setColor((float)i / this._totalCells);
            }
        }

        /// <summary>
        ///     Draws color bars from the gradient.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to draw bars to.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i < this._totalCells; i++)
            {
                spriteBatch.Draw(Game1.staminaRect, this._bars[i], this._colors[i]);
            }
        }
    }
}