namespace StardewMods.ShoppingCart.UI;

using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewMods.Common.Models;
using StardewValley.Menus;

/// <summary>
///     The quantity of an item to buy or sell.
/// </summary>
internal sealed class QuantityField
{
    private readonly ClickableTextureComponent _minus;
    private readonly ClickableTextureComponent _plus;
    private readonly Range<int> _range;
    private readonly TextBox _textBox;

    private Rectangle _bounds = Rectangle.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuantityField" /> class.
    /// </summary>
    /// <param name="cartItem">The CartItem for this QuantityField.</param>
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

    /// <summary>
    ///     Gets or sets the bounds of the Quantity Field.
    /// </summary>
    public Rectangle Bounds
    {
        get => this._bounds;
        set
        {
            if (this._bounds.Equals(value))
            {
                return;
            }

            this._bounds = value;
            this._textBox.X = value.X;
            this._textBox.Y = value.Y;
            this._textBox.Width = value.Width;
            this._minus.bounds.X = value.X - 24;
            this._minus.bounds.Y = value.Y + 4;
            this._plus.bounds.X = value.X + value.Width + 2;
            this._plus.bounds.Y = value.Y + 4;
        }
    }

    /// <summary>
    ///     Gets the CartItem associated with this QuantityField.
    /// </summary>
    public ICartItem CartItem { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    ///     Gets or sets the quantity represented by the text box.
    /// </summary>
    public int Quantity
    {
        get => string.IsNullOrWhiteSpace(this._textBox.Text) ? 0 : int.Parse(this._textBox.Text);
        set => this._textBox.Text = this._range.Clamp(value).ToString();
    }

    /// <summary>
    ///     Draws the cart item to the screen.
    /// </summary>
    /// <param name="b">The sprite batch to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        if (this.Quantity != this.CartItem.Quantity)
        {
            this.CartItem.Quantity = this.Quantity;
        }

        this._textBox.Draw(b, false);
        this._minus.draw(b);
        this._plus.draw(b);
    }

    /// <summary>
    ///     Perform a hover action.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if a component was hovered.</returns>
    public bool Hover(int x, int y)
    {
        if (!this.IsVisible || !this.Bounds.Contains(x, y))
        {
            this._textBox.Selected = false;
            return false;
        }

        this._textBox.SelectMe();
        return true;
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
            this.Quantity--;
            return true;
        }

        if (this._plus.containsPoint(x, y))
        {
            this.Quantity++;
            return true;
        }

        return this.Bounds.Contains(x, y);
    }

    /// <summary>
    ///     Perform a right click action.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if a component was clicked.</returns>
    public bool RightClick(int x, int y)
    {
        if (!this.IsVisible || !this.Bounds.Contains(x, y))
        {
            return false;
        }

        this._textBox.Text = string.Empty;
        return true;
    }
}