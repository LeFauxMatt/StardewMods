namespace StardewMods.ShoppingCart.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.ShoppingCart;
using StardewMods.ShoppingCart.Framework.Models;
using StardewMods.ShoppingCart.UI;
using StardewValley.Locations;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class Shop : IShop
{
    /// <summary>
    ///     The width of the shopping cart menu.
    /// </summary>
    public const int MenuWidth = Game1.tileSize * 9;

    private const int LineHeight = 48;

    private readonly IReflectedField<List<TemporaryAnimatedSprite>> _animations;
    private readonly int[] _cols;
    private readonly Dictionary<string, Point> _dims = new();
    private readonly List<ICartItem> _items = new();
    private readonly ClickableTextureComponent _purchase;
    private readonly IDictionary<ICartItem, QuantityField> _quantityFields = new Dictionary<ICartItem, QuantityField>();
    private readonly IReflectedField<float> _sellPercentage;
    private readonly IReflectedMethod _tryToPurchaseItem;

    private int _bottomY;
    private int _currentY;
    private int _offset;
    private int _topY;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Shop" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="menu">The <see cref="ShopMenu" /> to attach to.</param>
    public Shop(IModHelper helper, ShopMenu menu)
    {
        this.Menu = menu;
        this._animations = helper.Reflection.GetField<List<TemporaryAnimatedSprite>>(this.Menu, "animations");
        this._sellPercentage = helper.Reflection.GetField<float>(this.Menu, "sellPercentage");
        this._tryToPurchaseItem = helper.Reflection.GetMethod(this.Menu, "tryToPurchaseItem");
        this._purchase = new(
            new(0, 0, 15 * Game1.pixelZoom, 14 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(280, 412, 15, 14),
            Game1.pixelZoom)
        {
            visible = false,
        };

        this.Bounds = new(
            this.Menu.xPositionOnScreen + this.Menu.width + Game1.tileSize + IClickableMenu.borderWidth,
            this.Menu.yPositionOnScreen + IClickableMenu.borderWidth / 2,
            Shop.MenuWidth - IClickableMenu.borderWidth * 2,
            this.Menu.inventory.yPositionOnScreen
          - this.Menu.yPositionOnScreen
          + this.Menu.inventory.height
          - IClickableMenu.borderWidth);

        const int minWidth = 128;
        this._dims.Add(I18n.Ui_ShoppingCart(), Game1.dialogueFont.MeasureString(I18n.Ui_ShoppingCart()).ToPoint());
        this._dims.Add(I18n.Ui_Available(), Game1.smallFont.MeasureString(I18n.Ui_Available()).ToPoint());
        this._dims.Add(I18n.Ui_Price(), Game1.smallFont.MeasureString(I18n.Ui_Price()).ToPoint());
        this._dims.Add(I18n.Ui_Quantity(), Game1.smallFont.MeasureString(I18n.Ui_Quantity()).ToPoint());
        this._dims.Add(I18n.Ui_Total(), Game1.smallFont.MeasureString(I18n.Ui_Total()).ToPoint());
        this._dims.Add(I18n.Ui_Checkout(), Game1.smallFont.MeasureString(I18n.Ui_Checkout()).ToPoint());

        this._cols = new int[3];
        this._cols[0] = Game1.tileSize / 2 + Math.Max(this._dims[I18n.Ui_Available()].X + 8, minWidth);
        this._cols[2] = this.Bounds.Width;
        this._cols[1] = this._cols[2] - Math.Max(this._dims[I18n.Ui_Quantity()].X + 8, minWidth + Game1.tileSize);
    }

    /// <inheritdoc />
    public Rectangle Bounds { get; }

    /// <inheritdoc />
    public ShopMenu Menu { get; }

    /// <inheritdoc />
    public IEnumerable<ICartItem> ToBuy => this._items.OfType<Buyable>();

    /// <inheritdoc />
    public IEnumerable<ICartItem> ToSell => this._items.OfType<Sellable>();

    private List<TemporaryAnimatedSprite> Animations => this._animations.GetValue();

    private long BuyTotal => this.ToBuy.Sum(item => item.Total);

    private long GrandTotal => this.BuyTotal - this.SellTotal;

    private int Offset
    {
        get
        {
            if (this._offset > this._bottomY - this._topY - this.Bounds.Height + this.Bounds.Top)
            {
                this._offset = this._bottomY == 0
                    ? 0
                    : Math.Max(0, this._bottomY - this._topY - this.Bounds.Height + this.Bounds.Top);
            }
            else if (this._offset < 0)
            {
                this._offset = 0;
            }

            return this._offset;
        }
        set => this._offset = value;
    }

    private float SellPercentage => this._sellPercentage.GetValue();

    private long SellTotal => this.ToSell.Sum(item => item.Total);

    /// <inheritdoc />
    public bool AddToCart(ISalable toBuy, int quantity)
    {
        if (!this.Menu.itemPriceAndStock.TryGetValue(toBuy, out var priceAndStock))
        {
            return false;
        }

        var field = this._quantityFields.Where(item => item.Key.Item.IsEquivalentTo(toBuy))
                        .Select(item => item.Value)
                        .FirstOrDefault();
        if (field is not null)
        {
            field.Quantity += quantity;
            return true;
        }

        this._items.Add(new Buyable(toBuy, quantity, priceAndStock));
        this._items.Sort();
        return true;
    }

    /// <inheritdoc />
    public bool AddToCart(Item toSell)
    {
        if (!this.Menu.highlightItemToSell(toSell))
        {
            return false;
        }

        var field = this._quantityFields.Where(item => item.Key.Item.IsEquivalentTo(toSell))
                        .Select(item => item.Value)
                        .FirstOrDefault();
        if (field is not null)
        {
            field.Quantity += toSell.Stack;
            return true;
        }

        this._items.Add(new Sellable(toSell, this.SellPercentage, Game1.player.Items));
        this._items.Sort();
        return true;
    }

    /// <summary>
    ///     Draws the ShoppingCart overlay.
    /// </summary>
    /// <param name="b">The SpriteBatch to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        this._currentY = this.Bounds.Y;

        Game1.drawDialogueBox(
            this.Bounds.X - IClickableMenu.borderWidth,
            this.Bounds.Y - IClickableMenu.spaceToClearTopBorder,
            this.Bounds.Width + IClickableMenu.borderWidth * 2,
            this.Bounds.Height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth,
            false,
            true);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_ShoppingCart(),
            Game1.dialogueFont,
            new(this.Bounds.Center.X - this._dims[I18n.Ui_ShoppingCart()].X / 2, this._currentY),
            Game1.textColor);
        this._currentY += this._dims[I18n.Ui_ShoppingCart()].Y;

        // Draw Header
        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Available(),
            Game1.smallFont,
            new(this.Bounds.X + Game1.tileSize / 2 + 8, this._currentY),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Price(),
            Game1.smallFont,
            new(this.Bounds.X + this._cols[1] - this._dims[I18n.Ui_Price()].X, this._currentY),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Quantity(),
            Game1.smallFont,
            new(this.Bounds.X + this._cols[2] - this._dims[I18n.Ui_Quantity()].X - 32, this._currentY),
            Game1.textColor);

        this._currentY += Shop.LineHeight;
        this._topY = this._currentY;

        // Draw Buying
        var prevCategory = string.Empty;
        foreach (var toBuy in this.ToBuy)
        {
            var category = (toBuy.Item as Item ?? (Item)toBuy.Item.GetSalableInstance()).getCategoryName();
            if (string.IsNullOrWhiteSpace(category))
            {
                category = I18n.Ui_OtherCategory();
            }

            if (!category.Equals(prevCategory, StringComparison.OrdinalIgnoreCase))
            {
                prevCategory = category;
                if (!this.DrawRow(
                        Shop.LineHeight,
                        y =>
                        {
                            Utility.drawTextWithShadow(
                                b,
                                category,
                                Game1.smallFont,
                                new(this.Bounds.X, y),
                                Game1.textColor);
                        }))
                {
                    this._bottomY = this._currentY + Shop.LineHeight;
                    return;
                }
            }

            if (!this._quantityFields.TryGetValue(toBuy, out var quantityField))
            {
                quantityField = new(toBuy);
                this._quantityFields.Add(toBuy, quantityField);
            }

            quantityField.IsVisible = this.DrawRow(
                Shop.LineHeight,
                y =>
                {
                    quantityField.Bounds = new(
                        this.Bounds.X + this._cols[1] + Game1.tileSize / 2,
                        y - 4,
                        this._cols[2] - this._cols[1] - Game1.tileSize,
                        Shop.LineHeight);
                    quantityField.Draw(b);
                    this.DrawItem(toBuy, b, this.Bounds.X, y);
                });

            if (!quantityField.IsVisible)
            {
                this._bottomY = this._currentY + Shop.LineHeight;
                return;
            }
        }

        // Draw Total Buying
        if (!this.DrawRow(
                Shop.LineHeight * 2,
                y =>
                {
                    var text = $"{this.BuyTotal:n0}G";
                    var width = (int)Game1.smallFont.MeasureString(text).X;
                    Utility.drawTextWithShadow(
                        b,
                        I18n.Ui_Buying(),
                        Game1.smallFont,
                        new(this.Bounds.X, y),
                        Game1.textColor);
                    Utility.drawTextWithShadow(
                        b,
                        text,
                        Game1.smallFont,
                        new(this.Bounds.X + this._cols[1] - width, y),
                        Game1.textColor);
                }))
        {
            this._bottomY = this._currentY + Shop.LineHeight;
            return;
        }

        // Draw Selling
        prevCategory = string.Empty;
        foreach (var toSell in this.ToSell)
        {
            var category = (toSell.Item as Item ?? (Item)toSell.Item.GetSalableInstance()).getCategoryName();
            if (string.IsNullOrWhiteSpace(category))
            {
                category = I18n.Ui_OtherCategory();
            }

            if (!category.Equals(prevCategory, StringComparison.OrdinalIgnoreCase))
            {
                prevCategory = category;
                if (!this.DrawRow(
                        Shop.LineHeight,
                        y =>
                        {
                            Utility.drawTextWithShadow(
                                b,
                                category,
                                Game1.smallFont,
                                new(this.Bounds.X, y),
                                Game1.textColor);
                        }))
                {
                    this._bottomY = this._currentY + Shop.LineHeight;
                    return;
                }
            }

            if (!this._quantityFields.TryGetValue(toSell, out var quantityField))
            {
                quantityField = new(toSell);
                this._quantityFields.Add(toSell, quantityField);
            }

            quantityField.IsVisible = this.DrawRow(
                Shop.LineHeight,
                y =>
                {
                    quantityField.Bounds = new(
                        this.Bounds.X + this._cols[1] + Game1.tileSize / 2,
                        y - 4,
                        this._cols[2] - this._cols[1] - Game1.tileSize,
                        Shop.LineHeight);
                    quantityField.Draw(b);
                    this.DrawItem(toSell, b, this.Bounds.X, y);
                });

            if (!quantityField.IsVisible)
            {
                this._bottomY = this._currentY + Shop.LineHeight;
                return;
            }
        }

        // Draw Total Selling
        if (!this.DrawRow(
                Shop.LineHeight * 2,
                y =>
                {
                    var text = $"{Math.Abs(this.SellTotal):n0}G";
                    var width = (int)Game1.smallFont.MeasureString(text).X;
                    Utility.drawTextWithShadow(
                        b,
                        I18n.Ui_Selling(),
                        Game1.smallFont,
                        new(this.Bounds.X, y),
                        Game1.textColor);
                    Utility.drawTextWithShadow(
                        b,
                        text,
                        Game1.smallFont,
                        new(this.Bounds.X + this._cols[1] - width, y),
                        Game1.textColor);
                }))
        {
            this._bottomY = this._currentY + Shop.LineHeight;
            return;
        }

        // Draw Grand Total
        if (!this.DrawRow(
                Shop.LineHeight,
                y =>
                {
                    var text = $"{this.GrandTotal:n0}G";
                    var width = (int)Game1.smallFont.MeasureString(text).X;
                    Utility.drawTextWithShadow(
                        b,
                        I18n.Ui_Total(),
                        Game1.smallFont,
                        new(this.Bounds.X, y),
                        Game1.textColor);
                    Utility.drawTextWithShadow(
                        b,
                        text,
                        Game1.smallFont,
                        new(this.Bounds.X + this._cols[1] - width, y),
                        Game1.textColor);
                }))
        {
            this._bottomY = this._currentY + Shop.LineHeight;
            return;
        }

        // Draw purchase
        this._purchase.visible = false;
        this.DrawRow(
            Shop.LineHeight,
            y =>
            {
                var width = (int)Game1.smallFont.MeasureString(I18n.Ui_Checkout()).X;
                this._purchase.bounds.X = this.Bounds.X + this._cols[2] - 15 * Game1.pixelZoom - 8;
                this._purchase.bounds.Y = y;
                this._purchase.visible = true;
                Utility.drawTextWithShadow(
                    b,
                    I18n.Ui_Checkout(),
                    Game1.smallFont,
                    new(this.Bounds.X + this._cols[2] - 15 * Game1.pixelZoom - width - 12, y + 12),
                    Game1.textColor);
                this._purchase.draw(b);
            });
        this._bottomY = this._currentY;
    }

    /// <summary>
    ///     Perform a hover action.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Hover(int x, int y)
    {
        if (!this.Bounds.Contains(x, y))
        {
            return;
        }

        var remove = new List<ICartItem>();
        foreach (var (cartItem, quantityField) in this._quantityFields)
        {
            if (!quantityField.Hover(x, y) && cartItem.Quantity == 0)
            {
                remove.Add(cartItem);
            }
        }

        foreach (var cartItem in remove)
        {
            this._items.Remove(cartItem);
            this._quantityFields.Remove(cartItem);
        }
    }

    /// <summary>
    ///     Attempt to perform a left click.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if left click was handled.</returns>
    public bool LeftClick(int x, int y)
    {
        if (!this.Bounds.Contains(x, y))
        {
            return false;
        }

        // Purchase
        if (this._purchase.containsPoint(x, y))
        {
            if (this.TryCheckout() && ModEntry.CurrentShop is not null)
            {
                ModEntry.CurrentShop.Reset();
            }
            else
            {
                Game1.playSound("cancel");
            }

            return true;
        }

        // Check for Quantity update
        var cartItem = this._quantityFields.Values.FirstOrDefault(quantityField => quantityField.LeftClick(x, y))
                           ?.CartItem;
        if (cartItem is null)
        {
            return true;
        }

        // Check for Cart item total
        if (cartItem.Quantity > 0)
        {
            return true;
        }

        this._items.Remove(cartItem);
        this._quantityFields.Remove(cartItem);
        return true;
    }

    /// <summary>
    ///     Clears the shop of all queued items to buy or sell.
    /// </summary>
    public void Reset()
    {
        this._items.Clear();
        this._quantityFields.Clear();
    }

    /// <summary>
    ///     Attempt to perform a right click.
    /// </summary>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    /// <returns>Returns true if right click was handled.</returns>
    public bool RightClick(int x, int y)
    {
        if (!this.Bounds.Contains(x, y))
        {
            return false;
        }

        // Check for Quantity update
        foreach (var quantityField in this._quantityFields.Values)
        {
            if (quantityField.RightClick(x, y))
            {
                break;
            }
        }

        return true;
    }

    /// <summary>
    ///     Scrolls the menu.
    /// </summary>
    /// <param name="direction">The direction to scroll.</param>
    /// <returns>Returns true if the menu was scrolled.</returns>
    public bool Scroll(int direction)
    {
        var (x, y) = Game1.getMousePosition(true);
        if (!this.Bounds.Contains(x, y))
        {
            return false;
        }

        switch (direction)
        {
            case > 0:
                this.Offset -= Shop.LineHeight;
                Game1.playSound("shiny4");
                return true;
            case < 0:
                this.Offset += Shop.LineHeight;
                Game1.playSound("shiny4");
                return true;
        }

        return false;
    }

    private void DrawItem(ICartItem cartItem, SpriteBatch b, int x, int y)
    {
        // Draw Item
        cartItem.Item.drawInMenu(b, new(x - 8, y - 8), 0.5f, 1f, 0.9f, StackDrawType.Hide, Color.White, false);

        // Draw Quality
        if (cartItem.Item is SObject { Quality: > 0 } obj)
        {
            b.Draw(
                Game1.mouseCursors,
                new Vector2(x + Game1.tileSize / 2 + 8, y - 8) + new Vector2(0, 26f),
                obj.Quality < 4 ? new(338 + (obj.Quality - 1) * 8, 400, 8, 8) : new(346, 392, 8, 8),
                Color.White,
                0f,
                new(4f, 4f),
                3f,
                SpriteEffects.None,
                1f);
        }

        // Draw Price
        var text = $"{Math.Abs(cartItem.Total):n0}G";
        var width = (int)Game1.smallFont.MeasureString(text).X;
        b.DrawString(Game1.smallFont, text, new(x + this._cols[1] - width, y), Game1.textColor);

        // Draw Available
        if (cartItem.Item.IsInfiniteStock() || cartItem.Available == int.MaxValue)
        {
            width = (int)Game1.smallFont.MeasureString("-").X;
            b.DrawString(Game1.smallFont, "-", new(x + this._cols[0] - width, y), Game1.textColor);
            return;
        }

        text = $"{cartItem.Available:n0}";
        width = (int)Game1.smallFont.MeasureString(text).X;
        b.DrawString(Game1.smallFont, text, new(x + this._cols[0] - width, y), Game1.textColor);
    }

    private bool DrawRow(int lineHeight, Action<int> draw)
    {
        if (this._currentY - this.Offset >= this._topY)
        {
            draw.Invoke(this._currentY - this.Offset);
        }

        this._currentY += lineHeight;
        return this._currentY - this.Offset + Shop.LineHeight <= this.Bounds.Bottom;
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

        // Simulate inventory
        var inventory = new Item?[Game1.player.MaxItems];
        var qiGems = Game1.player.QiGems;
        var walnuts = Game1.netWorldState.Value.GoldenWalnuts.Value;
        for (var i = 0; i < inventory.Length; ++i)
        {
            inventory[i] = Game1.player.Items.ElementAtOrDefault(i)?.getOne();
            if (inventory[i] is not { } item)
            {
                continue;
            }

            item.Stack = Game1.player.Items[i].Stack;
            item._GetOneFrom(Game1.player.Items[i]);
        }

        // Simulate selling
        if (this.ToSell.Any(item => !((Sellable)item).TrySell(inventory, this.Menu.currency, true)))
        {
            return false;
        }

        // Simulate buying
        if ((
                from item in this.ToBuy
                let index = this.Menu.forSale.IndexOf(item.Item)
                where !((Buyable)item).TestBuy(inventory, ref qiGems, ref walnuts, index, this.Menu.canPurchaseCheck)
                select item).Any())
        {
            return false;
        }

        if (test)
        {
            return true;
        }

        var snappedPosition = new Vector2(this._purchase.bounds.X, this._purchase.bounds.Y);

        // Sell items
        var coins = 2;
        foreach (var toSell in this.ToSell)
        {
            if (!((Sellable)toSell).TrySell(Game1.player.Items, this.Menu.currency))
            {
                return false;
            }

            coins += toSell.Quantity / 8;
            for (var i = 0; i < Game1.player.MaxItems; ++i)
            {
                if (Game1.player.Items.ElementAtOrDefault(i)?.Stack == 0)
                {
                    Game1.player.Items[i] = null;
                }
            }

            // Add BuyBack
            ISalable? buyBackItem = null;
            if (this.Menu.CanBuyback())
            {
                buyBackItem = this.Menu.AddBuybackItem(toSell.Item, toSell.Price, toSell.Quantity);
            }

            if (toSell.Item is not SObject { Edibility: not -300 } obj)
            {
                continue;
            }

            var clone = (Item)toSell.Item.GetSalableInstance();
            clone.Stack = toSell.Quantity;
            clone._GetOneFrom(obj);
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
        ModEntry.MakePurchase = true;
        foreach (var toBuy in this.ToBuy)
        {
            var index = this.Menu.forSale.IndexOf(toBuy.Item);
            if (index == -1)
            {
                ModEntry.MakePurchase = false;
                return false;
            }

            var quantity = toBuy.Quantity;
            var maxStack = Math.Max(1, toBuy.Item.maximumStackSize());
            while (quantity > 0)
            {
                var stack = Math.Min(maxStack, quantity);
                quantity -= stack;
                if (this._tryToPurchaseItem.Invoke<bool>(toBuy.Item, this.Menu.heldItem, stack, 0, 0, index))
                {
                    this.Menu.itemPriceAndStock.Remove(toBuy.Item);
                    this.Menu.forSale.RemoveAt(index);
                }

                if (this.Menu.heldItem is not null && !Game1.player.addItemToInventoryBool(this.Menu.heldItem as Item))
                {
                    ModEntry.MakePurchase = false;
                    return false;
                }

                this.Menu.heldItem = null;
            }

            toBuy.Quantity = 0;
        }

        ModEntry.MakePurchase = false;

        if (this.ToSell.Any())
        {
            Game1.playSound("sell");
        }

        if (this.ToBuy.Any())
        {
            Game1.playSound("purchase");
        }

        return true;
    }
}