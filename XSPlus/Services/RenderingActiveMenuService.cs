namespace XSPlus.Services
{
    using System;
    using Interfaces;
    using Microsoft.Xna.Framework;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;

    /// <inheritdoc/>
    internal class RenderingActiveMenuService : IEventHandlerService<EventHandler<RenderingActiveMenuEventArgs>>
    {
        private readonly IDisplayEvents _displayEvents;
        private readonly PerScreen<int> _screenId = new();
        private readonly PerScreen<bool> _attached = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingActiveMenuService"/> class.
        /// </summary>
        /// <param name="displayEvents">Events related to UI and drawing to the screen.</param>
        /// <param name="itemGrabMenuChangedService">Event for when the active menu switches from/to an ItemGrabMenu with a chest.</param>
        public RenderingActiveMenuService(IDisplayEvents displayEvents, ItemGrabMenuChangedService itemGrabMenuChangedService)
        {
            this._displayEvents = displayEvents;
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
        }

        private event EventHandler<RenderingActiveMenuEventArgs>? RenderingActiveMenu;

        /// <inheritdoc/>
        public void AddHandler(EventHandler<RenderingActiveMenuEventArgs> handler)
        {
            this.RenderingActiveMenu += handler;
        }

        /// <inheritdoc/>
        public void RemoveHandler(EventHandler<RenderingActiveMenuEventArgs> handler)
        {
            this.RenderingActiveMenu -= handler;
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is not null && !this._attached.Value)
            {
                this._displayEvents.RenderingActiveMenu += this.OnRenderingActiveMenu;
                this._screenId.Value = Context.ScreenId;
                this._attached.Value = true;
                return;
            }

            if (e.ItemGrabMenu is null)
            {
                this._displayEvents.RenderingActiveMenu -= this.OnRenderingActiveMenu;
                this._screenId.Value = -1;
                this._attached.Value = false;
            }
        }

        [EventPriority(EventPriority.High)]
        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            // Draw background
            e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

            // Draw rendered items above background
            this.RenderingActiveMenu?.Invoke(this, e);
        }
    }
}