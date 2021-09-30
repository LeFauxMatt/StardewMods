namespace Common.Services
{
    using System;
    using Common.Interfaces;
    using Helpers;
    using Microsoft.Xna.Framework;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;

    /// <inheritdoc cref="BaseService" />
    internal class RenderingActiveMenuService : BaseService, IEventHandlerService<EventHandler<RenderingActiveMenuEventArgs>>
    {
        private static RenderingActiveMenuService Instance;
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<bool> _attached = new();

        private RenderingActiveMenuService(ItemGrabMenuChangedService itemGrabMenuChangedService)
            : base("RenderingActiveMenu")
        {
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
        }

        private event EventHandler<RenderingActiveMenuEventArgs> RenderingActiveMenu;

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

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="RenderingActiveMenuService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="RenderingActiveMenuService"/> class.</returns>
        public static RenderingActiveMenuService GetSingleton(ServiceManager serviceManager)
        {
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            return RenderingActiveMenuService.Instance ??= new RenderingActiveMenuService(itemGrabMenuChangedService);
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is not null && !this._attached.Value)
            {
                Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
                this._screenId.Value = Context.ScreenId;
                this._attached.Value = true;
                return;
            }

            if (e.ItemGrabMenu is null)
            {
                Events.Display.RenderingActiveMenu -= this.OnRenderingActiveMenu;
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