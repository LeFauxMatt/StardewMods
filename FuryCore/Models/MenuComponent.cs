namespace FuryCore.Models;

using System;
using FuryCore.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>
/// 
/// </summary>
public class MenuComponent
{
    private static int ComponentId = 69420;
    private readonly ClickableTextureComponent _component;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuComponent"/> class.
    /// </summary>
    /// <param name="component"></param>
    public MenuComponent(ClickableTextureComponent component)
    {
        this._component = component;
    }

    public MenuComponent(ItemGrabMenu menu, ComponentType componentType)
    {
        this.Menu = menu;
        this.ComponentType = componentType;
    }

    public ClickableTextureComponent Component
    {
        get
        {
            return this._component ?? this.ComponentType switch
            {
                ComponentType.OrganizeButton => this.Menu?.organizeButton,
                ComponentType.FillStacksButton => this.Menu?.fillStacksButton,
                ComponentType.ColorPickerToggleButton => this.Menu?.colorPickerToggleButton,
                ComponentType.SpecialButton => this.Menu?.specialButton,
                ComponentType.JunimoNoteIcon => this.Menu?.junimoNoteIcon,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public int Id
    {
        get
        {
            if (this.Component.myID == -500)
            {
                this.Component.myID = MenuComponent.ComponentId++;
            }

            return this.Component.myID;
        }
    }

    private ComponentType ComponentType { get; }

    private ItemGrabMenu Menu { get; }

    public virtual string HoverText
    {
        get => this.Component.hoverText;
        set => this.Component.hoverText = value;
    }

    public bool IsCustom
    {
        get => this._component is not null;
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