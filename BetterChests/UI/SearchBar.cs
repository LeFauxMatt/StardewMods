namespace StardewMods.BetterChests.UI;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Menu for searching for chests which contain specific items.
/// </summary>
internal class SearchBar : IClickableMenu
{
    private readonly IItemMatcher _itemMatcher;

    private readonly ClickableComponent _searchArea;

    private readonly TextBox _searchField;

    private readonly ClickableTextureComponent _searchIcon;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchBar" /> class.
    /// </summary>
    /// <param name="matcher">ItemMatcher for holding the selected item tags.</param>
    public SearchBar(IItemMatcher matcher)
    {
        this._itemMatcher = matcher;

        var texture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        this.width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
        this.height = texture.Height;

        var origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
        this.xPositionOnScreen = (int)origin.X;
        this.yPositionOnScreen = Game1.tileSize;

        this._searchField = new(texture, null, Game1.smallFont, Game1.textColor)
        {
            X = this.xPositionOnScreen,
            Y = this.yPositionOnScreen,
            Width = this.width,
            Selected = true,
            Text = this._itemMatcher.StringValue,
        };

        this._searchArea = new(Rectangle.Empty, string.Empty)
        {
            visible = true,
            bounds = new(this._searchField.X, this._searchField.Y, this._searchField.Width, this._searchField.Height),
        };

        this._searchIcon = new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f)
        {
            bounds = new(this._searchField.X + this._searchField.Width - 38, this._searchField.Y + 6, 32, 32),
        };
    }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        this._searchField.Draw(b);
        this._searchIcon.draw(b);
        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Enter:
                this.exitThisMenuNoSound();
                return;
            case Keys.Escape:
                this._searchField.Text = string.Empty;
                this.exitThisMenuNoSound();
                return;
            default:
                return;
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this._searchArea.containsPoint(x, y))
        {
            this._searchField.Selected = true;
            return;
        }

        this._searchField.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (this._searchArea.containsPoint(x, y))
        {
            this._searchField.Selected = true;
            this._searchField.Text = string.Empty;
            return;
        }

        this._searchField.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    protected override void cleanupBeforeExit()
    {
        if (!string.IsNullOrWhiteSpace(this._searchField.Text))
        {
            Log.Trace($"ChestFinder: {this._searchField.Text}");
            this._itemMatcher.StringValue = this._searchField.Text;
        }
        else
        {
            this._itemMatcher.Clear();
        }
    }
}