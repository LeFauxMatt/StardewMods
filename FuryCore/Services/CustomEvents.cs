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
    private readonly ClickableMenuChanged _clickableMenuChanged;
    private readonly GameObjectsRemoved _gameObjectsRemoved;
    private readonly HudComponentPressed _hudComponentPressed;
    private readonly MenuComponentPressed _menuComponentPressed;
    private readonly MenuComponentsLoading _menuComponentsLoading;
    private readonly MenuItemsChanged _menuItemsChanged;
    private readonly RenderedClickableMenu _renderedClickableMenu;
    private readonly RenderingClickableMenu _renderingClickableMenu;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomEvents" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CustomEvents(IModHelper helper, IModServices services)
    {
        this._clickableMenuChanged = new(helper.Events.GameLoop, services);
        this._gameObjectsRemoved = new(helper.Events, services);
        this._menuComponentsLoading = new(services);
        this._menuComponentPressed = new(helper, services);
        this._menuItemsChanged = new(services);
        this._renderedClickableMenu = new(helper.Events.Display, services);
        this._renderingClickableMenu = new(helper.Events.Display, services);
        this._hudComponentPressed = new(helper, services);
    }

    /// <inheritdoc />
    public event EventHandler<IClickableMenuChangedEventArgs> ClickableMenuChanged
    {
        add => this._clickableMenuChanged.Add(value);
        remove => this._clickableMenuChanged.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<GameObjectsRemovedEventArgs> GameObjectsRemoved
    {
        add => this._gameObjectsRemoved.Add(value);
        remove => this._gameObjectsRemoved.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ClickableComponentPressedEventArgs> HudComponentPressed
    {
        add => this._hudComponentPressed.Add(value);
        remove => this._hudComponentPressed.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ClickableComponentPressedEventArgs> MenuComponentPressed
    {
        add => this._menuComponentPressed.Add(value);
        remove => this._menuComponentPressed.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<MenuComponentsLoadingEventArgs> MenuComponentsLoading
    {
        add => this._menuComponentsLoading.Add(value);
        remove => this._menuComponentsLoading.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<IMenuItemsChangedEventArgs> MenuItemsChanged
    {
        add => this._menuItemsChanged.Add(value);
        remove => this._menuItemsChanged.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedClickableMenu
    {
        add => this._renderedClickableMenu.Add(value);
        remove => this._renderedClickableMenu.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingClickableMenu
    {
        add => this._renderingClickableMenu.Add(value);
        remove => this._renderingClickableMenu.Remove(value);
    }
}