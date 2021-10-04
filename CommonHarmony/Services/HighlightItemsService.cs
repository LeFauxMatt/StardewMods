namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Common.Models;
    using Common.Services;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <inheritdoc cref="BaseService" />
    internal class HighlightItemsService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private static HighlightItemsService Instance;
        private readonly PerScreen<IList<Func<Item, bool>>> _highlightItemHandlers = new()
        {
            Value = new List<Func<Item, bool>>(),
        };
        private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();

        private HighlightItemsService(ItemGrabMenuConstructedService itemGrabMenuConstructedService)
            : base("HighlightItems")
        {
            // Events
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructed);
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="HighlightItemsService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns a new instance of the <see cref="HighlightItemsService" /> class.</returns>
        public static async Task<HighlightItemsService> Create(ServiceManager serviceManager)
        {
            return HighlightItemsService.Instance ??= new(await serviceManager.Get<ItemGrabMenuConstructedService>());
        }

        private void OnItemGrabMenuConstructed(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu.inventory.highlightMethod != this.HighlightMethod)
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