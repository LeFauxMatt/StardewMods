namespace FuryCore.Services;

using System;
using FuryCore.Attributes;
using FuryCore.Events;
using FuryCore.Interfaces;
using FuryCore.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

/// <inheritdoc cref="FuryCore.Interfaces.IFuryEvents" />
[FuryCoreService(true)]
internal class CustomEvents : IFuryEvents, IModService
{
    private readonly ItemGrabMenuChanged _itemGrabMenuChanged;
    private readonly MenuComponentPressed _menuComponentPressed;
    private readonly RenderedItemGrabMenu _renderedItemGrabMenu;
    private readonly RenderingItemGrabMenu _renderingItemGrabMenu;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomEvents" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CustomEvents(IModHelper helper, IModServices services)
    {
        this._itemGrabMenuChanged = new(helper.Events.GameLoop, services);
        this._menuComponentPressed = new(helper, services);
        this._renderedItemGrabMenu = new(helper.Events.Display, services);
        this._renderingItemGrabMenu = new(helper.Events.Display, services);
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
}