namespace StardewMods.BetterChests.Framework.UI;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

/// <summary>
///     Menu for searching for chests which contain specific items.
/// </summary>
internal sealed class SearchBar : IClickableMenu
{
    private readonly ClickableComponent searchArea;
    private readonly TextBox searchField;
    private readonly ClickableTextureComponent searchIcon;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchBar" /> class.
    /// </summary>
    public SearchBar()
    {
        var texture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        this.width = Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width);
        this.height = texture.Height;

        var origin = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
        this.xPositionOnScreen = (int)origin.X;
        this.yPositionOnScreen = Game1.tileSize;

        this.searchField = new(texture, null, Game1.smallFont, Game1.textColor)
        {
            X = this.xPositionOnScreen,
            Y = this.yPositionOnScreen,
            Width = this.width,
            Selected = true,
        };

        this.searchArea = new(Rectangle.Empty, string.Empty)
        {
            visible = true,
            bounds = new(this.searchField.X, this.searchField.Y, this.searchField.Width, this.searchField.Height),
        };

        this.searchIcon = new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f)
        {
            bounds = new(this.searchField.X + this.searchField.Width - 38, this.searchField.Y + 6, 32, 32),
        };
    }

    /// <summary>
    ///     Gets the current search text.
    /// </summary>
    public string SearchText => this.searchField.Text;

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        this.searchField.Draw(b);
        this.searchIcon.draw(b);
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
                this.searchField.Text = string.Empty;
                this.exitThisMenuNoSound();
                return;
            default:
                return;
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.searchArea.containsPoint(x, y))
        {
            this.SetFocus();
            return;
        }

        this.searchField.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (this.searchArea.containsPoint(x, y))
        {
            this.SetFocus();
            this.searchField.Text = string.Empty;
            return;
        }

        this.searchField.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <summary>
    ///     Assigns focus to the search field.
    /// </summary>
    public void SetFocus()
    {
        Game1.activeClickableMenu = this;
        this.searchField.Selected = true;
    }
}