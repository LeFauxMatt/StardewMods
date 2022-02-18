namespace StardewMods.FuryCore.Models.ClickableComponents;

using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewValley.Menus;

/// <inheritdoc />
internal class VanillaClickableComponent : IClickableComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="VanillaClickableComponent" /> class.
    /// </summary>
    /// <param name="menu">The ItemGrabMenu.</param>
    /// <param name="componentType">A component on the ItemGrabMenu.</param>
    public VanillaClickableComponent(IClickableMenu menu, ComponentType componentType)
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
        this.Layer = ComponentLayer.Above;
    }

    /// <inheritdoc />
    public ComponentArea Area { get; }

    /// <inheritdoc />
    public ClickableTextureComponent Component
    {
        get => this.Menu switch
        {
            ItemGrabMenu itemGrabMenu => this.ComponentType switch
            {
                ComponentType.OrganizeButton => itemGrabMenu.organizeButton,
                ComponentType.FillStacksButton => itemGrabMenu.fillStacksButton,
                ComponentType.ColorPickerToggleButton => itemGrabMenu.colorPickerToggleButton,
                ComponentType.SpecialButton => itemGrabMenu.specialButton,
                ComponentType.JunimoNoteIcon => itemGrabMenu.junimoNoteIcon,
                ComponentType.Custom or _ => null,
            },
            _ => null,
        };
    }

    /// <inheritdoc />
    public ComponentType ComponentType { get; }

    /// <inheritdoc />
    public string HoverText
    {
        get => this.Component.hoverText;
    }

    /// <inheritdoc />
    public int Id
    {
        get => this.Component.myID;
    }

    /// <inheritdoc />
    public ComponentLayer Layer { get; }

    /// <inheritdoc />
    public string Name
    {
        get => this.Component.name;
    }

    /// <inheritdoc />
    public int X
    {
        get => this.Component.bounds.X;
        set => this.Component.bounds.X = value;
    }

    /// <inheritdoc />
    public int Y
    {
        get => this.Component.bounds.Y;
        set => this.Component.bounds.Y = value;
    }

    private IClickableMenu Menu { get; }

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch)
    {
        this.Component?.draw(spriteBatch);
    }

    /// <inheritdoc />
    public void TryHover(int x, int y, float maxScaleIncrease = 0.1f)
    {
        this.Component?.tryHover(x, y, maxScaleIncrease);
    }
}