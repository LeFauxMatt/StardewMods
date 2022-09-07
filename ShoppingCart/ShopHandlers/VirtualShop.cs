namespace StardewMods.ShoppingCart.ShopHandlers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.ShoppingCart.Models;
using StardewValley.Menus;

/// <summary>
///     A virtual representation of a <see cref="ShopMenu" />.
/// </summary>
internal class VirtualShop
{
    private readonly int[] _cols;
    private readonly Point[] _dims;
    private readonly int _lineHeight;
    private readonly ClickableTextureComponent _purchase;
    private readonly IReflectedField<float> _sellPercentage;
    private readonly IList<CartItem> _toBuy = new List<CartItem>();
    private readonly IList<CartItem> _toSell = new List<CartItem>();

    private int _bottomY;
    private Rectangle _bounds;
    private int _offset;
    private int _topY;

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualShop" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="menu">The <see cref="ShopMenu" /> to attach to.</param>
    public VirtualShop(IModHelper helper, ShopMenu menu)
    {
        this.Menu = menu;
        this._sellPercentage = helper.Reflection.GetField<float>(this.Menu, "sellPercentage");
        this._lineHeight = 48;
        this._purchase = new(
            new(0, 0, 15 * Game1.pixelZoom, 14 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(280, 412, 15, 14),
            Game1.pixelZoom)
        {
            visible = false,
        };

        const int minWidth = 128;
        this._dims = new Point[4];
        this._dims[0] = Game1.dialogueFont.MeasureString(I18n.Ui_ShoppingCart()).ToPoint();
        this._dims[1] = Game1.smallFont.MeasureString(I18n.Ui_Available()).ToPoint();
        this._dims[2] = Game1.smallFont.MeasureString(I18n.Ui_Price()).ToPoint();
        this._dims[3] = Game1.smallFont.MeasureString(I18n.Ui_Quantity()).ToPoint();

        this._cols = new int[3];
        this._cols[0] = Game1.tileSize / 2 + Math.Max(this._dims[1].X + 8, minWidth);
        this._cols[2] = Game1.tileSize * 9 - IClickableMenu.borderWidth * 2;
        this._cols[1] = this._cols[2] - Math.Max(this._dims[3].X + 8, minWidth + Game1.tileSize);

        this._bounds = new(
            this.Menu.xPositionOnScreen + this.Menu.width + Game1.tileSize,
            this.Menu.yPositionOnScreen + IClickableMenu.borderWidth / 2 - IClickableMenu.spaceToClearTopBorder,
            Game1.tileSize * 9,
            this.Menu.height + this.Menu.inventory.height - Game1.tileSize - IClickableMenu.borderWidth / 2);
    }

    /// <summary>
    ///     Gets the actual ShopMenu this VirtualShop is attached to.
    /// </summary>
    public ShopMenu Menu { get; }

    private int Offset
    {
        get
        {
            if (this._offset < 0)
            {
                this._offset = 0;
            }

            if (this._bottomY > 0 && this._offset > this._bottomY - this._topY - this._bounds.Height + this._bounds.Top)
            {
                this._offset -= this._lineHeight;
            }

            return this._offset;
        }
        set => this._offset = value;
    }

    private float SellPercentage => this._sellPercentage.GetValue();

    /// <summary>
    ///     Draw the Shopping Cart.
    /// </summary>
    /// <param name="b">The <see cref="SpriteBatch" /> to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        if (!Game1.options.showMenuBackground)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
        }

        var x = this._bounds.X + IClickableMenu.borderWidth;
        var y = this._bounds.Y + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;

