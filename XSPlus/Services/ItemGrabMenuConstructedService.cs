namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using HarmonyLib;
    using Interfaces;
    using Models;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc/>
    internal class ItemGrabMenuConstructedService : IEventHandlerService<EventHandler<ItemGrabMenuEventArgs>>
    {
        private static readonly ConstructorInfo ItemGrabMenuConstructor = AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) });
        private static ItemGrabMenuConstructedService Instance = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemGrabMenuConstructedService"/> class.
        /// </summary>
        /// <param name="harmony">The Harmony instance for patching the games internal code.</param>
        public ItemGrabMenuConstructedService(Harmony harmony)
        {
            ItemGrabMenuConstructedService.Instance = this;
            harmony.Patch(
                original: ItemGrabMenuConstructedService.ItemGrabMenuConstructor,
                postfix: new HarmonyMethod(typeof(ItemGrabMenuConstructedService), nameof(ItemGrabMenuConstructedService.ItemGrabMenu_constructor_postfix)));
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

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.playerChest.Value)
            {
                return;
            }

            __instance.setBackgroundTransparency(false);

            ItemGrabMenuConstructedService.Instance.InvokeAll(__instance, chest);
        }

        private void InvokeAll(ItemGrabMenu itemGrabMenu, Chest chest)
        {
            var eventArgs = new ItemGrabMenuEventArgs(itemGrabMenu, chest);
            if (this.ItemGrabMenuConstructed != null)
            {
                foreach (Delegate @delegate in this.ItemGrabMenuConstructed.GetInvocationList())
                {
                    @delegate.DynamicInvoke(this, eventArgs);
                }
            }
        }
    }
}