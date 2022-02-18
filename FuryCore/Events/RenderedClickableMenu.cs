namespace StardewMods.FuryCore.Events;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class RenderedClickableMenu : SortedEventHandler<RenderedActiveMenuEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();
    private readonly PerScreen<int> _screenId = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderedClickableMenu" /> class.
    /// </summary>
    /// <param name="display">SMAPI events related to UI and drawing to the screen.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public RenderedClickableMenu(IDisplayEvents display, IModServices services)
    {
        services.Lazy<CustomEvents>(events => events.ClickableMenuChanged += this.OnClickableMenuChanged);
        display.RenderedActiveMenu += this.OnRenderedActiveMenu;
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
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.HandlerCount == 0 || this.Menu is null || this.ScreenId != Context.ScreenId)
        {
            return;
        }

        // Draw render items below foreground
        this.InvokeAll(e);

        // Draw hover elements
        switch (this.Menu)
        {
            case ItemGrabMenu itemGrabMenu:
                if (itemGrabMenu.hoveredItem is not null)
                {
                    IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoveredItem.getDescription(), itemGrabMenu.hoveredItem.DisplayName, itemGrabMenu.hoveredItem, itemGrabMenu.heldItem != null);
                }
                else if (itemGrabMenu.hoverText != null)
                {
                    if (itemGrabMenu.hoverAmount > 0)
                    {
                        IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoverText, string.Empty, null, true, -1, 0, -1, -1, null, itemGrabMenu.hoverAmount);
                    }
                    else
                    {
                        IClickableMenu.drawHoverText(e.SpriteBatch, itemGrabMenu.hoverText, Game1.smallFont);
                    }
                }

                itemGrabMenu.heldItem?.drawInMenu(e.SpriteBatch, new(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

                break;
        }

        // Draw cursor
        this.Menu.drawMouse(e.SpriteBatch);
    }
}