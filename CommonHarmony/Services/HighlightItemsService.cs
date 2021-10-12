namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Services;
    using Interfaces;
    using Models;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="BaseService" />
    internal class HighlightItemsService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private readonly PerScreen<IList<Func<Item, bool>>> _highlightItemHandlers = new(() => new List<Func<Item, bool>>());
        private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();

        private HighlightItemsService(ServiceManager serviceManager)
            : base("HighlightItems")
        {
            // Dependencies
            this.AddDependency<ItemGrabMenuChangedService>(service => (service as ItemGrabMenuChangedService)?.AddHandler(this.OnItemGrabMenuChanged));
        }

        /// <inheritdoc />
        public void AddHandler(Func<Item, bool> handler)
        {
            this._highlightItemHandlers.Value.Add(handler);
        }

        /// <inheritdoc />
        public void RemoveHandler(Func<Item, bool> handler)
        {
            this._highlightItemHandlers.Value.Remove(handler);
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu?.context is Chest {playerChest: {Value: true}} && e.ItemGrabMenu.inventory.highlightMethod?.Target is not HighlightItemsService)
            {
                this._highlightMethod.Value = e.ItemGrabMenu.inventory.highlightMethod;
                e.ItemGrabMenu.inventory.highlightMethod = this.HighlightMethod;
            }
        }

        private bool HighlightMethod(Item item)
        {
            return this._highlightMethod.Value(item) && (this._highlightItemHandlers.Value.Count == 0 || this._highlightItemHandlers.Value.All(highlightMethod => highlightMethod(item)));
        }
    }
}