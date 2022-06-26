namespace StardewMods.Common.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     A dropdown for selecting a string from a list of values.
/// </summary>
internal class DropDownList
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DropDownList" /> class.
    /// </summary>
    /// <param name="values">The list of values to display.</param>
    /// <param name="x">The x-coordinate of the dropdown.</param>
    /// <param name="y">The y-coordinate of the dropdown.</param>
    /// <param name="callback">The action to call when a value is selected.</param>
    public DropDownList(IList<string> values, int x, int y, Action<string> callback)
    {
        this.Callback = callback;

        var textBounds = values.Select(value => Game1.smallFont.MeasureString(value).ToPoint()).ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);
        this.Bounds = new(x, y, textBounds.Max(textBound => textBound.X) + 16, textBounds.Sum(textBound => textBound.Y) + 16);
        this.Values = values.Select((value, index) => new ClickableComponent(new(this.Bounds.X + 8, this.Bounds.Y + 8 + textHeight * index, textBounds[index].X, textBounds[index].Y), value)).ToList();
    }

    private Rectangle Bounds { get; }

    private Action<string> Callback { get; }

    private List<ClickableComponent> Values { get; }

    /// <summary>
    ///     Draws the dropdown to screen.
    /// </summary>
    /// <param name="spriteBatch">The spritebatch to draw to.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.Bounds.X,
            this.Bounds.Y,
            this.Bounds.Width,
            this.Bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

        // Draw Values
        var (x, y) = Game1.getMousePosition(true);
        foreach (var value in this.Values)
        {
            if (value.bounds.Contains(x, y))
            {
                spriteBatch.Draw(Game1.staminaRect, new(value.bounds.X, value.bounds.Y, this.Bounds.Width - 16, value.bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
            }

            spriteBatch.DrawString(Game1.smallFont, value.name, new(value.bounds.X, value.bounds.Y), Game1.textColor);
        }
    }

    /// <summary>
    ///     Receive a left click action.
    /// </summary>
    /// <param name="x">The x-coordinate of the left click action.</param>
    /// <param name="y">The y-coordinate of the right click action.</param>
    public void LeftClick(int x, int y)
    {
        var value = this.Values.FirstOrDefault(value => value.bounds.Contains(x, y));
        if (value is not null)
        {
            this.Callback(value.name);
        }
    }
}