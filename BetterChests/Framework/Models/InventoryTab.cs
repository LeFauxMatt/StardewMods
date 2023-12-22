namespace StardewMods.BetterChests.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Menus;

/// <summary>Represents an inventory tab.</summary>
internal sealed class InventoryTab
{
    private readonly ItemMatcher itemMatcher;

    /// <summary>Initializes a new instance of the <see cref="InventoryTab" /> class.</summary>
    /// <param name="name"></param>
    /// <param name="hoverText"></param>
    /// <param name="texture"></param>
    /// <param name="index"></param>
    /// <param name="itemMatcher"></param>
    public InventoryTab(string name, string hoverText, Texture2D texture, int index, ItemMatcher itemMatcher)
    {
        this.itemMatcher = itemMatcher;
        this.Component = new ClickableTextureComponent(name, new Rectangle(0, 0, 16 * Game1.pixelZoom, 13 * Game1.pixelZoom), string.Empty, hoverText, texture, new Rectangle(16 * index, 4, 16, 12), Game1.pixelZoom);
    }

    /// <summary>Gets the clickable component.</summary>
    public ClickableTextureComponent Component { get; }

    /// <summary>Determines whether the given tab matches the item.</summary>
    /// <param name="item">The item to be checked.</param>
    /// <returns><c>true</c> if the item matches the item matcher; otherwise, <c>false</c>.</returns>
    public bool MatchesItem(Item item) => this.itemMatcher.MatchesFilter(item);
}
