namespace StardewMods.FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewValley;

/// <summary>
///     Represents a gradient bar as a list of rectangles and colors.
/// </summary>
internal class GradientBar
{
    private const int DefaultCells = 16;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GradientBar" /> class.
    /// </summary>
    /// <param name="orientation">The direction to orient the gradient.</param>
    /// <param name="area">The rectangular area of the gradient bar.</param>
    /// <param name="getColor">Function that returns a color shade based an an input value between 0 and 1.</param>
    /// <param name="cells">The number of cells to generate for the gradient bar.</param>
    public GradientBar(Axis orientation, Rectangle area, Func<float, Color> getColor, int cells = GradientBar.DefaultCells)
    {
        this.Orientation = orientation;
        this.Area = area;
        this.Cells = cells;
        this.GetColor = getColor;

        var cellSize = this.Orientation switch { Axis.Horizontal => area.Width, Axis.Vertical => area.Height, _ => 0 } / this.Cells;
        this.Bars = Enumerable.Range(0, this.Cells).Select(i => new Rectangle(
            this.Area.Left + this.Orientation switch { Axis.Horizontal => i * cellSize, Axis.Vertical => 0, _ => 0 },
            this.Area.Top + this.Orientation switch { Axis.Horizontal => 0, Axis.Vertical => i * cellSize, _ => 0 },
            this.Orientation switch { Axis.Horizontal => cellSize, Axis.Vertical => this.Area.Width, _ => 1 },
            this.Orientation switch { Axis.Horizontal => this.Area.Height, Axis.Vertical => cellSize, _ => 1 })).ToList();
    }

    /// <summary>
    ///     Gets the rectangular area of the gradient bar.
    /// </summary>
    public Rectangle Area { get; }

    private IList<Rectangle> Bars { get; }

    private int Cells { get; }

    private Func<float, Color> GetColor { get; }

    private Axis Orientation { get; }

    /// <summary>
    ///     Draws color bars from the gradient.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to draw bars to.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var (bar, color) in this.Bars.Select((bar, index) => (bar, this.GetColor((float)index / this.Cells))))
        {
            spriteBatch.Draw(Game1.staminaRect, bar, color);
        }
    }
}