        Game1.drawDialogueBox(this._bounds.X, this._bounds.Y, this._bounds.Width, this._bounds.Height, false, true);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_ShoppingCart(),
            Game1.dialogueFont,
            new(x + (this._bounds.Width - this._dims[0].X) / 2 - IClickableMenu.borderWidth, y),
            Game1.textColor);
        y += this._dims[0].Y;

        // Draw Header
        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Available(),
            Game1.smallFont,
            new(x + Game1.tileSize / 2 + 8, y),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Price(),
            Game1.smallFont,
            new(x + this._cols[1] - this._dims[2].X, y),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Quantity(),
            Game1.smallFont,
            new(x + this._cols[2] - this._dims[3].X - 32, y),
            Game1.textColor);

        y += this._lineHeight;
        this._topY = y;

        // Draw Buying
        foreach (var toBuy in this._toBuy)
        {
            toBuy.Visible = y - this.Offset >= this._topY;
            toBuy.Draw(b, x, y - this.Offset, this._cols);

            y += this._lineHeight;
            if (y - this.Offset + this._lineHeight < this._bounds.Bottom - IClickableMenu.borderWidth)
            {
                continue;
            }

            this._bottomY = y + this._lineHeight;
            return;
        }

        // Draw Total Buying
        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(b, I18n.Ui_Buy(), Game1.smallFont, new(x, y - this.Offset), Game1.textColor);
        }

        var buyTotal = this._toBuy.Sum(toBuy => toBuy.Total);
        var text = $"{Math.Abs(buyTotal):n0}G";
        var width = (int)Game1.smallFont.MeasureString(text).X;

        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(
                b,
                text,
                Game1.smallFont,
                new(x + this._cols[1] - width, y - this.Offset),
                Game1.textColor);
        }

        y += this._lineHeight * 2;
        if (y - this.Offset + this._lineHeight >= this._bounds.Bottom - IClickableMenu.borderWidth)
        {
            this._bottomY = y + this._lineHeight;
            return;
        }

        // Draw Selling
        foreach (var toSell in this._toSell)
        {
            toSell.Visible = y - this.Offset >= this._topY;
            toSell.Draw(b, x, y - this.Offset, this._cols);

            y += this._lineHeight;
            if (y - this.Offset + this._lineHeight < this._bounds.Bottom - IClickableMenu.borderWidth)
            {
                continue;
            }

            this._bottomY = y + this._lineHeight;
            return;
        }

        // Draw Total Selling
        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(b, I18n.Ui_Sell(), Game1.smallFont, new(x, y - this.Offset), Game1.textColor);
        }

        var sellTotal = this._toSell.Sum(toSell => toSell.Total);
        text = $"{sellTotal:n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(
                b,
                text,
                Game1.smallFont,
                new(x + this._cols[1] - width, y - this.Offset),
                Game1.textColor);
        }

        y += this._lineHeight * 2;
        if (y - this.Offset + this._lineHeight >= this._bounds.Bottom - IClickableMenu.borderWidth)
        {
            this._bottomY = y + this._lineHeight;
            return;
        }

        // Draw Grand Total
        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(b, I18n.Ui_Total(), Game1.smallFont, new(x, y - this.Offset), Game1.textColor);
        }

        var total = buyTotal + sellTotal;
        text = $"{total:n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        if (y - this.Offset >= this._topY)
        {
            Utility.drawTextWithShadow(
                b,
                text,
                Game1.smallFont,
                new(x + this._cols[1] - width, y - this.Offset),
                Game1.textColor);
        }

        y += this._lineHeight;
        if (y - this.Offset + this._lineHeight >= this._bounds.Bottom - IClickableMenu.borderWidth)
        {
            this._bottomY = y + this._lineHeight;
            return;
        }

        // Draw purchase
        width = (int)Game1.smallFont.MeasureString(I18n.Ui_Purchase()).X;
        this._bottomY = y + this._lineHeight;
        this._purchase.visible = y - this.Offset >= this._topY;

        if (!this._purchase.visible)
        {
            return;
        }

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Purchase(),
            Game1.smallFont,
            new(x + this._cols[2] - 15 * Game1.pixelZoom - width - 12, y - this.Offset + 12),
            Game1.textColor);

        this._purchase.bounds.X = x + this._cols[2] - 15 * Game1.pixelZoom - 8;
        this._purchase.bounds.Y = y - this.Offset;
        this._purchase.draw(b);
    }

    /// <summary>
    ///     Attempt to perform a left click.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if left click was handled.</returns>
    public bool LeftClick(int x, int y)
    {
        // Purchase
        if (this._purchase.containsPoint(x, y))
        {
            if (this.TryPurchase())
            {
                Game1.playSound("sell");
                Game1.playSound("purchase");
            }
            else
            {
                Game1.playSound("cancel");
            }

            return true;
        }

        // Buying
        var toRemove = new List<CartItem>();
        var clicked = false;
        foreach (var cartItem in this._toBuy)
        {
            if (cartItem.LeftClick(x, y))
            {
                clicked = true;
            }

            if (cartItem.Total == 0)
            {
                toRemove.Add(cartItem);
            }

            if (clicked)
            {
                break;
            }
        }

        foreach (var cartItem in toRemove)
        {
            this._toBuy.Remove(cartItem);
        }

        if (clicked)
        {
            return true;
        }

        // Selling
        toRemove.Clear();
        foreach (var cartItem in this._toSell)
        {
            if (cartItem.LeftClick(x, y))
            {
                clicked = true;
            }

            if (cartItem.Total == 0)
            {
                toRemove.Add(cartItem);
            }

            if (clicked)
            {
                break;
            }
        }

        foreach (var cartItem in toRemove)
        {
            this._toSell.Remove(cartItem);
        }

        if (clicked)
        {
            return true;
        }

        // Get item to sell
        var sellSlot = this.Menu.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (sellSlot is not null)
        {
            var sellIndex = Convert.ToInt32(sellSlot.name);
            var toSell = this.Menu.inventory.actualInventory.ElementAtOrDefault(sellIndex);
            if (toSell is not null && this.TrySell(toSell))
            {
                return true;
            }
        }

        // Get item to buy
        var buySlot = this.Menu.forSaleButtons.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (buySlot is not null)
        {
            var buyIndex = this.Menu.currentItemIndex + this.Menu.forSaleButtons.IndexOf(buySlot);
            var toBuy = this.Menu.forSale.ElementAtOrDefault(buyIndex);
            if (toBuy is not null && this.TryBuy(toBuy))
            {
                return true;
            }
        }

        return this._bounds.Contains(x, y);
    }

    /// <summary>
    ///     Move an existing cart to another shop.
    /// </summary>
    /// <param name="other">The other shop to move items to.</param>
    public virtual void MoveItems(VirtualShop other)
    {
        other._toBuy.Clear();
        other._toSell.Clear();

        foreach (var toBuy in this._toBuy)
        {
            other._toBuy.Add(toBuy);
        }

        foreach (var toSell in this._toSell)
        {
            other._toSell.Add(toSell);
        }
    }

    /// <summary>
    ///     Scrolls the menu.
    /// </summary>
    /// <param name="direction">The direction to scroll.</param>
    /// <returns>Returns true if the menu was scrolled.</returns>
    public bool Scroll(int direction)
    {
        var (x, y) = Game1.getMousePosition(true);
        if (!this._bounds.Contains(x, y))
        {
            return false;
        }

        switch (direction)
        {
            case > 0:
                this.Offset -= this._lineHeight;
                Game1.playSound("shiny4");
                return true;
            case < 0:
                this.Offset += this._lineHeight;
                Game1.playSound("shiny4");
                return true;
        }

        return false;
    }

    public bool TryPurchase()
    {
        // Check affordability
        var total = this._toBuy.Sum(toBuy => toBuy.Total) + this._toSell.Sum(toSell => toSell.Total);
        if (Game1.player.Money + total < 0)
        {
            return false;
        }

        // Check space
        var inventory = new List<Item>();
        foreach (var item in Game1.player.Items)
        {
            if (item is null)
            {
                continue;
            }

            var clone = item.getOne();
            clone.Stack = item.Stack;
            inventory.Add(clone);
        }

        foreach (var toSell in this._toSell)
        {
            var quantity = toSell.Quantity;
            for (var i = inventory.Count - 1; i >= 0; i--)
            {
                if (!inventory[i].canStackWith(toSell.Item))
                {
                    continue;
                }

                var stack = inventory[i].Stack;
                stack -= quantity;
                if (stack <= 0)
                {
                    inventory.RemoveAt(i);
                    quantity -= inventory[i].Stack;
                    continue;
                }

                inventory[i].Stack -= quantity;
                break;
            }
        }

        foreach (var toBuy in this._toBuy)
        {
            var quantity = toBuy.Quantity;
            var maxStack = toBuy.Item.maximumStackSize();
            for (var i = 0; i < inventory.Count; i++)
            {
                if (!inventory[i].canStackWith(toBuy.Item))
                {
                    continue;
                }

                var stack = inventory[i].Stack;
                stack += quantity;
                if (stack > maxStack)
                {
                    inventory[i].Stack = maxStack;
                    quantity -= maxStack - stack;
                    continue;
                }

                inventory[i].Stack += quantity;
                break;
            }

            while (quantity > 0)
            {
                if (inventory.Count >= Game1.player.MaxItems)
                {
                    return false;
                }

                var clone = (Item)toBuy.Item.GetSalableInstance();
                clone.Stack = Math.Min(quantity, maxStack);
                quantity -= clone.Stack;
                inventory.Add(clone);
            }
        }

        return true;
    }

    /// <summary>
    ///     Attempt to add an item for buying.
    /// </summary>
    /// <param name="salable">The item to buy.</param>
    /// <returns>Returns true if item can be purchased.</returns>
    protected virtual bool TryBuy(ISalable? salable)
    {
        if (salable is null || !this.Menu.itemPriceAndStock.TryGetValue(salable, out var priceAndStock))
        {
            return false;
        }

        var cartItem = this._toBuy.FirstOrDefault(cartItem => cartItem.Item.canStackWith(salable));
        if (cartItem is not null)
        {
            return true;
        }

        cartItem = CartItem.ToBuy(salable, 1, priceAndStock);
        this._toBuy.Add(cartItem);
        return true;
    }

    /// <summary>
    ///     Attempt to add an item for selling.
    /// </summary>
    /// <param name="item">The item to sell.</param>
    /// <returns>Returns true if item can be sold.</returns>
    protected virtual bool TrySell(Item? item)
    {
        if (item is null || !this.Menu.highlightItemToSell(item))
        {
            return false;
        }

        var cartItem = this._toSell.FirstOrDefault(cartItem => cartItem.Item.canStackWith(item));
        if (cartItem is not null)
        {
            return true;
        }

        cartItem = CartItem.ToSell(item, this.SellPercentage, Game1.player.Items);
        this._toSell.Add(cartItem);
        return true;
    }
}