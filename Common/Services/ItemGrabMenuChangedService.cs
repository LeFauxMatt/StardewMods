namespace Common.Services
{
    using System;
    using Common.Helpers;
    using Common.Interfaces;
    using Common.Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="BaseService" />
    internal class ItemGrabMenuChangedService : BaseService, IEventHandlerService<EventHandler<ItemGrabMenuEventArgs>>
    {
        private static ItemGrabMenuChangedService Instance;
        private readonly PerScreen<IClickableMenu> _menu = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _screenId = new() { Value = -1 };

        private ItemGrabMenuChangedService()
            : base("ItemGrabMenuChanged")
        {
            Events.Display.MenuChanged += this.OnMenuChanged;
        }

        private event EventHandler<ItemGrabMenuEventArgs> ItemGrabMenuChanged;

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ItemGrabMenuChangedService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuChangedService"/> class.</returns>
        public static ItemGrabMenuChangedService GetSingleton(ServiceManager serviceManager)
        {
            return ItemGrabMenuChangedService.Instance ??= new ItemGrabMenuChangedService();
        }

        /// <inheritdoc/>
        public void AddHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuChanged += eventHandler;
        }

        /// <inheritdoc/>
        public void RemoveHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuChanged -= eventHandler;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this._menu.Value))
            {
                return;
            }

            this._menu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest { playerChest: { Value: true } } chest } itemGrabMenu)
            {
                this._attached.Value = false;
                this._screenId.Value = -1;
                this.InvokeAll(null, null);
                return;
            }

            this._attached.Value = true;
            this._screenId.Value = Context.ScreenId;
            this.InvokeAll(itemGrabMenu, chest);
        }

        private void InvokeAll(ItemGrabMenu itemGrabMenu, Chest chest)
        {
            var eventArgs = new ItemGrabMenuEventArgs(itemGrabMenu, chest);
            this.ItemGrabMenuChanged?.Invoke(this, eventArgs);
        }
    }
}