namespace FuryCore.Models;

using System;
using FuryCore.Enums;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

/// <summary>
/// </summary>
public class MenuComponent
{
    private readonly ClickableTextureComponent _component;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponent" /> class.
    /// </summary>
    /// <param name="component"></param>
    public MenuComponent(ClickableTextureComponent component, ComponentArea area = ComponentArea.Custom)
    {
        this.Area = area;
        this._component = component;
    }

    public MenuComponent(ItemGrabMenu menu, ComponentType componentType)
    {
        this.Menu = menu;
        this.ComponentType = componentType;
        this.Area = this.ComponentType switch
        {
            ComponentType.OrganizeButton => ComponentArea.Right,
            ComponentType.FillStacksButton => ComponentArea.Right,
            ComponentType.ColorPickerToggleButton => ComponentArea.Right,
            ComponentType.SpecialButton => ComponentArea.Right,
            ComponentType.JunimoNoteIcon => ComponentArea.Right,
            _ => ComponentArea.Custom,
        };
    }

    public ComponentArea Area { get; }

    public ComponentType ComponentType { get; }

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

    private static int ComponentId { get; set; } = 69420;

    private ItemGrabMenu Menu { get; }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        this.Component.draw(spriteBatch);
    }

    public virtual void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        this.Component.tryHover(x, y, maxScaleIncrease);
    }
}