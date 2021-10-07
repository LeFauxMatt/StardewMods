namespace CommonHarmony.Services
{
    using System;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Interfaces;
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
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();

        private RenderingActiveMenuService(ItemGrabMenuChangedService itemGrabMenuChangedService)
            : base("RenderingActiveMenu")
        {
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        }

        /// <inheritdoc />
        public void AddHandler(EventHandler<RenderingActiveMenuEventArgs> handler)
        {
            this.RenderingActiveMenu += handler;
        }

        /// <inheritdoc />
        public void RemoveHandler(EventHandler<RenderingActiveMenuEventArgs> handler)
        {
            this.RenderingActiveMenu -= handler;
        }

        private event EventHandler<RenderingActiveMenuEventArgs> RenderingActiveMenu;

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="RenderingActiveMenuService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="RenderingActiveMenuService" /> class.</returns>
        public static async Task<RenderingActiveMenuService> Create(ServiceManager serviceManager)
        {
            return RenderingActiveMenuService.Instance ??= new(await serviceManager.Get<ItemGrabMenuChangedService>());
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null)
            {
                this._menu.Value = null;
                return;
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
            this.RenderingActiveMenu?.Invoke(this, e);
        }
    }
}