namespace StardewMods.ShoppingCart.UI;

using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewMods.Common.Models;
using StardewValley.Menus;

internal class QuantityField
{
    private readonly ClickableTextureComponent _minus;
    private readonly ClickableTextureComponent _plus;
    private readonly Range<int> _range;
    private readonly TextBox _textBox;

    public QuantityField(ICartItem cartItem)
    {
        this.CartItem = cartItem;
        this._range = new(0, this.CartItem.Available);

        this._minus = new(
            new(0, 0, 7 * Game1.pixelZoom, 8 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(177, 345, 7, 8),
            Game1.pixelZoom);

        this._plus = new(
            new(0, 0, 7 * Game1.pixelZoom, 8 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(184, 345, 7, 8),
            Game1.pixelZoom);

        this._textBox = new(
            Game1.content.Load<Texture2D>("LooseSprites/textBox"),
            null,
            Game1.smallFont,
            Game1.textColor)
        {
            numbersOnly = true,
            Text = this.CartItem.Quantity.ToString(),
        };
    }

    public ICartItem CartItem { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    private Rectangle Bounds => new(this._textBox.X, this._textBox.Y, this._textBox.Width, this._textBox.Height);

    /// <summary>
    ///     Draws the cart item to the screen.
    /// </summary>
    /// <param name="b">The sprite batch to draw to.</param>
    /// <param name="x">The x-coordinate to draw the item at.</param>
    /// <param name="y">The y-coordinate to draw the item at.</param>
    /// <param name="cols">The column widths.</param>
    public void Draw(SpriteBatch b, int x, int y, int[] cols)
    {
        if (this._textBox.Text != this.CartItem.Quantity.ToString())
        {
            this.CartItem.Quantity = string.IsNullOrWhiteSpace(this._textBox.Text)
                ? 0
                : this._range.Clamp(int.Parse(this._textBox.Text));
        }

        this._textBox.X = x + cols[1] + 32;
        this._textBox.Y = y - 4;
        this._textBox.Width = cols[2] - cols[1] - 64;

        this._minus.bounds.X = x + cols[1] + 8;
        this._minus.bounds.Y = y;

        this._plus.bounds.X = x + cols[2] - 30;
        this._plus.bounds.Y = y;

        this._textBox.Draw(b, false);
        this._minus.draw(b);
        this._plus.draw(b);
    }

    /// <summary>
    ///     Perform a left click action.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if a component was clicked.</returns>
    public bool LeftClick(int x, int y)
    {
        if (!this.IsVisible)
        {
            return false;
        }

        if (this._minus.containsPoint(x, y))
        {
            this.CartItem.Quantity--;
        }
        else if (this._plus.containsPoint(x, y))
        {
            this.CartItem.Quantity++;
        }
        else if (!this.Bounds.Contains(x, y))
        {
            this._textBox.Selected = false;
            return false;
        }

        this._textBox.Text = this.CartItem.Quantity.ToString();
        this._textBox.SelectMe();
        return true;
    }
}