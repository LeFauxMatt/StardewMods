namespace StardewMods.FindAnything.Framework.UI;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;

/// <summary>Menu for searching for chests which contain specific items.</summary>
internal sealed class SearchOverlay : IClickableMenu
{
    private readonly SearchBar searchBar;

    /// <summary>Initializes a new instance of the <see cref="SearchOverlay" /> class.</summary>
    /// <param name="searchBar">The SearchBar instance to associate with the SearchOverlay.</param>
    public SearchOverlay(SearchBar searchBar) => this.searchBar = searchBar;

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        this.searchBar.Draw(b);
        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void receiveKeyPress(Keys key)
    {
        if (key is Keys.Enter or Keys.Escape)
        {
            this.exitThisMenuNoSound();
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.searchBar.LeftClick(x, y);
        if (this.searchBar.Selected)
        {
            Game1.activeClickableMenu = this;
            return;
        }

        this.searchBar.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        this.searchBar.RightClick(x, y);
        if (this.searchBar.Selected)
        {
            Game1.activeClickableMenu = this;
            return;
        }

        this.searchBar.Selected = false;
        this.exitThisMenuNoSound();
    }

    /// <summary>Shows the search overlay at the top of the screen.</summary>
    public void Show()
    {
        Game1.activeClickableMenu = this;
        this.searchBar.SetWidth(Math.Min(12 * Game1.tileSize, Game1.uiViewport.Width));
        var origin = Utility.getTopLeftPositionForCenteringOnScreen(
            this.searchBar.Area.Width,
            this.searchBar.Area.Height);

        this.searchBar.MoveTo((int)origin.X, Game1.tileSize);
        this.searchBar.Selected = true;
    }
}