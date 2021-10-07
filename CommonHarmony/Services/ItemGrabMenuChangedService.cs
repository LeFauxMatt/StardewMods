namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Enums;
    using HarmonyLib;
    using Interfaces;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="BaseService" />
    internal class ItemGrabMenuChangedService : BaseService, IEventHandlerService<EventHandler<ItemGrabMenuEventArgs>>
    {
        private static readonly ConstructorInfo ItemGrabMenuConstructor = AccessTools.Constructor(
            typeof(ItemGrabMenu),
            new[]
            {
                typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
            });
        private static ItemGrabMenuChangedService Instance;
        private readonly PerScreen<bool> _attached = new();
        private readonly List<ServiceHandler<EventHandler<ItemGrabMenuEventArgs>>> _handlers = new();
        private readonly PerScreen<IClickableMenu> _menu = new();
        private ServiceHandler<EventHandler<ItemGrabMenuEventArgs>>[] _cachedHandlers = new ServiceHandler<EventHandler<ItemGrabMenuEventArgs>>[0];
        private int _handlerCount;
        private bool _hasNewHandlers;

        private ItemGrabMenuChangedService()
            : base("ItemGrabMenuConstructed")
        {
            // Events
            Events.Display.MenuChanged += this.OnMenuChanged;

            // Patches
            Mixin.Postfix(
                ItemGrabMenuChangedService.ItemGrabMenuConstructor,
                typeof(ItemGrabMenuChangedService),
                nameof(ItemGrabMenuChangedService.ItemGrabMenu_constructor_postfix));
        }

        /// <inheritdoc />
        public void AddHandler(EventHandler<ItemGrabMenuEventArgs> handler)
        {
            lock (this._handlers)
            {
                var priority = handler.Method.GetCustomAttribute<HandlerPriorityAttribute>()?.Priority ?? HandlerPriority.Normal;
                var serviceHandler = new ServiceHandler<EventHandler<ItemGrabMenuEventArgs>>(handler, this._handlerCount++, priority);
                this._handlers.Add(serviceHandler);
                this._cachedHandlers = null;
                this._hasNewHandlers = true;
            }
        }

        /// <inheritdoc />
        public void RemoveHandler(EventHandler<ItemGrabMenuEventArgs> handler)
        {
            lock (this._handlers)
            {
                for (var i = this._handlers.Count - 1; i >= 0; i--)
                {
                    if (this._handlers[i].Handler != handler)
                    {
                        continue;
                    }

                    this._handlers.RemoveAt(i);
                    this._cachedHandlers = null;
                    break;
                }
            }
        }

        private event EventHandler<ItemGrabMenuEventArgs> ItemGrabMenuConstructed;

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="ItemGrabMenuChangedService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuChangedService" /> class.</returns>
        public static async Task<ItemGrabMenuChangedService> Create(ServiceManager serviceManager)
        {
            return ItemGrabMenuChangedService.Instance ??= new();
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            ItemGrabMenuChangedService.Instance._menu.Value = __instance;

            if (__instance is not {shippingBin: false, context: Chest {playerChest: {Value: true}} chest})
            {
                ItemGrabMenuChangedService.Instance._attached.Value = false;
                ItemGrabMenuChangedService.Instance.InvokeAll(__instance, null, -1);
                return;
            }

            __instance.setBackgroundTransparency(false);

            ItemGrabMenuChangedService.Instance._attached.Value = true;
            ItemGrabMenuChangedService.Instance.InvokeAll(__instance, chest, Context.ScreenId, true);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this._menu.Value))
            {
                return;
            }

            this._menu.Value = e.NewMenu;
            if (this._menu.Value is not ItemGrabMenu {shippingBin: false, context: Chest {playerChest: {Value: true}} chest} itemGrabMenu)
            {
                this._attached.Value = false;
                this.InvokeAll(null, null, -1);
                return;
            }

            this._attached.Value = true;
            this.InvokeAll(itemGrabMenu, chest, Context.ScreenId);
        }

        private void InvokeAll(ItemGrabMenu itemGrabMenu, Chest chest, int screenId, bool isNew = false)
        {
            if (this._handlers.Count == 0)
            {
                return;
            }

            var eventArgs = new ItemGrabMenuEventArgs(itemGrabMenu, chest, screenId, isNew);
            var handlers = this._cachedHandlers;
            if (handlers is null)
            {
                lock (this._handlers)
                {
                    if (this._hasNewHandlers && this._handlers.Any(p => p.Priority != HandlerPriority.Normal))
                    {
                        this._handlers.Sort();
                    }

                    this._cachedHandlers = handlers = this._handlers.ToArray();
                    this._hasNewHandlers = false;
                }
            }

            foreach (var serviceHandler in handlers)
            {
                try
                {
                    serviceHandler.Handler.Invoke(this, eventArgs);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed in {nameof(ItemGrabMenuChangedService)}. {ex.Message}");
                }
            }
        }
    }
}