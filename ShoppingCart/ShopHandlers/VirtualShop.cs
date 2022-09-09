namespace StardewMods.ShoppingCart.ShopHandlers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.ShoppingCart.Models;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>
///     A virtual representation of a <see cref="ShopMenu" />.
/// </summary>
internal class VirtualShop
{
    private readonly IReflectedField<List<TemporaryAnimatedSprite>> _animations;
    private readonly int[] _cols;
    private readonly Dictionary<string, Point> _dims = new();
    private readonly int _lineHeight;
    private readonly ClickableTextureComponent _purchase;
    private readonly IReflectedField<float> _sellPercentage;
    private readonly List<CartItem> _toBuy = new();
    private readonly List<CartItem> _toSell = new();
    private readonly IReflectedMethod _tryToPurchaseItem;

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
        this._animations = helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.Menu, "animations");
        this._sellPercentage = helper.Reflection.GetField<float>(this.Menu, "sellPercentage");
        this._tryToPurchaseItem = helper.Reflection.GetMethod(this.Menu, "tryToPurchaseItem");
        this._lineHeight = 48;
        this._purchase = new(
            new(0, 0, 15 * Game1.pixelZoom, 14 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(280, 412, 15, 14),
            Game1.pixelZoom)
        {
            visible = false,
        };

        this._bounds = new(
            this.Menu.xPositionOnScreen + this.Menu.width + Game1.tileSize,
            this.Menu.yPositionOnScreen + IClickableMenu.borderWidth / 2 - IClickableMenu.spaceToClearTopBorder,
            Game1.tileSize * 9,
            this.Menu.height + this.Menu.inventory.height - Game1.tileSize - IClickableMenu.borderWidth / 2);

        const int minWidth = 128;
        this._dims.Add(I18n.Ui_ShoppingCart(), Game1.dialogueFont.MeasureString(I18n.Ui_ShoppingCart()).ToPoint());
        this._dims.Add(I18n.Ui_Available(), Game1.smallFont.MeasureString(I18n.Ui_Available()).ToPoint());
        this._dims.Add(I18n.Ui_Price(), Game1.smallFont.MeasureString(I18n.Ui_Price()).ToPoint());
        this._dims.Add(I18n.Ui_Quantity(), Game1.smallFont.MeasureString(I18n.Ui_Quantity()).ToPoint());
        this._dims.Add(I18n.Ui_Total(), Game1.smallFont.MeasureString(I18n.Ui_Total()).ToPoint());
        this._dims.Add(I18n.Ui_Checkout(), Game1.smallFont.MeasureString(I18n.Ui_Checkout()).ToPoint());

        this._cols = new int[3];
        this._cols[0] = Game1.tileSize / 2 + Math.Max(this._dims[I18n.Ui_Available()].X + 8, minWidth);
        this._cols[2] = this._bounds.Width - IClickableMenu.borderWidth * 2;
        this._cols[1] = this._cols[2] - Math.Max(this._dims[I18n.Ui_Quantity()].X + 8, minWidth + Game1.tileSize);
    }

    /// <summary>
    ///     Gets the actual ShopMenu this VirtualShop is attached to.
    /// </summary>
    public ShopMenu Menu { get; }

    private List<TemporaryAnimatedSprite> Animations => this._animations.GetValue();

    private long BuyTotal => this._toBuy.Sum(toBuy => toBuy.Total);

    private long GrandTotal => this.BuyTotal + this.SellTotal;

    private int Offset
    {
        get
        {
            if (this._bottomY > 0 && this._offset > this._bottomY - this._topY - this._bounds.Height + this._bounds.Top)
            {
                this._offset -= this._lineHeight;
            }

            if (this._offset < 0)
            {
                this._offset = 0;
            }

            return this._offset;
        }
        set => this._offset = value;
    }

    private float SellPercentage => this._sellPercentage.GetValue();

    private long SellTotal => this._toSell.Sum(toSell => toSell.Total);

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
            new(x + (this._bounds.Width - this._dims[I18n.Ui_ShoppingCart()].X) / 2 - IClickableMenu.borderWidth, y),
            Game1.textColor);
        y += this._dims[I18n.Ui_ShoppingCart()].Y;

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
            new(x + this._cols[1] - this._dims[I18n.Ui_Price()].X, y),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Quantity(),
            Game1.smallFont,
            new(x + this._cols[2] - this._dims[I18n.Ui_Quantity()].X - 32, y),
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
            Utility.drawTextWithShadow(b, I18n.Ui_Buying(), Game1.smallFont, new(x, y - this.Offset), Game1.textColor);
        }

        var text = $"{this.BuyTotal:n0}G";
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
            Utility.drawTextWithShadow(b, I18n.Ui_Selling(), Game1.smallFont, new(x, y - this.Offset), Game1.textColor);
        }

        text = $"{Math.Abs(this.SellTotal):n0}G";
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

        text = $"{this.GrandTotal:n0}G";
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
        width = (int)Game1.smallFont.MeasureString(I18n.Ui_Checkout()).X;
        this._bottomY = y + this._lineHeight;
        this._purchase.visible = y - this.Offset >= this._topY;

        if (!this._purchase.visible)
        {
            return;
        }

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Checkout(),
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
            if (this.TryCheckout() && ShoppingCart.CurrentShop is not null)
            {
                ShoppingCart.CurrentShop.Reset();
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
            cartItem.Quantity += !salable.IsInfiniteStock() && salable.Stack != int.MaxValue ? salable.Stack : 1;
            return true;
        }

        cartItem = CartItem.ToBuy(
            salable,
            !salable.IsInfiniteStock() && salable.Stack != int.MaxValue ? salable.Stack : 1,
            priceAndStock);
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
            cartItem.Quantity += item.Stack;
            return true;
        }

        cartItem = CartItem.ToSell(item, this.SellPercentage, Game1.player.Items);
        this._toSell.Add(cartItem);
        return true;
    }

    private bool PurchaseItem(CartItem toBuy)
    {
        var index = this.Menu.forSale.IndexOf(toBuy.Item);
        if (index == -1)
        {
            return false;
        }

        var quantity = toBuy.Quantity;
        var maxStack = toBuy.Item.maximumStackSize();
        while (quantity > 0)
        {
            var stack = Math.Min(maxStack, quantity);
            quantity -= stack;
            if (this._tryToPurchaseItem.Invoke<bool>(toBuy.Item, this.Menu.heldItem, stack, 0, 0, index))
            {
                this.Menu.itemPriceAndStock.Remove(toBuy.Item);
                this.Menu.forSale.RemoveAt(index);
                continue;
            }

            if (this.Menu.heldItem is null || !Game1.player.addItemToInventoryBool(this.Menu.heldItem as Item))
            {
                return false;
            }

            this.Menu.heldItem = null;
        }

        return true;
    }

    private void Reset()
    {
        this._toBuy.Clear();
        this._toSell.Clear();
    }

    private bool TryCheckout(bool test = false)
    {
        // Check affordability
        if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.Menu.currency) - this.GrandTotal < 0)
        {
            if (test)
            {
                return false;
            }

            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
            Game1.playSound("cancel");
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

        // Simulate selling
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
                    quantity -= inventory[i].Stack;
                    inventory.RemoveAt(i);
                    continue;
                }

                inventory[i].Stack -= quantity;
                break;
            }
        }

        // Simulate buying
        foreach (var toBuy in this._toBuy)
        {
            var index = this.Menu.forSale.IndexOf(toBuy.Item);
            if (index != -1 && this.Menu.canPurchaseCheck is not null && !this.Menu.canPurchaseCheck(index))
            {
                return false;
            }

            var quantity = toBuy.Quantity;
            var maxStack = toBuy.Item.maximumStackSize();
            foreach (var item in inventory)
            {
                if (!item.canStackWith(toBuy.Item))
                {
                    continue;
                }

                var stack = item.Stack;
                stack += quantity;
                if (stack > maxStack)
                {
                    item.Stack = maxStack;
                    quantity -= maxStack - stack;
                    continue;
                }

                item.Stack += quantity;
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

        if (test)
        {
            return true;
        }

        var snappedPosition = new Vector2(this._purchase.bounds.X, this._purchase.bounds.Y);

        // Sell items
        var coins = 2;
        foreach (var toSell in this._toSell)
        {
            var quantity = toSell.Quantity;
            coins += quantity / 8;
            for (var i = 0; i < Game1.player.MaxItems; ++i)
            {
                var item = Game1.player.Items.ElementAtOrDefault(i);
                if (item?.canStackWith(toSell.Item) != true)
                {
                    continue;
                }

                var stack = item.Stack;
                stack -= quantity;
                if (stack <= 0)
                {
                    Game1.player.Items[i] = null;
                    quantity -= item.Stack;
                    ShopMenu.chargePlayer(Game1.player, this.Menu.currency, toSell.Price * item.Stack);
                    continue;
                }

                item.Stack -= quantity;
                ShopMenu.chargePlayer(Game1.player, this.Menu.currency, toSell.Price * quantity);
                break;
            }

            // Vanilla game code
            ISalable? buyBackItem = null;
            if (this.Menu.CanBuyback())
            {
                buyBackItem = this.Menu.AddBuybackItem(toSell.Item, toSell.Price, toSell.Quantity);
            }

            if (toSell.Item is not SObject { Edibility: not -300 })
            {
                continue;
            }

            var clone = (Item)toSell.Item.GetSalableInstance();
            clone.Stack = toSell.Quantity;
            toSell.Quantity = 0;
            if (buyBackItem is not null && this.Menu.buyBackItemsToResellTomorrow.ContainsKey(buyBackItem))
            {
                this.Menu.buyBackItemsToResellTomorrow[buyBackItem].Stack += clone.Stack;
            }
            else if (Game1.currentLocation is ShopLocation shopLocation)
            {
                if (buyBackItem is not null)
                {
                    this.Menu.buyBackItemsToResellTomorrow[buyBackItem] = clone;
                }

                shopLocation.itemsToStartSellingTomorrow.Add(clone);
            }
        }

        coins = Math.Min(coins, 99);
        for (var i = 0; i < coins; ++i)
        {
            this.Animations.Add(
                new(
                    "TileSheets/debris",
                    new(Game1.random.Next(2) * 16, 64, 16, 16),
                    9999f,
                    1,
                    999,
                    snappedPosition + new Vector2(32f, 32f),
                    false,
                    false)
                {
                    alphaFade = 0.025f,
                    motion = new(Game1.random.Next(-3, 4), -4f),
                    acceleration = new(0f, 0.5f),
                    delayBeforeAnimationStart = i * 25,
                    scale = 2f,
                });

            this.Animations.Add(
                new(
                    "TileSheets/debris",
                    new(Game1.random.Next(2) * 16, 64, 16, 16),
                    9999f,
                    1,
                    999,
                    snappedPosition + new Vector2(32f, 32f),
                    false,
                    false)
                {
                    scale = 4f,
                    alphaFade = 0.025f,
                    delayBeforeAnimationStart = i * 50,
                    motion = Utility.getVelocityTowardPoint(
                        new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32),
                        new(
                            this.Menu.xPositionOnScreen - 36,
                            this.Menu.yPositionOnScreen + this.Menu.height - this.Menu.inventory.height - 16),
                        8f),
                    acceleration = Utility.getVelocityTowardPoint(
                        new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32),
                        new(
                            this.Menu.xPositionOnScreen - 36,
                            this.Menu.yPositionOnScreen + this.Menu.height - this.Menu.inventory.height - 16),
                        0.5f),
                });
        }

        // Buy items
        if (this._toBuy.Any(toBuy => !this.PurchaseItem(toBuy)))
        {
            return false;
        }

        if (this._toSell.Any())
        {
            Game1.playSound("sell");
        }

        if (this._toBuy.Any())
        {
            Game1.playSound("purchase");
        }

        return true;
    }
}