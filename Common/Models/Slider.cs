namespace Common.Models
{
    using Enums;
    using Microsoft.Xna.Framework;

    /// <summary>
    ///     A slider that relates a positional value to a floating value between 0 and 1.
    /// </summary>
    internal class Slider
    {
        private readonly Axis _axis;
        private readonly Range<int> _coordinateRange = new();
        private readonly Range<float> _valueRange = new(0, 1);
        private Rectangle _area;
        private int _coordinate;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Slider" /> class.
        /// </summary>
        /// <param name="axis">The axis that the slider is aligned to.</param>
        public Slider(Axis axis)
        {
            this._axis = axis;
        }

        /// <summary>Gets or sets the bounding area for the slider.</summary>
        public Rectangle Area
        {
            get => this._area;
            set
            {
                this._area = value;
                this._coordinateRange.Minimum = this._axis switch
                {
                    Axis.Horizontal => value.Left,
                    Axis.Vertical => value.Top,
                    _ => 0,
                };

                this._coordinateRange.Maximum = this._axis switch
                {
                    Axis.Horizontal => value.Right - 1,
                    Axis.Vertical => value.Bottom - 1,
                    _ => 0,
                };
            }
        }

        /// <summary>Gets or sets the relative value of the slider from 0 to 1.</summary>
        public float Value
        {
            get => this._valueRange.Clamp(((float)this._coordinate - this._coordinateRange.Minimum) / (this._coordinateRange.Maximum - this._coordinateRange.Minimum));
            set => this.Coordinate = (int)(value * (this._coordinateRange.Maximum - this._coordinateRange.Minimum) + this._coordinateRange.Minimum);
        }

        /// <summary>Gets or sets the position of the slider along its axis.</summary>
        public int Coordinate
        {
            get => this._coordinateRange.Clamp(this._coordinate);
            set => this._coordinate = value;
        }
    }
}