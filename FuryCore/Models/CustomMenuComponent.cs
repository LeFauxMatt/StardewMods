namespace FuryCore.Models;

using FuryCore.Enums;
using FuryCore.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <inheritdoc />
public class CustomMenuComponent : IMenuComponent
{
    private int? _myId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomMenuComponent" /> class.
    /// </summary>
    /// <param name="component">The clickable texture component.</param>
    /// <param name="area">The area of the screen to orient the component to.</param>
    public CustomMenuComponent(ClickableTextureComponent component, ComponentArea area = ComponentArea.Custom)
        : this(area)
    {
        this.Component = component;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomMenuComponent"/> class.
    /// </summary>
    /// <param name="area">The area of the screen to orient the component to.</param>
    protected CustomMenuComponent(ComponentArea area)
    {
        this.Area = area;
        this.ComponentType = ComponentType.Custom;
    }

    /// <inheritdoc/>
    public ComponentArea Area { get; protected init; }

    /// <inheritdoc/>
    public ComponentType ComponentType { get; protected init; }

    /// <inheritdoc/>
    public ClickableTextureComponent Component { get; protected init; }

    /// <inheritdoc/>
    public int Id
    {
        get => this._myId ??= this.Component.myID = CustomMenuComponent.ComponentId++;
    }

    /// <inheritdoc/>
    public string Name
    {
        get => this.Component.name;
    }

    private static int ComponentId { get; set; } = 69420;

    /// <inheritdoc/>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        this.Component?.draw(spriteBatch);
    }

    /// <inheritdoc/>
    public virtual void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        this.Component?.tryHover(x, y, maxScaleIncrease);
    }
}