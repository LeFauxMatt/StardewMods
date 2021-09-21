namespace XSPlus.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Models;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <inheritdoc />
    internal class HighlightItemsService : IEventHandlerService<IHighlightItemInterface>
    {
        private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();
        private readonly PerScreen<IList<IHighlightItemInterface>> _highlightItemHandlers = new() { Value = new List<IHighlightItemInterface>() };
        private readonly InventoryType _inventoryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightItemsService"/> class.
        /// </summary>
        /// <param name="itemGrabMenuConstructedService">Service to handle creation/invocation of ItemGrabMenuConstructed event.</param>
        /// <param name="inventoryType">The type of inventory that HighlightItems will apply to.</param>
        /// <returns>Returns a new instance of the <see cref="HighlightItemsService"/> class.</returns>
        public HighlightItemsService(ItemGrabMenuConstructedService itemGrabMenuConstructedService, InventoryType inventoryType)
        {
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);
            this._inventoryType = inventoryType;
        }

        /// <summary>
        /// The type of inventory that HighlightItems will apply to.
        /// </summary>
        public enum InventoryType
        {
            /// <summary>The players inventory.</summary>
            Player,

            /// <summary>The chest inventory.</summary>
            Chest,
        }

        /// <inheritdoc/>
        public void AddHandler(IHighlightItemInterface handler)
        {
            this._highlightItemHandlers.Value.Add(handler);
        }

        /// <inheritdoc/>
        public void RemoveHandler(IHighlightItemInterface handler)
        {
            this._highlightItemHandlers.Value.Remove(handler);
        }

        /// <summary>
        /// Provides logic for reassigning the default highlight method.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemGrabMenuConstructedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            InventoryMenu inventoryMenu = this._inventoryType switch
            {
                InventoryType.Chest => e.ItemGrabMenu!.ItemsToGrabMenu,
                InventoryType.Player => e.ItemGrabMenu!.inventory,
            };
            if (inventoryMenu.highlightMethod != this.HighlightMethod)
            {
                this._highlightMethod.Value = e.ItemGrabMenu.inventory.highlightMethod;
                inventoryMenu.highlightMethod = this.HighlightMethod;
            }
        }

        private bool HighlightMethod(Item item)
        {
            return this._highlightMethod.Value.Invoke(item) && this._highlightItemHandlers.Value.All(highlightMethod => highlightMethod.HighlightMethod(item));
        }
    }
}