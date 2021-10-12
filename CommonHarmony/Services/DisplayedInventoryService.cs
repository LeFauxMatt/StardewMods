namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Models;
    using Common.Services;
    using Enums;
    using HarmonyLib;
    using Interfaces;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <summary>
    ///     Service for manipulating the displayed items in an inventory menu.
    /// </summary>
    internal class DisplayedInventoryService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private static DisplayedInventoryService Instance;
        private readonly PerScreen<int> _columns = new();
        private readonly PerScreen<List<Item>> _filteredItems = new();
        private readonly IList<Func<Item, bool>> _filterItemHandlers = new List<Func<Item, bool>>();
        private readonly PerScreen<IList<Item>> _items = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private readonly PerScreen<int> _offset = new();
        private readonly PerScreen<Range<int>> _range = new(() => new());
        private readonly PerScreen<int> _rows = new();

        private DisplayedInventoryService(ServiceManager serviceManager)
            : base("DisplayedInventory")
        {
            // Init
            DisplayedInventoryService.Instance ??= this;

            // Dependencies
            this.AddDependency<ItemGrabMenuChangedService>(service => (service as ItemGrabMenuChangedService)?.AddHandler(this.OnItemGrabMenuChanged));
            this.AddDependency<HarmonyService>(
                service =>
                {
                    var harmony = service as HarmonyService;
                    var methodParams = new[]
                    {
                        typeof(SpriteBatch), typeof(int), typeof(int), typeof(int),
                    };

                    harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), methodParams),
                        typeof(DisplayedInventoryService),
                        nameof(DisplayedInventoryService.InventoryMenu_draw_transpiler),
                        PatchType.Transpiler);

                    harmony?.ApplyPatches(this.ServiceName);
                });

            // Events
            serviceManager.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        }

        /// <summary>
        ///     Gets or sets the number of rows the currently displayed items are offset by.
        /// </summary>
        public int Offset
        {
            get => this._range.Value.Clamp(this._offset.Value);
            set
            {
                value = this._range.Value.Clamp(value);
                if (this._offset.Value != value && this._menu.Value is not null)
                {
                    this._offset.Value = value;
                    this.ReSyncInventory(this._menu.Value.ItemGrabMenu.ItemsToGrabMenu);
                }
            }
        }

        /// <summary>
        ///     Gets the displayed items.
        /// </summary>
        private IEnumerable<Item> Items
        {
            get
            {
                var offset = this.Offset * this._columns.Value;
                for (var i = 0; i < this._filteredItems.Value.Count; i++)
                {
                    var item = this._filteredItems.Value.ElementAtOrDefault(i);
                    if (item is null)
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

        /// <inheritdoc />
        public void AddHandler(Func<Item, bool> handler)
        {
            this._filterItemHandlers.Add(handler);
        }

        /// <inheritdoc />
        public void RemoveHandler(Func<Item, bool> handler)
        {
            this._filterItemHandlers.Remove(handler);
        }

        /// <summary>
        ///     Forces displayed inventory to resync.
        /// </summary>
        public void ReSyncInventory(InventoryMenu inventoryMenu, bool reset = false)
        {
            if (reset)
            {
                this._offset.Value = 0;
            }

            this._filteredItems.Value = new(this._filterItemHandlers.Count == 0 ? this._items.Value : this._items.Value.Where(this.FilterMethod));
            this._range.Value.Maximum = Math.Max(0, this._filteredItems.Value.Count.RoundUp(this._columns.Value) / this._columns.Value - this._rows.Value);

            var slot = 0;
            foreach (var item in this.Items)
            {
                inventoryMenu.inventory[slot++].name = (item is not null ? this._items.Value.IndexOf(item) : int.MaxValue).ToString();
                if (slot == inventoryMenu.inventory.Count)
                {
                    return;
                }
            }

            do
            {
                inventoryMenu.inventory[slot++].name = int.MaxValue.ToString();
            } while (slot < inventoryMenu.inventory.Count);
        }

        private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Filter actualInventory to managed inventory.");
            var scrollItemsPatch = new PatternPatch();
            scrollItemsPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
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
            return DisplayedInventoryService.Instance._menu.Value is not null && ReferenceEquals(DisplayedInventoryService.Instance._menu.Value.ItemGrabMenu.ItemsToGrabMenu, inventoryMenu)
                ? DisplayedInventoryService.Instance.Items.Take(inventoryMenu.capacity).ToList()
                : actualInventory;
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null)
            {
                this._menu.Value = null;
                return;
            }

            if (this._menu.Value is null || !ReferenceEquals(e.Chest, this._menu.Value.Chest))
            {
                this._columns.Value = e.ItemGrabMenu.ItemsToGrabMenu.capacity / e.ItemGrabMenu.ItemsToGrabMenu.rows;
                this._rows.Value = e.ItemGrabMenu.ItemsToGrabMenu.rows;
                this._items.Value = e.ItemGrabMenu.ItemsToGrabMenu.actualInventory;
                this.ReSyncInventory(e.ItemGrabMenu.ItemsToGrabMenu, true);
            }

            this._menu.Value = e;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (this._menu.Value is not null)
            {
                this.ReSyncInventory(this._menu.Value.ItemGrabMenu.ItemsToGrabMenu);
            }
        }

        private bool FilterMethod(Item item)
        {
            return this._filterItemHandlers.Count == 0 || this._filterItemHandlers.All(filterMethod => filterMethod(item));
        }
    }
}