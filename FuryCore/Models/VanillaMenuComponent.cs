namespace StardewMods.FuryCore.Models;

using System;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal class VanillaMenuComponent : IMenuComponent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="VanillaMenuComponent" /> class.
    /// </summary>
    /// <param name="menu">The ItemGrabMenu.</param>
    /// <param name="componentType">A component on the ItemGrabMenu.</param>
    public VanillaMenuComponent(ItemGrabMenu menu, ComponentType componentType)
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

    /// <inheritdoc />
    public ComponentArea Area { get; }

    /// <inheritdoc />
    public ClickableTextureComponent Component
    {
        get => this.ComponentType switch
        {
            ComponentType.OrganizeButton => this.Menu?.organizeButton,
            ComponentType.FillStacksButton => this.Menu?.fillStacksButton,
            ComponentType.ColorPickerToggleButton => this.Menu?.colorPickerToggleButton,
            ComponentType.SpecialButton => this.Menu?.specialButton,
            ComponentType.JunimoNoteIcon => this.Menu?.junimoNoteIcon,
            ComponentType.Custom or _ => throw new ArgumentOutOfRangeException($"Invalid ComponentType {this.ComponentType}."),
        };
    }

    /// <inheritdoc />
    public ComponentType ComponentType { get; }

    /// <inheritdoc />
    public int Id
    {
        get => this.Component.myID;
    }

    /// <inheritdoc />
    public string Name
    {
        get => this.Component.name;
    }

    private ItemGrabMenu Menu { get; }

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