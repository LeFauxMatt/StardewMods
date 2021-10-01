namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Interfaces;
    using Common.Models;
    using Common.Services;
    using CommonHarmony;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <summary>
    /// Service for manipulating the displayed items in an inventory menu.
    /// </summary>
    internal class DisplayedInventoryService : BaseService, IEventHandlerService<Func<Item, bool>>
    {
        private static DisplayedInventoryService Instance;
        private readonly PerScreen<IList<Func<Item, bool>>> _filterItemHandlers = new() { Value = new List<Func<Item, bool>>() };
        private readonly PerScreen<IList<Item>> _items = new();
        private readonly PerScreen<Range<int>> _range = new() { Value = new Range<int>() };
        private readonly PerScreen<InventoryMenu> _menu = new();
        private readonly PerScreen<int> _columns = new();
        private readonly PerScreen<int> _offset = new();

        private DisplayedInventoryService(ItemGrabMenuConstructedService itemGrabMenuConstructedService, ItemGrabMenuChangedService itemGrabMenuChangedService)
            : base("DisplayedInventory")
        {
            // Events
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuEvent);
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuEvent);
            Events.Player.InventoryChanged += this.OnInventoryChanged;

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
        /// <returns>An instance of the <see cref="DisplayedInventoryService"/> class.</returns>
        public static DisplayedInventoryService GetSingleton(ServiceManager serviceManager)
        {
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            return DisplayedInventoryService.Instance ??= new DisplayedInventoryService(itemGrabMenuConstructedService, itemGrabMenuChangedService);
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
                if (item is not null)
                {
                    this._menu.Value.inventory[i].name = this._items.Value.IndexOf(item).ToString();
                    return;
                }

                this._menu.Value.inventory[i].name = (i < this._items.Value.Count ? int.MaxValue : i).ToString();
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
            return ReferenceEquals(DisplayedInventoryService.Instance._menu.Value, inventoryMenu)
                ? DisplayedInventoryService.Instance.Items.Take(inventoryMenu.capacity).ToList()
                : actualInventory;
        }

        private void OnItemGrabMenuEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null)
            {
                this._menu.Value = null;
                return;
            }

            this._menu.Value = e.ItemGrabMenu.ItemsToGrabMenu;
            this._columns.Value = e.ItemGrabMenu.ItemsToGrabMenu.capacity / e.ItemGrabMenu.ItemsToGrabMenu.rows;
            this._items.Value = e.ItemGrabMenu.ItemsToGrabMenu.actualInventory;
            this.ReSyncInventory();
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            this.ReSyncInventory();
        }

        private bool FilterMethod(Item item)
        {
            return this._filterItemHandlers.Value.Count == 0 || this._filterItemHandlers.Value.All(filterMethod => filterMethod(item));
        }
    }
}