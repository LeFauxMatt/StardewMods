namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Interfaces;
    using Models;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="BaseService" />
    internal class ItemGrabMenuConstructedService : BaseService, IEventHandlerService<EventHandler<ItemGrabMenuEventArgs>>
    {
        private static readonly ConstructorInfo ItemGrabMenuConstructor = AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) });
        private static ItemGrabMenuConstructedService Instance;

        private ItemGrabMenuConstructedService()
            : base("ItemGrabMenuConstructed")
        {
            Mixin.Postfix(
                ItemGrabMenuConstructedService.ItemGrabMenuConstructor,
                typeof(ItemGrabMenuConstructedService),
                nameof(ItemGrabMenuConstructedService.ItemGrabMenu_constructor_postfix));
        }

        private event EventHandler<ItemGrabMenuEventArgs>? ItemGrabMenuConstructed;

        /// <inheritdoc/>
        public void AddHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuConstructed += eventHandler;
        }

        /// <inheritdoc/>
        public void RemoveHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuConstructed -= eventHandler;
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ItemGrabMenuConstructedService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuConstructedService"/> class.</returns>
        public static ItemGrabMenuConstructedService GetSingleton(ServiceManager serviceManager)
        {
            return ItemGrabMenuConstructedService.Instance ??= new ItemGrabMenuConstructedService();
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest { playerChest: { Value: true } } chest)
            {
                ItemGrabMenuConstructedService.Instance.InvokeAll(__instance, null);
                return;
            }

            __instance.setBackgroundTransparency(false);

            ItemGrabMenuConstructedService.Instance.InvokeAll(__instance, chest);
        }

        private void InvokeAll(ItemGrabMenu itemGrabMenu, Chest chest)
        {
            var eventArgs = new ItemGrabMenuEventArgs(itemGrabMenu, chest);
            this.ItemGrabMenuConstructed?.Invoke(this, eventArgs);
        }
    }
}