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
internal class CustomEvents : IFuryEvents, IService
{
    private readonly ItemGrabMenuChanged _itemGrabMenuChanged;
    private readonly MenuComponentPressed _menuComponentPressed;
    private readonly RenderedItemGrabMenu _renderedItemGrabMenu;
    private readonly RenderingItemGrabMenu _renderingItemGrabMenu;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomEvents"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CustomEvents(IModHelper helper, ServiceCollection services)
    {
        this._itemGrabMenuChanged = new(helper.Events.Display, services);
        this._menuComponentPressed = new(helper, services);
        this._renderedItemGrabMenu = new(helper.Events.Display, services);
        this._renderingItemGrabMenu = new(helper.Events.Display, services);
    }

    /// <inheritdoc/>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged
    {
        add => this._itemGrabMenuChanged.Add(value);
        remove => this._itemGrabMenuChanged.Remove(value);
    }

    /// <inheritdoc/>
    public event EventHandler<MenuComponentPressedEventArgs> MenuComponentPressed
    {
        add => this._menuComponentPressed.Add(value);
        remove => this._menuComponentPressed.Remove(value);
    }

    /// <inheritdoc/>
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedItemGrabMenu
    {
        add => this._renderedItemGrabMenu.Add(value);
        remove => this._renderedItemGrabMenu.Remove(value);
    }

    /// <inheritdoc/>
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingItemGrabMenu
    {
        add => this._renderingItemGrabMenu.Add(value);
        remove => this._renderingItemGrabMenu.Remove(value);
    }
}