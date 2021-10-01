namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Interfaces;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
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

        private HighlightItemsService()
            : base("HighlightItems")
        {
            // Patches
            Mixin.Postfix(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.highlightAllItems)),
                typeof(HighlightItemsService),
                nameof(HighlightItemsService.InventoryMenu_highlightAllItems_prefix));
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
        public static HighlightItemsService GetSingleton(ServiceManager serviceManager)
        {
            return HighlightItemsService.Instance ??= new HighlightItemsService();
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void InventoryMenu_highlightAllItems_prefix(InventoryMenu __instance, ref bool __result, Item i)
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu {inventory: { } inventoryMenu} || !ReferenceEquals(inventoryMenu, __instance))
            {
                return;
            }

            __result = __result && HighlightItemsService.Instance.HighlightMethod(i);
        }

        private bool HighlightMethod(Item item)
        {
            return this._highlightMethod.Value.Invoke(item) && this._highlightItemHandlers.Value.All(highlightMethod => highlightMethod(item));
        }
    }
}