namespace StardewMods.BetterChests.Framework.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>
///     A dropdown for selecting a string from a list of values.
/// </summary>
internal sealed class DropDownList : IClickableMenu
{
    private readonly Rectangle bounds;
    private readonly Action<string?> callback;
    private readonly Dictionary<string, string> localValues;
    private readonly List<ClickableComponent> values;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DropDownList" /> class.
    /// </summary>
    /// <param name="values">The list of values to display.</param>
    /// <param name="x">The x-coordinate of the dropdown.</param>
    /// <param name="y">The y-coordinate of the dropdown.</param>
    /// <param name="callback">The action to call when a value is selected.</param>
    /// <param name="translation">Translations from the i18n folder.</param>
    public DropDownList(IList<string> values, int x, int y, Action<string?> callback, ITranslationHelper translation)
        : base(x, y, 0, 0)
    {
        this.callback = callback;
        this.localValues = values.ToDictionary(
            value => value,
            value => translation.Get($"tag.{value}").Default(value).ToString());
        var textBounds = this.localValues.Values.Select(value => Game1.smallFont.MeasureString(value).ToPoint())
            .ToList();
        var textHeight = textBounds.Max(textBound => textBound.Y);
        this.width = textBounds.Max(textBound => textBound.X) + 16;
        this.height = textBounds.Sum(textBound => textBound.Y) + 16;
        this.bounds = new(x, y, this.width, this.height);
        this.values = values.Select(
                (value, index) => new ClickableComponent(
                    new(
                        this.bounds.X + 8,
                        this.bounds.Y + 8 + (textHeight * index),
                        this.bounds.Width,
                        textBounds[index].Y),
                    value))
            .ToList();
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        DropDownList.drawTextureBox(
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
        var (x, y) = Game1.getMousePosition(true);
        foreach (var value in this.values)
        {
            if (value.bounds.Contains(x, y))
            {
                b.Draw(
                    Game1.staminaRect,
                    new(value.bounds.X, value.bounds.Y, this.bounds.Width - 16, value.bounds.Height),
                    new Rectangle(0, 0, 1, 1),
                    Color.Wheat,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0.975f);
            }

            b.DrawString(
                Game1.smallFont,
                this.localValues[value.name],
                new(value.bounds.X, value.bounds.Y),
                Game1.textColor);
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var value = this.values.FirstOrDefault(value => value.bounds.Contains(x, y));
        this.callback(value?.name);
    }
}