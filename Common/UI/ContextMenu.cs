namespace Common.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

internal class ContextMenu : ClickableComponent
{
    private readonly Func<string, bool> _leftClickSelect;
    private readonly IList<ClickableComponent> _values;
    private string _selectedValue;

    public ContextMenu(IList<string> values, int x, int y, Func<string, bool> leftClickSelect)
        : base(Rectangle.Empty, nameof(ContextMenu))
    {
        this._leftClickSelect = leftClickSelect;

        var textBounds = values.Select(value => Game1.smallFont.MeasureString(value)).ToList();
        this.bounds.X = x;
        this.bounds.Y = y;
        this.bounds.Width = (int)textBounds.Max(textBound => textBound.X) + 16;
        this.bounds.Height = (int)textBounds.Sum(textBound => textBound.Y);

        var textPos = new Vector2(this.bounds.X + 8, this.bounds.Y);
        this._values = new List<ClickableComponent>();
        for (var i = 0; i < values.Count; i++)
        {
            this._values.Add(new(new((int)textPos.X, (int)textPos.Y, (int)textBounds[i].X, (int)textBounds[i].Y), values[i]));
            textPos.Y += textBounds[i].Y;
        }
    }

    /// <summary>
    ///     Pass left mouse button pressed input to the Context Menu.
    /// </summary>
    /// <returns>Returns true if the color was updated.</returns>
    public bool LeftClick(int x = -1, int y = -1)
    {
        var point = x == -1 || y == -1
            ? Game1.getMousePosition(true)
            : new(x, y);

        var value = this._values.FirstOrDefault(value => value.bounds.Contains(point));
        if (value is not null)
        {
            this._selectedValue = value.name;
            this._leftClickSelect(this._selectedValue);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Pass mouse movement to the Context Menu.
    /// </summary>
    /// <returns>Returns true if selection was updated.</returns>
    public bool OnHover(int x = -1, int y = -1)
    {
        var point = x == -1 || y == -1
            ? Game1.getMousePosition(true)
            : new(x, y);

        var value = this._values.FirstOrDefault(value => value.bounds.Contains(point));
        if (value is not null)
        {
            this._selectedValue = value.name;
            return true;
        }

        return false;
    }

    public void Draw(SpriteBatch b)
    {
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            this.bounds.X,
            this.bounds.Y,
            this.bounds.Width,
            this.bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            0.97f);

        // Draw Values
        foreach (var value in this._values)
        {
            if (this._selectedValue == value.name)
            {
                b.Draw(Game1.staminaRect, new(this.bounds.X, value.bounds.Y, this.bounds.Width, value.bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
            }

            b.DrawString(Game1.smallFont, value.name, new(value.bounds.X, value.bounds.Y), Game1.textColor);
        }
    }
}