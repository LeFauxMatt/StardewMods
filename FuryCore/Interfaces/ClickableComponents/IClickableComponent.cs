#nullable disable

namespace StardewMods.FuryCore.Interfaces.ClickableComponents;

using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewValley.Menus;

/// <summary>
///     Represents a <see cref="ClickableTextureComponent" /> that is drawn to the active menu.
/// </summary>
public interface IClickableComponent
{
    /// <summary>
    ///     Gets a value indicate which area of the screen will the component be oriented to.
    /// </summary>
    public ComponentArea Area { get; }

    /// <summary>
    ///     Gets the <see cref="ClickableTextureComponent" />.
    /// </summary>
    public ClickableTextureComponent Component { get; }

    /// <summary>
    ///     Gets the type of component.
    /// </summary>
    public ComponentType ComponentType { get; }

    /// <summary>
    ///     Gets the text to display while hovering over this component.
    /// </summary>
    public string HoverText { get; }

    /// <summary>
    ///     Gets the Id of the component used for game controllers.
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether or not the component is visible.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    ///     Gets a value indicating if the component should be drawn below or above the menu.
    /// </summary>
    public ComponentLayer Layer { get; }

    /// <summary>
    ///     Gets the name of the component.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets or sets the x-coordinate of the component.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     Gets or sets the y-coordinate of the component.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    ///     Draw the component to the screen.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    public void Draw(SpriteBatch spriteBatch);

    /// <summary>
    ///     Performs an action when the component is hovered over.
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse.</param>
    /// <param name="y">The y-coordinate of the mouse.</param>
    /// <param name="maxScaleIncrease">The maximum increase to scale the component when hovered.</param>
    public void TryHover(int x, int y, float maxScaleIncrease = 0.1f);
}