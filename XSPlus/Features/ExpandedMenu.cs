using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Common.Extensions;
using CommonHarmony;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class ExpandedMenu : FeatureWithParam<int>
    {
        // ReSharper disable InconsistentNaming
        private static readonly Type[] ItemGrabMenu_constructor_params = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static readonly Type[] MenuWithInventory_draw_params = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        // ReSharper restore InconsistentNaming
        private static ExpandedMenu _feature;
        private readonly PerScreen<IClickableMenu> _oldMenu = new();
        private readonly PerScreen<Chest> _context = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _scrolledAmount = new();
        public ExpandedMenu(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            
            // Patches
            Harmony.Patch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)}),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler))
            );
            Harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                postfix: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_postfix))
            );
            Harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_transpiler))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] {typeof(SpriteBatch)}),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_draw_transpiler))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw),MenuWithInventory_draw_params),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuWithInventory_draw_transpiler))
            );
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
            
            // Patches
            Harmony.Unpatch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)}),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler))
            );
            Harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_postfix))
            );
            Harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_transpiler))
            );
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] {typeof(SpriteBatch)}),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_draw_transpiler))
            );
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw),MenuWithInventory_draw_params),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuWithInventory_draw_transpiler))
            );
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, _oldMenu.Value))
                return;
            _oldMenu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } || !IsEnabled(chest))
            {
                Helper.Events.Input.ButtonsChanged -= OnButtonsChanged;
                Helper.Events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
                _attached.Value = false;
            }
            else if (!_attached.Value)
            {
                if (!ReferenceEquals(_context.Value, chest))
                {
                    _context.Value = chest;
                    _scrolledAmount.Value = 0;
                }
                Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
                Helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
                _attached.Value = true;
            }
        }
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (_oldMenu.Value is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu)
                return;
            if (XSPlus.Config.ScrollUp.JustPressed() && _scrolledAmount.Value > 0)
            {
                _scrolledAmount.Value--;
                Helper.Input.SuppressActiveKeybinds(XSPlus.Config.ScrollUp);
            }
            else if (XSPlus.Config.ScrollDown.JustPressed() && _scrolledAmount.Value < Math.Max(0, chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count.RoundUp(12) / 12 - itemGrabMenu.ItemsToGrabMenu.rows))
            {
                _scrolledAmount.Value++;
                Helper.Input.SuppressActiveKeybinds(XSPlus.Config.ScrollDown);
            }
        }
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (_oldMenu.Value is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu)
                return;
            var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
            switch (e.Delta)
            {
                case > 0 when _scrolledAmount.Value > 0:
                    _scrolledAmount.Value--;
                    break;
                case < 0 when _scrolledAmount.Value < Math.Max(0, items.Count.RoundUp(12) / 12 - itemGrabMenu.ItemsToGrabMenu.rows):
                    _scrolledAmount.Value++;
                    break;
                default:
                    return;
            }
            var filteredInventory = ScrollItems(items, itemGrabMenu.ItemsToGrabMenu);
            for (var i = 0; i < itemGrabMenu.ItemsToGrabMenu.inventory.Count; i++)
            {
                var item = filteredInventory.ElementAtOrDefault(i);
                itemGrabMenu.ItemsToGrabMenu.inventory[i].name = item is not null
                    ? items.IndexOf(item).ToString()
                    : items.Count.ToString();
            }
        }
        /// <summary>Filter actualInventory to offset by scrolled amount.</summary>
        private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)))
                )
                .Log("Filter actualInventory to offset by scrolled amount.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ScrollItems))));
                })
                .Repeat(-1);

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(ExpandedMenu)}::{nameof(ExpandedMenu.InventoryMenu_draw_transpiler)}", LogLevel.Warn);
        }
        /// <summary>Resize top inventory menu for expanded rows.</summary>
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !_feature.IsEnabled(chest))
                return;
            var offset = MenuOffset(__instance);
            __instance.height += offset;
            __instance.inventory.movePosition(0, offset);
            if (__instance.okButton != null)
                __instance.okButton.bounds.Y += offset;
            if (__instance.trashCan != null)
                __instance.trashCan.bounds.Y += offset;
            if (__instance.dropItemInvisibleButton != null)
                __instance.dropItemInvisibleButton.bounds.Y += offset;
            __instance.RepositionSideButtons();
        }
        /// <summary>Generate additional slots/rows for top inventory menu.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte) 36),
                    new CodeInstruction(OpCodes.Beq_S)
                )
                .Log("Changing jump condition from Beq 36 to Bge 10.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    var jumpCode = list.Last.Value;
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte) 10));
                    list.AddLast(new CodeInstruction(OpCodes.Bge_S, jumpCode.operand));
                });
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(InventoryMenu), new[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)))
                )
                .Find(
                    new CodeInstruction(OpCodes.Ldc_I4_M1),
                    new CodeInstruction(OpCodes.Ldc_I4_3)
                )
                .Log("Overriding default values for capacity and rows.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(MenuCapacity))));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(MenuRows))));
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(ExpandedMenu)}::{nameof(ItemGrabMenu_constructor_transpiler)}", LogLevel.Warn);
        }
        /// <summary>Move/resize backpack by expanded menu height</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);
            
            patternPatches
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Log("Moving backpack icon down by expanded menu extra height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                })
                .Repeat(3);
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.", LogLevel.Warn);
        }
        /// <summary>Move/resize bottom dialogue box by search bar height</summary>
        private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Moving bottom dialogue box down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4, 192),
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Shrinking bottom dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.", LogLevel.Warn);
        }
        private static IList<Item> ScrollItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
        {
            if (_feature._oldMenu.Value is not ItemGrabMenu { context: Chest chest } itemGrabMenu || !ReferenceEquals(itemGrabMenu.ItemsToGrabMenu, inventoryMenu))
                return actualInventory;
            var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).AsEnumerable();
            items = items.Skip(12 * _feature._scrolledAmount.Value);
            items = items.Take(inventoryMenu.capacity);
            return items.ToList();
        }
        private static int MenuCapacity(ItemGrabMenu menu)
        {
            if (menu.context is not Chest chest || !_feature.IsEnabled(chest))
                return -1;  // Vanilla
            var capacity = chest.GetActualCapacity();
            return capacity switch
            {
                < 72 => Math.Min(XSPlus.Config.MenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => XSPlus.Config.MenuRows * 12 // Large
            };
        }
        private static int MenuRows(ItemGrabMenu menu)
        {
            if (menu.context is not Chest chest || !_feature.IsEnabled(chest))
                return 3; // Vanilla
            var capacity = chest.GetActualCapacity();
            return capacity switch
            {
                < 72 => (int)Math.Min(XSPlus.Config.MenuRows, Math.Ceiling(capacity / 12f)),
                _ => XSPlus.Config.MenuRows
            };
        }
        private static int MenuOffset(MenuWithInventory menu)
        {
            if (menu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !_feature.IsEnabled(chest))
                return 0;
            var rows = MenuRows(itemGrabMenu);
            return 64 * (rows - 3);
        }
    }
}