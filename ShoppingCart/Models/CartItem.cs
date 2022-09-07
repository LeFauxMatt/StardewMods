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
    private readonly ISalable _item;
    private readonly ClickableTextureComponent _minus;
    private readonly ClickableTextureComponent _plus;
    private readonly int _price;
    private readonly int _quantity;

    private CartItem(ISalable item, int price, int quantity, int available)
    {
        this._item = item;
        this._price = price;
        this._quantity = quantity;
        this._available = available;
        this._minus = new(new(0, 0, 7, 8), Game1.mouseCursors, new(177, 345, 7, 8), Game1.pixelZoom);
        this._plus = new(new(0, 0, 7, 8), Game1.mouseCursors, new(184, 345, 7, 8), Game1.pixelZoom);
    }

    public int Total => this._price * this._quantity;

    public static CartItem ToBuy(ISalable item, int quantity, int[] priceAndStock)
    {
        return new(item, -priceAndStock[0], quantity, priceAndStock[1]);
    }

    public static CartItem ToSell(Item item, float sellPercentage, IEnumerable<Item?> inventory)
    {
        var copy = item.getOne();
        var price = copy switch
        {
            SObject obj => (int)(obj.sellToStorePrice() * sellPercentage),
            _ => (int)(copy.salePrice() / 2f * sellPercentage),
        };
        var available = inventory.Sum(inventoryItem => inventoryItem?.Stack ?? 0);
        return new(copy, price, item.Stack, available);
    }

    public void Draw(SpriteBatch b, int x, int y, int[] cols)
    {
        int width;
        string text;

        this._item.drawInMenu(b, new(x - 8, y - 8), 0.5f, 1f, 0.9f, StackDrawType.Hide, Color.White, false);

        if (this._available < int.MaxValue)
        {
            text = $"{this._available:n0}";
            width = (int)Game1.smallFont.MeasureString(text).X;
            b.DrawString(Game1.smallFont, text, new(x + cols[0] - width, y), Game1.textColor);
        }

        text = $"{Math.Abs(this.Total):n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        b.DrawString(Game1.smallFont, text, new(x + cols[1] - width, y), Game1.textColor);

        text = $"{this._quantity:n0}";
        width = (int)Game1.smallFont.MeasureString(text).X;
        b.DrawString(Game1.smallFont, text, new(x + cols[2] - width - 40, y), Game1.textColor);

        this._minus.bounds.X = x + cols[1] + 32;
        this._minus.bounds.Y = y;
        this._plus.bounds.X = x + cols[2] - 32;
        this._plus.bounds.Y = y;
        this._minus.draw(b);
        this._plus.draw(b);
    }
}