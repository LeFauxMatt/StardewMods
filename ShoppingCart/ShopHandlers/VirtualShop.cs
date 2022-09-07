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
    private readonly ModConfig _config;
    private readonly int[] _dims;
    private readonly IModHelper _helper;
    private readonly int _lineHeight;
    private readonly ClickableTextureComponent _purchase;
    private readonly IReflectedField<float> _sellPercentage;
    private readonly IList<CartItem> _toBuy = new List<CartItem>();
    private readonly IList<CartItem> _toSell = new List<CartItem>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualShop" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="menu">The <see cref="ShopMenu" /> to attach to.</param>
    public VirtualShop(IModHelper helper, ModConfig config, ShopMenu menu)
    {
        this._helper = helper;
        this._config = config;
        this.Menu = menu;
        this._sellPercentage = this._helper.Reflection.GetField<float>(this.Menu, "sellPercentage");
        this._lineHeight = (int)Game1.smallFont.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ").Y;
        this._purchase = new(new(0, 0, 15, 14), Game1.mouseCursors, new(280, 412, 15, 14), Game1.pixelZoom);

        const int minWidth = 128;
        this._dims = new int[3];
        this._dims[0] = (int)Game1.smallFont.MeasureString(I18n.Ui_Available()).X;
        this._dims[1] = (int)Game1.smallFont.MeasureString(I18n.Ui_Price()).X;
        this._dims[2] = (int)Game1.smallFont.MeasureString(I18n.Ui_Quantity()).X;

        this._cols = new int[3];
        this._cols[0] = Game1.tileSize / 2 + Math.Max(this._dims[0] + 8, minWidth);
        this._cols[2] = Game1.tileSize * 9 - IClickableMenu.borderWidth * 2;
        this._cols[1] = this._cols[2] - Math.Max(this._dims[2] + 8, minWidth + Game1.tileSize);
    }

    /// <summary>
    ///     Gets the actual ShopMenu this VirtualShop is attached to.
    /// </summary>
    public ShopMenu Menu { get; }

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

        var x = this.Menu.xPositionOnScreen + this.Menu.width + IClickableMenu.borderWidth + Game1.tileSize;
        var y = this.Menu.yPositionOnScreen + IClickableMenu.borderWidth / 2;

        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - IClickableMenu.spaceToClearTopBorder,
            Game1.tileSize * 9,
            this.Menu.height,
            false,
            true);

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
            new(x + this._cols[1] - this._dims[1], y),
            Game1.textColor);

        Utility.drawTextWithShadow(
            b,
            I18n.Ui_Quantity(),
            Game1.smallFont,
            new(x + this._cols[2] - this._dims[2] - 32, y),
            Game1.textColor);

        y += this._lineHeight;

        // Draw Buying
        foreach (var toBuy in this._toBuy)
        {
            toBuy.Draw(b, x, y, this._cols);
            y += this._lineHeight;
        }

        // Draw Total Buying
        Utility.drawTextWithShadow(b, I18n.Ui_Buy(), Game1.smallFont, new(x, y), Game1.textColor);

        var buyTotal = this._toBuy.Sum(toBuy => toBuy.Total);
        var text = $"{Math.Abs(buyTotal):n0}G";
        var width = (int)Game1.smallFont.MeasureString(text).X;
        Utility.drawTextWithShadow(b, text, Game1.smallFont, new(x + this._cols[1] - width, y), Game1.textColor);
        y += this._lineHeight * 2;

        // Draw Selling
        foreach (var toSell in this._toSell)
        {
            toSell.Draw(b, x, y, this._cols);
            y += this._lineHeight;
        }

        // Draw Total Selling
        Utility.drawTextWithShadow(b, I18n.Ui_Sell(), Game1.smallFont, new(x, y), Game1.textColor);

        var sellTotal = this._toSell.Sum(toSell => toSell.Total);
        text = $"{sellTotal:n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        Utility.drawTextWithShadow(b, text, Game1.smallFont, new(x + this._cols[1] - width, y), Game1.textColor);
        y += this._lineHeight * 2;

        // Draw Grand Total
        Utility.drawTextWithShadow(b, I18n.Ui_Total(), Game1.smallFont, new(x, y), Game1.textColor);

        var total = buyTotal + sellTotal;
        text = $"{total:n0}G";
        width = (int)Game1.smallFont.MeasureString(text).X;
        Utility.drawTextWithShadow(b, text, Game1.smallFont, new(x + this._cols[1] - width, y), Game1.textColor);
        y += this._lineHeight * 2;

        // Draw purchase
        width = (int)Game1.smallFont.MeasureString(I18n.Ui_Purchase()).X;
        Utility.drawTextWithShadow(b, I18n.Ui_Purchase(), Game1.smallFont, new(x + this._cols[2] - 15 * Game1.pixelZoom - width - 8, y + 12), Game1.textColor);
        this._purchase.bounds.X = x + this._cols[2] - 15 * Game1.pixelZoom - 8;
        this._purchase.bounds.Y = y;
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
        if (buySlot is null)
        {
            return false;
        }

        var buyIndex = this.Menu.currentItemIndex + this.Menu.forSaleButtons.IndexOf(buySlot);
        var toBuy = this.Menu.forSale.ElementAtOrDefault(buyIndex);
        return toBuy is not null && this.TryBuy(toBuy);
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
    ///     Return items from cart.
    /// </summary>
    public virtual void ReturnItems() { }

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

        this._toBuy.Add(CartItem.ToBuy(salable, 1, priceAndStock));
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

        this._toSell.Add(CartItem.ToSell(item, this.SellPercentage, Game1.player.Items));
        return true;
    }
}