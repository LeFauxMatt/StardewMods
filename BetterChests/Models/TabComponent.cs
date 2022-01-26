namespace BetterChests.Models;

using System.Collections.Generic;
using FuryCore.Enums;
using FuryCore.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class TabComponent : MenuComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TabComponent"/> class.
    /// </summary>
    /// <param name="component">The texture that is drawn for the tab.</param>
    /// <param name="tags">The context tags that determine what items are shown for this tab.</param>
    public TabComponent(ClickableTextureComponent component, IList<string> tags)
        : base(component, ComponentArea.Bottom)
    {
        this.Tags = tags;
    }

    /// <summary>
    /// Gets this tab's list of context tags for filtering displayed items.
    /// </summary>
    public IList<string> Tags { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this tab is currently selected.
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    /// Gets or sets the base alignment for each tab. The currently selected tab is slightly offset.
    /// </summary>
    public int BaseY { get; set; }

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