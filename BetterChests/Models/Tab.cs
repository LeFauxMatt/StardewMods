namespace BetterChests.Models;

using System.Collections.Generic;
using FuryCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class Tab : MenuComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tab"/> class.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="tags"></param>
    public Tab(ClickableTextureComponent component, IList<string> tags)
        : base(component)
    {
        this.Tags = tags;
    }

    public IList<string> Tags { get; }

    public bool Selected { get; set; }

    public int BaseY { get; set; } = 0;

    /// <inheritdoc/>
    public override void Draw(SpriteBatch spriteBatch)
    {
        var color = this.Selected ? Color.White : Color.Gray;
        this.Component.bounds.Y = this.BaseY + (this.Selected ? Game1.pixelZoom : 0);
        this.Component.draw(spriteBatch, color, 0.86f + (this.Component.bounds.Y / 20000f));
    }

    /// <inheritdoc/>
    public override void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        // Do Nothing
    }
}