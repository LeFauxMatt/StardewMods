namespace FuryCore.Interfaces;

using FuryCore.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>
/// Represents a <see cref="ClickableTextureComponent" /> that is drawn to the active menu.
/// </summary>
public interface IMenuComponent
{
    /// <summary>
    /// gets the area of the screen that the component is oriented to.
    /// </summary>
    public ComponentArea Area { get; }

    /// <summary>
    /// Gets the type of component.
    /// </summary>
    public ComponentType ComponentType { get; }

    /// <summary>
    /// Gets the <see cref="ClickableTextureComponent" />.
    /// </summary>
    public ClickableTextureComponent Component { get; }

    /// <summary>
    /// Gets the text to display while hovering over this component.
    /// </summary>
    public virtual string HoverText
    {
        get => this.Component?.hoverText;
    }

    /// <summary>
    /// Gets the Id of the component used for game controllers.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of the component.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the x-coordinate of the component.
    /// </summary>
    public virtual int X
    {
        get => this.Component.bounds.X;
        set => this.Component.bounds.X = value;
    }

    /// <summary>
    /// Gets or sets the y-coordinate of the component.
    /// </summary>
    public virtual int Y
    {
        get => this.Component.bounds.Y;
        set => this.Component.bounds.Y = value;
    }

    /// <summary>
    /// Draw the component to the screen.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to draw the component to.</param>
    public void Draw(SpriteBatch spriteBatch);

    /// <summary>
    /// Performs an action when the component is hovered over.
    /// </summary>
    /// <param name="x">The x-coordinate of the mouse.</param>
    /// <param name="y">The y-coordinate of the mouse.</param>
    /// <param name="maxScaleIncrease">The maximum increase to scale the component when hovered.</param>
    public void TryHover(int x, int y, float maxScaleIncrease = 0.1f);
}