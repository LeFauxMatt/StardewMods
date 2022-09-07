namespace StardewMods.ShoppingCart.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

internal class CartItem
{
    private readonly int _available;
    private readonly ClickableTextureComponent _minus;
    private readonly ClickableTextureComponent _plus;
    private readonly int _price;
    private readonly TextBox _quantityField;

    private CartItem(ISalable item, int price, int quantity, int available)
    {
        this.Item = item;
        this._price = price;
        this.Quantity = quantity;
        this._available = available;

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

        this._quantityField = new(
            Game1.content.Load<Texture2D>("LooseSprites/textBox"),
            null,
            Game1.smallFont,
            Game1.textColor)
        {
            numbersOnly = true,
            Text = quantity.ToString(),
        };
    }

    public ISalable Item { get; }

    public int Quantity { get; set; }

    public int Total => this._price * this.Quantity;

    public bool Visible { get; set; }

    public static CartItem ToBuy(ISalable item, int quantity, int[] priceAndStock)
    {
        return new(item, -priceAndStock[0], quantity, priceAndStock[1]);
    }

    public static CartItem ToSell(Item item, float sellPercentage, IEnumerable<Item?> inventory)
    {
        var copy = item.GetSalableInstance();
        var price = copy switch
        {
            SObject obj => (int)(obj.sellToStorePrice() * sellPercentage),
            _ => (int)(copy.salePrice() / 2f * sellPercentage),
        };

        var available = inventory.OfType<Item>()
                                 .Where(inventoryItem => inventoryItem.canStackWith(item))
                                 .Sum(inventoryItem => inventoryItem.Stack > 0 ? inventoryItem.Stack : 1);
        return new(copy, price, item.Stack, available);
    }

    public void Draw(SpriteBatch b, int x, int y, int[] cols)
    {
        if (!this.Visible)
        {
            return;
        }

        int width;
        string text;

        this.Item.drawInMenu(b, new(x - 8, y - 8), 0.5f, 1f, 0.9f, StackDrawType.Hide, Color.White, false);

        if (this._available < int.MaxValue)
        {
            text = $"{this._available:n0}";
            width = (int)Game1.smallFont.MeasureString(text).X;
            b.DrawString(Game1.smallFont, text, new(x + cols[0] - width, y), Game1.textColor);
        }

        if (this._quantityField.Text != this.Quantity.ToString())
        {
            this.Quantity = string.IsNullOrWhiteSpace(this._quantityField.Text)
                ? 0
                : Convert.ToInt32(this._quantityField.Text);
            if (this.Quantity > this._available)
            {
                this.Quantity = this._available;
            }
        }

        if (this._quantityField.Text != this.Quantity.ToString())
        {
            this._quantityField.Text = this.Quantity.ToString();
        }

        text = $"{Math.Abs(this.Total):n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        b.DrawString(Game1.smallFont, text, new(x + cols[1] - width, y), Game1.textColor);

        this._quantityField.X = x + cols[1] + 32;
        this._quantityField.Y = y - 4;
        this._quantityField.Width = cols[2] - cols[1] - 64;
        this._quantityField.Draw(b, false);

        this._minus.bounds.X = x + cols[1] + 8;
        this._minus.bounds.Y = y;
        this._plus.bounds.X = x + cols[2] - 30;
        this._plus.bounds.Y = y;
        this._minus.draw(b);
        this._plus.draw(b);
    }

    public bool LeftClick(int x, int y)
    {
        if (!this.Visible)
        {
            return false;
        }

        if (this._minus.containsPoint(x, y))
        {
            this.Quantity--;
            this._quantityField.Text = this.Quantity.ToString();
            return true;
        }

        if (this._plus.containsPoint(x, y))
        {
            this.Quantity++;
            this._quantityField.Text = this.Quantity.ToString();
            return true;
        }

        var bounds = new Rectangle(
            this._quantityField.X,
            this._quantityField.Y,
            this._quantityField.Width,
            this._quantityField.Height);
        if (!bounds.Contains(x, y))
        {
            this._quantityField.Selected = false;
            return false;
        }

        this._quantityField.SelectMe();
        return true;
    }
}