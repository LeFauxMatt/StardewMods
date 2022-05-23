#nullable disable

namespace StardewMods.BetterChests.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class NumberComponent : IClickableComponent
{
    /// <inheritdoc />
    public ComponentArea Area
    {
        get => ComponentArea.Custom;
    }

    /// <inheritdoc />
    public ClickableTextureComponent Component
    {
        get => null;
    }

    /// <inheritdoc />
    public ComponentType ComponentType
    {
        get => ComponentType.Custom;
    }

    /// <inheritdoc />
    public string HoverText
    {
        get => string.Empty;
    }

    /// <inheritdoc />
    public int Id
    {
        get => -1;
    }

    /// <inheritdoc />
    public bool IsVisible { get; set; }

    /// <inheritdoc />
    public ComponentLayer Layer
    {
        get => ComponentLayer.Above;
    }

    /// <inheritdoc />
    public string Name
    {
        get => string.Empty;
    }

    /// <summary>
    ///     Gets or sets the number value.
    /// </summary>
    public int Value { get; set; }

    /// <inheritdoc />
    public int X { get; set; }

    /// <inheritdoc />
    public int Y { get; set; }

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch)
    {
        IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, new(0, 256, 60, 60), this.X, this.Y, 64, 34 + Game1.tileSize / 3 + Game1.tileSize / 16, Color.White, drawShadow: true);
        Utility.drawTextWithShadow(spriteBatch, this.Value.ToString(), Game1.smallFont, new(this.X + Game1.pixelZoom * 4, this.Y + Game1.pixelZoom * 4), Game1.textColor);
    }

    /// <inheritdoc />
    public void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
    }
}