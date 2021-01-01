using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.UI;
using StardewHack;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage
{
    internal class ModEntry : HackWithConfig<ModEntry, ModConfig>
    {
        private static readonly PerScreen<int> ScrollTop = new PerScreen<int>();
        private readonly PerScreen<ChestOverlay> _chestOverlay = new PerScreen<ChestOverlay>();
        private DataLoader _dataLoader;

        /// <summary>
        /// Returns the amount to offset the InventoryMenu by depending on if there is an overflow of items to display.
        /// </summary>
        /// <param name="inventoryMenu">The Inventory Menu to base scrolling on.</param>
        /// <returns>The number of slot items to offset inventory by.</returns>
        public static int GetScrollTop(InventoryMenu inventoryMenu) =>
            ScrollTop.Value <= 12 * Math.Ceiling(inventoryMenu.actualInventory.Count / 12f) - inventoryMenu.capacity
                ? ScrollTop.Value
                : 0;
        
        public override void HackEntry(IModHelper helper)
        {
            _dataLoader = new DataLoader(helper, Monitor);

            // Events
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            
            // Patches
            if (!config.AllowModdedCapacity)
                return;
            
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;

            ChestPatches.Init(Monitor, harmony);
            
            Patch(() => new ItemGrabMenu(null, false, false, null, null, null, null, false, false, false, false, false,
                    0, null, 0, null),
                ItemGrabMenu_ctor);
            Patch((InventoryMenu im) => im.leftClick(0,0,null,false),
                InventoryMenu_leftClick);
            Patch((InventoryMenu im) => im.rightClick(0, 0, null, false, false),
                InventoryMenu_rightClick);
            Patch((InventoryMenu im) => im.hover(0, 0, null),
                InventoryMenu_hover);
            Patch((InventoryMenu im) => im.draw(null, 0, 0, 0),
                InventoryMenu_draw);
        }
        /// <summary>
        /// Converts vanilla chests to expanded, if necessary.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Utility.ForAllLocations(delegate(GameLocation location)
            {
                foreach (var itemPosition in location.Objects.Pairs
                    .Where(c =>
                        c.Value is Chest &&
                        c.Value.ShouldBeExpandedStorage() &&
                        !c.Value.IsExpandedStorage()))
                {
                    var pos = itemPosition.Key;
                    var obj = itemPosition.Value;
                    location.Objects[pos] = obj.ToExpandedStorage();
                }
            });
        }
        /// <summary>
        /// Resets scrolling/overlay when chest menu exits or context changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Reset scroll when storage is closed or a different one is accessed.
            if (e.OldMenu is ItemGrabMenu && e.NewMenu is null)
                ScrollTop.Value = 0;
            
            // Menu is not a relevant context or context is unchanged.
            if (!(e.NewMenu is ItemGrabMenu menu) || _chestOverlay.Value?.Menu == menu)
                return;
            
            // Remove old overlay
            if (_chestOverlay.Value != null)
            {
                _chestOverlay.Value?.Dispose();
                _chestOverlay.Value = null;
            }
            
            // Add new overlay
            _chestOverlay.Value = new ChestOverlay(menu, helper.Events, helper.Input);
        }
        
        /// <summary>
        /// Scrolls inventory menu when there is an overflow of items.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!(Game1.activeClickableMenu is ItemGrabMenu {ItemsToGrabMenu: { } inventoryMenu}))
                return;
            if (e.Delta < 0 && ScrollTop.Value < inventoryMenu.actualInventory.Count - inventoryMenu.capacity)
                ScrollTop.Value += inventoryMenu.capacity / inventoryMenu.rows;
            else if (e.Delta > 0 && ScrollTop.Value > 0)
                ScrollTop.Value -= inventoryMenu.capacity / inventoryMenu.rows;
        }
        /// <summary>
        /// Converts objects to modded storage when placed in the world.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Added.Count() != 1)
                return;

            var itemPosition = e.Added.Single();
            var pos = itemPosition.Key;
            var obj = itemPosition.Value;

            if (!obj.ShouldBeExpandedStorage() || obj.IsExpandedStorage())
                return;
            
            Monitor.VerboseLog($"OnObjectListChanged: Converting to Expanded Storage Chest");
            e.Location.objects[pos] = obj.ToExpandedStorage();
        }
        /// <summary>
        /// Loads default chest InventoryMenu when storage has modded capacity.
        /// </summary>
        private void ItemGrabMenu_ctor()
        {
            var code = FindCode(
                Instructions.Isinst(typeof(Chest)),
                Instructions.Callvirt(typeof(Chest), nameof(Chest.GetActualCapacity)),
                Instructions.Ldc_I4_S(36)
            );
            var pos = code.Follow(3);
            code[3] = Instructions.Bge(AttachLabel(pos[0]));
        }
        private void InventoryMenu_leftClick()
        {
            OffsetSlotNumber();
        }
        private void InventoryMenu_rightClick()
        {
            OffsetSlotNumber();
        }
        private void InventoryMenu_hover()
        {
            OffsetSlotNumber();
        }
        /// <summary>
        /// Offsets displayed slots by the scrolled amount.
        /// </summary>
        private void OffsetSlotNumber()
        {
            // Convert.ToInt32(c.name) + ModEntry.ScrollTop(this)
            FindCode(
                Instructions.Ldfld(typeof(ClickableComponent), nameof(ClickableComponent.name)),
                Instructions.Call(typeof(Convert), nameof(Convert.ToInt32), typeof(string))
            ).Append(
                Instructions.Ldarg_0(),
                Instructions.Call(typeof(ModEntry), nameof(GetScrollTop), typeof(InventoryMenu)),
                Instructions.Add()
            );
        }
        /// <summary>
        /// Displays the correct item when InventoryMenu is scrolled.
        /// </summary>
        private void InventoryMenu_draw()
        {
            // actualInventory.Count - ModEntry.GetScrollTop(this)
            var code = FindCode(
                Instructions.Ldfld(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)),
                Instructions.Callvirt_get(typeof(ICollection<Item>), nameof(ICollection<Item>.Count))
            );
            code.Append(
                Instructions.Ldarg_0(),
                Instructions.Call(typeof(ModEntry), nameof(GetScrollTop), typeof(InventoryMenu)),
                Instructions.Sub()
            );
            
            // actualInventory[k + ModEntry.GetScrollTop(this)]
            for (var i = 0; i < 7; i++)
            {
                code = code.FindNext(
                    Instructions.Ldfld(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)),
                    OpCodes.Ldloc_S
                );
                code.Append(
                    Instructions.Ldarg_0(),
                    Instructions.Call(typeof(ModEntry), nameof(GetScrollTop), typeof(InventoryMenu)),
                    Instructions.Add()
                );
            }
        }
    }
}