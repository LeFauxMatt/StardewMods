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
    private readonly ClickableTextureComponent minus;
    private readonly ClickableTextureComponent plus;
    private readonly Range<int> range;
    private readonly TextBox textBox;

    private Rectangle bounds = Rectangle.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuantityField" /> class.
    /// </summary>
    /// <param name="cartItem">The CartItem for this QuantityField.</param>
    public QuantityField(ICartItem cartItem)
    {
        this.CartItem = cartItem;
        this.range = new(0, this.CartItem.Available);

        this.minus = new(
            new(0, 0, 7 * Game1.pixelZoom, 8 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(177, 345, 7, 8),
            Game1.pixelZoom);

        this.plus = new(
            new(0, 0, 7 * Game1.pixelZoom, 8 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(184, 345, 7, 8),
            Game1.pixelZoom);

        this.textBox = new(
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
        get => this.bounds;
        set
        {
            if (this.bounds.Equals(value))
            {
                return;
            }

            this.bounds = value;
            this.textBox.X = value.X;
            this.textBox.Y = value.Y;
            this.textBox.Width = value.Width;
            this.minus.bounds.X = value.X - 24;
            this.minus.bounds.Y = value.Y + 4;
            this.plus.bounds.X = value.X + value.Width + 2;
            this.plus.bounds.Y = value.Y + 4;
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
        get => string.IsNullOrWhiteSpace(this.textBox.Text) ? 0 : int.Parse(this.textBox.Text);
        set => this.textBox.Text = this.range.Clamp(value).ToString();
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

        this.textBox.Draw(b, false);
        this.minus.draw(b);
        this.plus.draw(b);
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
            this.textBox.Selected = false;
            return false;
        }

        this.textBox.SelectMe();
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

        if (this.minus.containsPoint(x, y))
        {
            --this.Quantity;
            return true;
        }

        if (this.plus.containsPoint(x, y))
        {
            ++this.Quantity;
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

        this.textBox.Text = string.Empty;
        return true;
    }
}