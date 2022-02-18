namespace StardewMods.FuryCore.Events;

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class RenderingClickableMenu : SortedEventHandler<RenderingActiveMenuEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();
    private readonly PerScreen<int> _screenId = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderingClickableMenu" /> class.
    /// </summary>
    /// <param name="display">SMAPI events related to UI and drawing to the screen.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public RenderingClickableMenu(IDisplayEvents display, IModServices services)
    {
        services.Lazy<CustomEvents>(events => events.ClickableMenuChanged += this.OnClickableMenuChanged);
        display.RenderingActiveMenu += this.OnRenderingActiveMenu;
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

    private void OnClickableMenuChanged(object sender, ClickableMenuChangedEventArgs e)
    {
        this.Menu = e.Menu;
        this.ScreenId = e.ScreenId;

        switch (this.Menu)
        {
            case ItemGrabMenu itemGrabMenu:
                itemGrabMenu.setBackgroundTransparency(false);
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