namespace StardewMods.EnhancedJunimoChests.Models;

using Microsoft.Xna.Framework;

/// <summary>The data model for the color and items.</summary>
internal sealed class ColorData(string name, string item, Color color)
{
    /// <summary>Gets or sets the name of the color.</summary>
    public string Name { get; set; } = name;

    /// <summary>Gets or sets the item required to change to the color.</summary>
    public string Item { get; set; } = item;

    /// <summary>Gets or sets the color.</summary>
    public Color Color { get; set; } = color;
}