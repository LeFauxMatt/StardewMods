#nullable disable

namespace StardewMods.FuryCore.Services;

using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;

/// <inheritdoc cref="ICustomEvents" />
[FuryCoreService(true)]
internal class CustomEvents : ICustomEvents, IModService
{
    private readonly ClickableMenuChanged _clickableMenuChanged;
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
        this._renderedClickableMenu = new(helper.Events.Display, services);
        this._renderingClickableMenu = new(helper.Events.Display, services);
    }

    /// <inheritdoc />
    public event EventHandler<IClickableMenuChangedEventArgs> ClickableMenuChanged
    {
        add => this._clickableMenuChanged.Add(value);
        remove => this._clickableMenuChanged.Remove(value);
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