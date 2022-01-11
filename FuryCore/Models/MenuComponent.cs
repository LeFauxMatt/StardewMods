namespace FuryCore.Models;

using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

/// <summary>
/// 
/// </summary>
public class MenuComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MenuComponent"/> class.
    /// </summary>
    /// <param name="component"></param>
    public MenuComponent(ClickableTextureComponent component)
    {
        this.Component = component;
    }

    public ClickableTextureComponent Component { get; }

    public virtual string HoverText
    {
        get => this.Component.hoverText;
        set => this.Component.hoverText = value;
    }

    public virtual string Name
    {
        get => this.Component.name;
        set => this.Component.name = value;
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        this.Component.draw(spriteBatch);
    }

    public virtual void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        this.Component.tryHover(x, y, maxScaleIncrease);
    }
}