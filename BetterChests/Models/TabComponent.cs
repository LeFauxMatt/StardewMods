﻿namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces.MenuComponents;
using StardewMods.FuryCore.Models.MenuComponents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuComponent" />
internal class TabComponent : CustomMenuComponent, IMenuComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TabComponent" /> class.
    /// </summary>
    /// <param name="component">The texture that is drawn for the tab.</param>
    /// <param name="tags">The context tags that determine what items are shown for this tab.</param>
    public TabComponent(ClickableTextureComponent component, IList<string> tags)
        : base(component, ComponentArea.Bottom)
    {
        this.Tags = tags;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this tab is currently selected.
    /// </summary>
    public bool Selected { get; set; }

    /// <summary>
    ///     Gets this tab's list of context tags for filtering displayed items.
    /// </summary>
    public IList<string> Tags { get; }

    /// <inheritdoc />
    public int Y { get; set; }

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch)
    {
        var color = this.Selected ? Color.White : Color.Gray;
        this.Component.bounds.Y = this.Y + (this.Selected ? Game1.pixelZoom : 0);
        this.Component.draw(spriteBatch, color, 0.86f + this.Component.bounds.Y / 20000f);
    }

    /// <inheritdoc />
    public override void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        // Do Nothing
    }
}