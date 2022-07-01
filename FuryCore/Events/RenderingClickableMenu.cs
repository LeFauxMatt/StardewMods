#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class RenderingClickableMenu : SortedEventHandler<RenderingActiveMenuEventArgs>
{
    private readonly Lazy<GameObjects> _gameObjects;
    private readonly PerScreen<IClickableMenu> _menu = new();
    private readonly PerScreen<int> _screenId = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingClickableMenu" /> class.
    /// </summary>
    /// <param name="display">SMAPI events related to UI and drawing to the screen.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public RenderingClickableMenu(IDisplayEvents display, IModServices services)
    {
        this._gameObjects = services.Lazy<GameObjects>();
        services.Lazy<CustomEvents>(events => events.ClickableMenuChanged += this.OnClickableMenuChanged);
        display.RenderingActiveMenu += this.OnRenderingActiveMenu;
    }

    private GameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private int ScreenId
    {
        get => this._screenId.Value;
        set => this._screenId.Value = value;
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        switch (e.Menu)
        {
            case ItemGrabMenu { context: { } context } itemGrabMenu when this.GameObjects.TryGetGameObject(context, out var gameObject) && gameObject is IStorageContainer:
                this.Menu = e.Menu;
                this.ScreenId = e.ScreenId;
                itemGrabMenu.setBackgroundTransparency(false);
                break;
            default:
                this.Menu = null;
                this.ScreenId = -1;
                break;
        }
    }

    [EventPriority(EventPriority.High + 1000)]
    private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (this.Menu is null || this.ScreenId != Context.ScreenId)
        {
            return;
        }

        // Draw background
        e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

        // Draw rendered items above background
        this.InvokeAll(e);
    }
}