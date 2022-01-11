namespace FuryCore.Events;

using FuryCore.Models;
using FuryCore.Services;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

/// <inheritdoc />
internal class RenderingItemGrabMenu : SortedEventHandler<RenderingActiveMenuEventArgs>
{
    private readonly PerScreen<ItemGrabMenuChangedEventArgs> _menu = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderingItemGrabMenu"/> class.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="services"></param>
    public RenderingItemGrabMenu(IDisplayEvents display, ServiceCollection services)
    {
        services.Lazy<CustomEvents>(events => events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged);
        display.RenderingActiveMenu += this.OnRenderingActiveMenu;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.ItemGrabMenu is null)
        {
            this._menu.Value = null;
            return;
        }

        if (e.IsNew)
        {
            e.ItemGrabMenu.setBackgroundTransparency(false);
        }

        this._menu.Value = e;
    }

    [EventPriority(EventPriority.High)]
    private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
        {
            return;
        }

        // Draw background
        e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

        // Draw rendered items above background
        this.InvokeAll(e);
    }
}