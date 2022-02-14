namespace StardewMods.FuryCore.Services;

using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.CustomEvents;

/// <inheritdoc cref="ICustomEvents" />
[FuryCoreService(true)]
internal class CustomEvents : ICustomEvents, IModService
{
    private readonly GameObjectsRemoved _gameObjectsRemoved;
    private readonly ItemGrabMenuChanged _itemGrabMenuChanged;
    private readonly MenuComponentPressed _menuComponentPressed;
    private readonly RenderedItemGrabMenu _renderedItemGrabMenu;
    private readonly RenderingItemGrabMenu _renderingItemGrabMenu;
    private readonly ToolbarIconPressed _toolbarIconPressed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomEvents" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CustomEvents(IModHelper helper, IModServices services)
    {
        this._gameObjectsRemoved = new(helper.Events, services);
        this._itemGrabMenuChanged = new(helper.Events.GameLoop, services);
        this._menuComponentPressed = new(helper, services);
        this._renderedItemGrabMenu = new(helper.Events.Display, services);
        this._renderingItemGrabMenu = new(helper.Events.Display, services);
        this._toolbarIconPressed = new(helper, services);
    }

    /// <inheritdoc />
    public event EventHandler<GameObjectsRemovedEventArgs> GameObjectsRemoved
    {
        add => this._gameObjectsRemoved.Add(value);
        remove => this._gameObjectsRemoved.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged
    {
        add => this._itemGrabMenuChanged.Add(value);
        remove => this._itemGrabMenuChanged.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<MenuComponentPressedEventArgs> MenuComponentPressed
    {
        add => this._menuComponentPressed.Add(value);
        remove => this._menuComponentPressed.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedItemGrabMenu
    {
        add => this._renderedItemGrabMenu.Add(value);
        remove => this._renderedItemGrabMenu.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingItemGrabMenu
    {
        add => this._renderingItemGrabMenu.Add(value);
        remove => this._renderingItemGrabMenu.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ToolbarIconPressedEventArgs> ToolbarIconPressed
    {
        add => this._toolbarIconPressed.Add(value);
        remove => this._toolbarIconPressed.Remove(value);
    }
}