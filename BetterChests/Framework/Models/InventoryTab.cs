namespace StardewMods.BetterChests.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Menus;

/// <summary>Represents an inventory tab.</summary>
internal sealed class InventoryTab : IItemFilter
{
    private readonly ItemMatcher itemMatcher;

    private bool selected;

    /// <summary>Initializes a new instance of the <see cref="InventoryTab" /> class.</summary>
    /// <param name="name">The name of the tab.</param>
    /// <param name="hoverText">The text to display when hovering over the tab.</param>
    /// <param name="texture">The texture of the tab.</param>
    /// <param name="index">The index of the tab.</param>
    /// <param name="itemMatcher">The item matcher for filtering items in the inventory.</param>
    public InventoryTab(string name, string hoverText, Texture2D texture, int index, ItemMatcher itemMatcher)
    {
        this.itemMatcher = itemMatcher;
        this.Component = new ClickableTextureComponent(
            name,
            new Rectangle(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
            string.Empty,
            hoverText,
            texture,
            new Rectangle(16 * index, 4, 16, 16),
            Game1.pixelZoom);
    }

    /// <summary>Gets the clickable component.</summary>
    public ClickableTextureComponent Component { get; }

    /// <inheritdoc />
    public bool MatchesFilter(Item item) => this.itemMatcher.MatchesFilter(item);

    /// <summary>Draws the component on the screen.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            this.Component.texture,
            new Vector2(this.Component.bounds.X, this.Component.bounds.Y),
            new Rectangle(16, this.Component.sourceRect.Y, 16, 16),
            this.selected ? Color.White : Color.Gray,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            0.86f);

        // Tab icon
        this.Component.draw(
            spriteBatch,
            this.selected ? Color.White : Color.Gray,
            0.86f + (this.Component.bounds.Y / 20000f));
    }

    /// <summary>Selects the current tab.</summary>
    public void Select()
    {
        this.selected = true;
        this.Component.sourceRect.Y = 0;
        this.Component.bounds.Y -= 4;
    }

    /// <summary>Unselects the current tab.</summary>
    public void Deselect()
    {
        this.selected = false;
        this.Component.sourceRect.Y = 0;
        this.Component.bounds.Y += 4;
    }
}