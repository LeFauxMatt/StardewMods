namespace FuryCore.Events;

using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class RenderedItemGrabMenu : SortedEventHandler<RenderedActiveMenuEventArgs>
{
    private readonly PerScreen<ItemGrabMenuChangedEventArgs> _menu = new();

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

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.ItemGrabMenu is null)
        {
            this._menu.Value = null;
            return;
        }

        this._menu.Value = e;
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this.HandlerCount == 0)
        {
            return;
        }

        // Draw render items below foreground
        this.InvokeAll(e);

        // Draw foreground
        if (this._menu.Value.ItemGrabMenu.hoverText is not null && (this._menu.Value.ItemGrabMenu.hoveredItem is null or null || this._menu.Value.ItemGrabMenu.ItemsToGrabMenu is null))
        {
            if (this._menu.Value.ItemGrabMenu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, this._menu.Value.ItemGrabMenu.hoverText, string.Empty, null, true, -1, 0, -1, -1, null, this._menu.Value.ItemGrabMenu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this._menu.Value.ItemGrabMenu.hoverText, Game1.smallFont);
            }
        }

        if (this._menu.Value.ItemGrabMenu.hoveredItem is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this._menu.Value.ItemGrabMenu.hoveredItem.getDescription(), this._menu.Value.ItemGrabMenu.hoveredItem.DisplayName, this._menu.Value.ItemGrabMenu.hoveredItem, this._menu.Value.ItemGrabMenu.heldItem is not null);
        }
        else if (this._menu.Value.ItemGrabMenu.hoveredItem is not null && this._menu.Value.ItemGrabMenu.ItemsToGrabMenu is not null)
        {
            IClickableMenu.drawToolTip(e.SpriteBatch, this._menu.Value.ItemGrabMenu.ItemsToGrabMenu.descriptionText, this._menu.Value.ItemGrabMenu.ItemsToGrabMenu.descriptionTitle, this._menu.Value.ItemGrabMenu.hoveredItem, this._menu.Value.ItemGrabMenu.heldItem is not null);
        }

        this._menu.Value.ItemGrabMenu.heldItem?.drawInMenu(e.SpriteBatch, new(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

        this._menu.Value.ItemGrabMenu.drawMouse(e.SpriteBatch);
    }
}