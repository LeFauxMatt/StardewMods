namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums;
    using Interfaces;
    using Models;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <inheritdoc cref="BaseService" />
    internal class HighlightItemsService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private static HighlightItemsService ChestInstance;
        private static HighlightItemsService PlayerInstance;
        private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();
        private readonly PerScreen<IList<Func<Item, bool>>> _highlightItemHandlers = new() { Value = new List<Func<Item, bool>>() };
        private readonly InventoryType _inventoryType;

        private HighlightItemsService(
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            InventoryType inventoryType,
            string serviceName)
            : base(serviceName)
        {
            this._inventoryType = inventoryType;
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuEvent);
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuEvent);
        }

        /// <inheritdoc/>
        public void AddHandler(Func<Item, bool> handler)
        {
            this._highlightItemHandlers.Value.Add(handler);
        }

        /// <inheritdoc/>
        public void RemoveHandler(Func<Item, bool> handler)
        {
            this._highlightItemHandlers.Value.Remove(handler);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightItemsService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <param name="inventoryType">The type of inventory that HighlightItems will apply to.</param>
        /// <returns>Returns a new instance of the <see cref="HighlightItemsService"/> class.</returns>
        public static HighlightItemsService GetSingleton(ServiceManager serviceManager, InventoryType inventoryType)
        {
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();

            return inventoryType switch
            {
                InventoryType.Chest => HighlightItemsService.ChestInstance ??= new HighlightItemsService(itemGrabMenuConstructedService, itemGrabMenuChangedService, inventoryType, "HighlightChestItems"),
                InventoryType.Player => HighlightItemsService.PlayerInstance ??= new HighlightItemsService(itemGrabMenuConstructedService, itemGrabMenuChangedService, inventoryType, "HighlightPlayerItems"),
                _ => throw new ArgumentOutOfRangeException(nameof(inventoryType), inventoryType, null),
            };
        }

        /// <summary>
        /// Provides logic for reassigning the default highlight method.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemGrabMenuEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null)
            {
                return;
            }

            var inventoryMenu = this._inventoryType switch
            {
                InventoryType.Chest => e.ItemGrabMenu.ItemsToGrabMenu,
                InventoryType.Player => e.ItemGrabMenu.inventory,
            };
            if (inventoryMenu.highlightMethod != this.HighlightMethod)
            {
                this._highlightMethod.Value = inventoryMenu.highlightMethod;
                inventoryMenu.highlightMethod = this.HighlightMethod;
            }
        }

        private bool HighlightMethod(Item item)
        {
            return this._highlightMethod.Value.Invoke(item) && this._highlightItemHandlers.Value.All(highlightMethod => highlightMethod(item));
        }
    }
}