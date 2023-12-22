namespace StardewMods.BetterChests.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Extensions;
using StardewMods.Common.Models;
using StardewValley.Menus;

/// <summary>Represents a slider control that allows the user to select a value within a range.</summary>
internal sealed class Slider
{
    private static readonly Range<float> Unit = new(0, 1);
    private readonly Rectangle area;
    private readonly ClickableComponent[] bars;

    private readonly Func<float> getMethod;

    private readonly int selected;
    private readonly Action<float> setMethod;
    private readonly Func<float, Color>? shadeFunction;
    private readonly Color[]? shades;
    private readonly Texture2D? texture;
    private readonly Range<int> track;

    /// <summary>Initializes a new instance of the <see cref="Slider" /> class.</summary>
    /// <param name="shadeFunction">A function that determines the color of each step based on its index.</param>
    /// <param name="getMethod">The getter for the value which the slider controls.</param>
    /// <param name="setMethod">The setter for the value which the slider controls.</param>
    /// <param name="area">The rectangular area in which the slider control will be displayed.</param>
    /// <param name="steps">The number of steps in the slider control.</param>
    public Slider(Func<float, Color> shadeFunction, Func<float> getMethod, Action<float> setMethod, Rectangle area, int steps)
        : this(getMethod, setMethod, area, steps)
    {
        this.shadeFunction = shadeFunction;
        this.shades = new Color[steps];

        for (var step = 0; step < steps; ++step)
        {
            this.shades[step] = shadeFunction(step);
        }
    }

    /// <summary>Initializes a new instance of the <see cref="Slider" /> class.</summary>
    /// <param name="getMethod">The getter for the value which the slider controls.</param>
    /// <param name="setMethod">The setter for the value which the slider controls.</param>
    /// <param name="texture">Top texture to use as the background.</param>
    /// <param name="area">The rectangular area in which the slider control will be displayed.</param>
    /// <param name="steps">The number of steps in the slider control.</param>
    public Slider(Texture2D texture, Func<float> getMethod, Action<float> setMethod, Rectangle area, int steps)
        : this(getMethod, setMethod, area, steps) =>
        this.texture = texture;

    private Slider(Func<float> getMethod, Action<float> setMethod, Rectangle area, int steps)
    {
        this.getMethod = getMethod;
        this.setMethod = setMethod;
        this.area = area;
        this.track = new Range<int>(area.Top, area.Bottom);
        this.bars = new ClickableComponent[steps];

        var height = area.Height / steps;
        for (var step = 0; step < steps; ++step)
        {
            this.bars[step] = new ClickableComponent(new Rectangle(area.Left, area.Top + (step * height), area.Width, height), string.Empty)
            {
                myID = step + 4343,
                upNeighborID = step > 0 ? step + 4343 - 1 : -1,
                downNeighborID = step < steps - 1 ? step + 4343 + 1 : -1,
            };
        }

        // Initialize selected
        var y = getMethod().Remap(Slider.Unit, this.track);
        if (y <= this.bars[0].bounds.Bottom)
        {
            if (this.selected == 0)
            {
                return;
            }

            this.selected = 0;
        }
        else if (y >= this.bars[^1].bounds.Top)
        {
            if (this.selected == this.bars.Length - 1)
            {
                return;
            }

            this.selected = this.bars.Length - 1;
        }
        else
        {
            for (var i = 1; i < this.bars.Length - 1; ++i)
            {
                if (y >= this.bars[i].bounds.Top)
                {
                    continue;
                }

                if (this.selected == i - 1)
                {
                    return;
                }

                this.selected = i - 1;
                break;
            }
        }
    }

    /// <summary>Draws the track and thumb to the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw track
        if (this.shades is not null)
        {
            for (var i = 0; i < this.bars.Length; ++i)
            {
                spriteBatch.Draw(Game1.staminaRect, this.bars[i].bounds, this.shades[i]);
            }
        }
        else if (this.texture is not null)
        {
            spriteBatch.Draw(this.texture, this.area, Color.White);
        }

        // Draw thumb
        spriteBatch.Draw(
            Game1.mouseCursors,
            new Rectangle(this.bars[this.selected].bounds.Left - 8, this.bars[this.selected].bounds.Center.Y - 8, 20, 16),
            new Rectangle(412, 495, 5, 4),
            Color.White,
            MathHelper.PiOver2,
            new Vector2(2.5f, 4f),
            SpriteEffects.None,
            1);
    }
}
