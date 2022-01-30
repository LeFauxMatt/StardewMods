namespace FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using FuryCore.Enums;
using FuryCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class DropDownMenu : CustomMenuComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DropDownMenu"/> class.
    /// </summary>
    /// <param name="values">The list of values to display.</param>
    /// <param name="x">The x-coordinate of the dropdown.</param>
    /// <param name="y">The y-coordinate of the dropdown.</param>
    /// <param name="onSelect">The action to call when a value is selected.</param>
    public DropDownMenu(IList<string> values, int x, int y, Action<string> onSelect)
        : base(ComponentArea.Custom)
    {
        var textBounds = values.Select(value => Game1.smallFont.MeasureString(value)).ToList();
        var textHeight = (int)textBounds.Max(textBound => textBound.Y);

        this.OnSelect = onSelect;
        this.Component = new(
            new(x, y, (int)textBounds.Max(textBound => textBound.X) + 16, (int)textBounds.Sum(textBound => textBound.Y) + 16),
            Game1.mouseCursors,
            OptionsDropDown.dropDownBGSource,
            1f);

        var (fx, fy) = new Vector2(this.Component.bounds.X + 8, this.Component.bounds.Y + 8);
        this.Values = values.Select((value, index) => new ClickableComponent(new((int)fx, (int)fy + (textHeight * index), (int)textBounds[index].X, (int)textBounds[index].Y), value)).ToList();
    }

    private Action<string> OnSelect { get; }

    private string SelectedValue { get; set; }

    private IList<ClickableComponent> Values { get; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(
            spriteBatch,
            this.Component.texture,
            this.Component.sourceRect,
            this.Component.bounds.X,
            this.Component.bounds.Y,
            this.Component.bounds.Width,
            this.Component.bounds.Height,
            Color.White,
            Game1.pixelZoom,
            this.Component.drawShadow,
            0.97f);

        // Draw Values
        foreach (var value in this.Values)
        {
            if (this.SelectedValue == value.name)
            {
                spriteBatch.Draw(Game1.staminaRect, new(value.bounds.X, value.bounds.Y, value.bounds.Width, value.bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
            }

            spriteBatch.DrawString(Game1.smallFont, value.name, new(value.bounds.X, value.bounds.Y), Game1.textColor);
        }
    }

    /// <inheritdoc />
    public override void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        var value = this.Values.FirstOrDefault(value => value.bounds.Contains(x, y));
        if (value is not null)
        {
            this.SelectedValue = value.name;
        }
    }

    /// <summary>
    ///     Pass left mouse button pressed input to the Context Menu.
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse.</param>
    /// <param name="y">The y-coordinate of the mouse.</param>
    public void LeftClick(int x, int y)
    {
        var value = this.Values.FirstOrDefault(value => value.bounds.Contains(x, y));
        if (value is not null)
        {
            this.SelectedValue = value.name;
            this.OnSelect(this.SelectedValue);
        }
    }
}