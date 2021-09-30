namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Enums;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Interfaces;
    using Common.Models;
    using Common.Services;
    using CommonHarmony;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <summary>
    /// Service for manipulating the displayed items in an inventory menu.
    /// </summary>
    internal class DisplayedInventoryService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private static DisplayedInventoryService ChestInstance;
        private static DisplayedInventoryService PlayerInstance;
        private readonly PerScreen<IList<Func<Item, bool>>> _filterItemHandlers = new() { Value = new List<Func<Item, bool>>() };
        private readonly InventoryType _inventoryType;
        private readonly PerScreen<object> _context = new();
        private readonly PerScreen<IList<Item>> _items = new();
        private readonly PerScreen<Range<int>> _range = new() { Value = new Range<int>() };
        private readonly PerScreen<InventoryMenu> _menu = new();
        private readonly PerScreen<int> _columns = new();
        private readonly PerScreen<int> _offset = new();

        private DisplayedInventoryService(ItemGrabMenuConstructedService itemGrabMenuConstructedService, InventoryType inventoryType, string serviceName)
            : base(serviceName)
        {
            this._inventoryType = inventoryType;

            // Events
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);

            // Patches
            Mixin.Transpiler(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                typeof(DisplayedInventoryService),
                nameof(DisplayedInventoryService.InventoryMenu_draw_transpiler));
        }

        /// <summary>
        /// Gets or sets the number of rows the currently displayed items are offset by.
        /// </summary>
        public int Offset
        {
            get => this._offset.Value;
            set
            {
                this._range.Value.Maximum = Math.Max(0, (this._items.Value.Count.RoundUp(this._columns.Value) / this._columns.Value) - this._menu.Value.rows);
                value = this._range.Value.Clamp(value);
                if (this._offset.Value != value)
                {
                    this._offset.Value = value;
                    this.ReSyncInventory();
                }
            }
        }

        /// <summary>
        /// Gets the displayed items.
        /// </summary>
        private IEnumerable<Item> Items
        {
            get
            {
                var offset = this._offset.Value * this._columns.Value;
                for (var i = 0; i < this._items.Value.Count; i++)
                {
                    var item = this._items.Value.ElementAtOrDefault(i);
                    if (item is null || !this.FilterMethod(item))
                    {
                        continue;
                    }

                    if (offset > 0)
                    {
                        offset--;
                        continue;
                    }

                    yield return item;
                }
            }
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="DisplayedInventoryService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <param name="inventoryType">The type of inventory that DisplayedInventory will apply to.</param>
        /// <returns>An instance of the <see cref="DisplayedInventoryService"/> class.</returns>
        public static DisplayedInventoryService GetSingleton(ServiceManager serviceManager, InventoryType inventoryType)
        {
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();

            return inventoryType switch
            {
                InventoryType.Chest => DisplayedInventoryService.ChestInstance ??= new DisplayedInventoryService(itemGrabMenuConstructedService, inventoryType, "DisplayedChestInventory"),
                InventoryType.Player => DisplayedInventoryService.PlayerInstance ??= new DisplayedInventoryService(itemGrabMenuConstructedService, inventoryType, "DisplayedPlayerInventory"),
                _ => throw new ArgumentOutOfRangeException(nameof(inventoryType), inventoryType, null),
            };
        }

        /// <inheritdoc/>
        public void AddHandler(Func<Item, bool> handler)
        {
            this._filterItemHandlers.Value.Add(handler);
        }

        /// <inheritdoc/>
        public void RemoveHandler(Func<Item, bool> handler)
        {
            this._filterItemHandlers.Value.Remove(handler);
        }

        /// <summary>
        /// Forces displayed inventory to resync.
        /// </summary>
        public void ReSyncInventory()
        {
            IList<Item> items = this.Items.Take(this._menu.Value.inventory.Count).ToList();
            for (var i = 0; i < this._menu.Value.inventory.Count; i++)
            {
                var item = items.ElementAtOrDefault(i);
                var index = item is not null ? this._items.Value.IndexOf(item) : int.MaxValue;
                this._menu.Value.inventory[i].name = index.ToString();
            }
        }

        private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Filter actualInventory to managed inventory.");
            var scrollItemsPatch = new PatternPatch(PatchType.Replace);
            scrollItemsPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisplayedInventoryService), nameof(DisplayedInventoryService.DisplayedItems))));
                })
                .Repeat(-1);

            var patternPatches = new PatternPatches(instructions, scrollItemsPatch);
            foreach (var patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(DisplayedInventoryService)}::{nameof(DisplayedInventoryService.InventoryMenu_draw_transpiler)}");
            }
        }

        private static IList<Item> DisplayedItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest { playerChest: { Value: true } } } itemGrabMenu)
            {
                return actualInventory;
            }

            if (ReferenceEquals(itemGrabMenu.ItemsToGrabMenu, inventoryMenu) && DisplayedInventoryService.ChestInstance is not null)
            {
                return DisplayedInventoryService.ChestInstance.Items.Take(inventoryMenu.capacity).ToList();
            }

            if (ReferenceEquals(itemGrabMenu.inventory, inventoryMenu) && DisplayedInventoryService.PlayerInstance is not null)
            {
                return DisplayedInventoryService.PlayerInstance.Items.Take(inventoryMenu.capacity).ToList();
            }

            return actualInventory;
        }

        private void OnItemGrabMenuConstructedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null)
            {
                return;
            }

            var menu = this._inventoryType switch
            {
                InventoryType.Chest => e.ItemGrabMenu.ItemsToGrabMenu,
                InventoryType.Player => e.ItemGrabMenu.inventory,
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (!ReferenceEquals(this._context.Value, e.ItemGrabMenu.context))
            {
                this._context.Value = e.ItemGrabMenu.context;
                this._items.Value = menu.actualInventory;
            }

            this._menu.Value = menu;
            this._columns.Value = menu.capacity / menu.rows;
            this.ReSyncInventory();
        }

        private bool FilterMethod(Item item)
        {
            return this._filterItemHandlers.Value.Count == 0 || this._filterItemHandlers.Value.All(filterMethod => filterMethod(item));
        }
    }
}