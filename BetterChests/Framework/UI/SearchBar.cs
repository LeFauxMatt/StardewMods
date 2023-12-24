namespace StardewMods.BetterChests.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>Represents a search overlay control that allows the user to input text.</summary>
internal sealed class SearchBar
{
    private const int CountdownTimer = 20;
    private readonly ClickableTextureComponent icon;

    private readonly Action<string> setMethod;
    private readonly TextBox textBox;
    private Rectangle area;
    private string previousText;
    private int timeout;

    /// <summary>Initializes a new instance of the <see cref="SearchBar" /> class.</summary>
    /// <param name="texture">The texture used for the search overlay.</param>
    /// <param name="getMethod">The function that gets the current search text.</param>
    /// <param name="setMethod">The action that sets the search text.</param>
    /// <param name="area">The area in which the search overlay is displayed.</param>
    public SearchBar(Texture2D texture, Func<string> getMethod, Action<string> setMethod, Rectangle area)
    {
        this.previousText = getMethod();
        this.setMethod = setMethod;
        this.area = area;
        this.textBox = new TextBox(texture, null, Game1.smallFont, Game1.textColor)
        {
            X = area.X,
            Y = area.Y,
            Width = area.Width,
            Selected = true,
        };

        this.icon = new ClickableTextureComponent(
            new Rectangle(area.X + this.textBox.Width - 38, area.Y + 6, 32, 32),
            Game1.mouseCursors,
            new Rectangle(80, 0, 13, 13),
            2.5f);
    }

    /// <summary>Gets the area in which the search bar is displayed.</summary>
    public Rectangle Area => this.area;

    /// <summary>Gets or sets a value indicating whether the search bar is currently selected.</summary>
    public bool Selected
    {
        get => this.textBox.Selected;
        set => this.textBox.Selected = value;
    }

    /// <summary>Moves the search bar to the specified coordinates.</summary>
    /// <param name="x">The x-coordinate to move to.</param>
    /// <param name="y">The y-coordinate to move to.</param>
    public void MoveTo(int x, int y)
    {
        this.area.X = x;
        this.area.Y = y;
        this.textBox.X = x;
        this.textBox.Y = y;
        this.icon.bounds.X = x + this.textBox.Width - 38;
        this.icon.bounds.Y = y + 6;
    }

    /// <summary>Sets the width of the search bar.</summary>
    /// <param name="width">The width to set.</param>
    public void SetWidth(int width)
    {
        this.area.Width = width;
        this.textBox.Width = width;
        this.icon.bounds.X = this.area.X + this.textBox.Width - 38;
    }

    /// <summary>Draws the search overlay to the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        this.textBox.Draw(spriteBatch);
        this.icon.draw(spriteBatch);
    }

    /// <summary>Performs a left click at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse click.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse click.</param>
    /// <returns>Returns true if the search bar was clicked; otherwise, false.</returns>
    public bool LeftClick(int mouseX, int mouseY)
    {
        this.Selected = this.area.Contains(mouseX, mouseY);
        return this.Selected;
    }

    /// <summary>Performs a right click at the specified coordinates on the screen.</summary>
    /// <param name="mouseX">The X-coordinate of the mouse click.</param>
    /// <param name="mouseY">The Y-coordinate of the mouse click.</param>
    /// <returns>Returns true if the search bar was clicked; otherwise, false.</returns>
    public bool RightClick(int mouseX, int mouseY)
    {
        if (this.area.Contains(mouseX, mouseY))
        {
            this.Selected = true;
            this.textBox.Text = string.Empty;
        }

        return this.Selected;
    }

    /// <summary>Updates the search bar based on the mouse position.</summary>
    /// <param name="mouseX">The x-coordinate of the mouse position.</param>
    /// <param name="mouseY">The y-coordinate of the mouse position.</param>
    public void Update(int mouseX, int mouseY)
    {
        this.textBox.Hover(mouseX, mouseY);
        if (this.timeout > 0 && --this.timeout == 0)
        {
            this.setMethod(this.textBox.Text);
        }

        if (this.textBox.Text.Equals(this.previousText, StringComparison.Ordinal))
        {
            return;
        }

        this.timeout = SearchBar.CountdownTimer;
        this.previousText = this.textBox.Text;
    }
}