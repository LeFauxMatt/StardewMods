namespace StardewMods.Common.Integrations.BetterChests;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Represents an object that can be colored.
/// </summary>
public interface IColorable
{
    /// <summary>
    ///     Gets or sets the current color of the object.
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    ///     Draws the colorable object to the screen.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch" /> to draw to.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public void Draw(SpriteBatch spriteBatch, int x, int y);
}