namespace FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using FuryCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class DropDownMenu : MenuComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContextMenu" /> class.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="onSelect"></param>
    public DropDownMenu(IList<string> values, int x, int y, Action<string> onSelect)
        : base(new(new(x, y, 0, 0), Game1.mouseCursors, OptionsDropDown.dropDownBGSource, 1f))
    {
        this.OnSelect = onSelect;

        var textBounds = values.Select(value => Game1.smallFont.MeasureString(value)).ToList();
        this.Component.bounds.Width = (int)textBounds.Max(textBound => textBound.X) + 16;
        this.Component.bounds.Height = (int)textBounds.Sum(textBound => textBound.Y) + 16;
        var textHeight = (int)textBounds.Max(textBound => textBound.Y);

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
    /// <param name="x"></param>
    /// <param name="y"></param>
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