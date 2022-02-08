namespace StardewMods.FuryCore.Events;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class RenderedItemGrabMenu : SortedEventHandler<RenderedActiveMenuEventArgs>
{
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int> _screenId = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RenderedItemGrabMenu" /> class.
    /// </summary>
    /// <param name="display">SMAPI events related to UI and drawing to the screen.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public RenderedItemGrabMenu(IDisplayEvents display, IModServices services)
    {
        services.Lazy<CustomEvents>(events => events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged);
        display.RenderedActiveMenu += this.OnRenderedActiveMenu;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private int ScreenId
    {
        get => this._screenId.Value;
        set => this._screenId.Value = value;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
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

        // Draw foreground
        if (this.Menu.hoverText is not null && (this.Menu.hoveredItem is null or null || this.Menu.ItemsToGrabMenu is null))
        {
            if (this.Menu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, this.Menu.hoverText, string.Empty, null, true, -1, 0, -1, -1, null, this.Menu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this.Menu.hoverText, Game1.smallFont);
            }
        }

        if (this.Menu.hoveredItem is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this.Menu.hoveredItem.getDescription(), this.Menu.hoveredItem.DisplayName, this.Menu.hoveredItem, this.Menu.heldItem is not null);
        }
        else if (this.Menu.hoveredItem is not null && this.Menu.ItemsToGrabMenu is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this.Menu.ItemsToGrabMenu.descriptionText, this.Menu.ItemsToGrabMenu.descriptionTitle, this.Menu.hoveredItem, this.Menu.heldItem is not null);
        }

        this.Menu.heldItem?.drawInMenu(e.SpriteBatch, new(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

        this.Menu.drawMouse(e.SpriteBatch);
    }
}