namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Helpers;
    using CommonHarmony;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
    using Netcode;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class ExpandedMenu : FeatureWithParam<int>
    {
        private static readonly Type[] ItemGrabMenuConstructorParams = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static readonly Type[] MenuWithInventoryDrawParams = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        private static ExpandedMenu Instance;
        private readonly IInputHelper _inputHelper;
        private readonly Func<KeybindList> _getScrollUp;
        private readonly Func<KeybindList> _getScrollDown;
        private readonly Func<int> _getMaxMenuRows;
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<ItemGrabMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<int> _scrolledAmount = new();
        private readonly PerScreen<int> _menuCapacity = new();
        private readonly PerScreen<int> _menuRows = new();
        private readonly PerScreen<int> _menuOffset = new() { Value = -1 };

        /// <summary>Initializes a new instance of the <see cref="ExpandedMenu"/> class.</summary>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="getScrollUp">Get method for configured scroll up button.</param>
        /// <param name="getScrollDown">Get method for configured scroll down button.</param>
        /// <param name="getMaxMenuRows">Get method for configured default menu rows.</param>
        public ExpandedMenu(IInputHelper inputHelper, Func<KeybindList> getScrollUp, Func<KeybindList> getScrollDown, Func<int> getMaxMenuRows)
            : base("ExpandedMenu")
        {
            ExpandedMenu.Instance = this;
            this._inputHelper = inputHelper;
            this._getScrollUp = getScrollUp;
            this._getScrollDown = getScrollDown;
            this._getMaxMenuRows = getMaxMenuRows;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuConstructor += this.OnItemGrabMenuConstructor;
            CommonFeature.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            harmony.Patch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler)));
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenu.ItemGrabMenuConstructorParams),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_draw_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), ExpandedMenu.MenuWithInventoryDrawParams),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuWithInventory_draw_transpiler)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuConstructor -= this.OnItemGrabMenuConstructor;
            CommonFeature.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            harmony.Unpatch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenu.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_draw_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), ExpandedMenu.MenuWithInventoryDrawParams),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuWithInventory_draw_transpiler)));
        }

        /// <summary>Filter actualInventory to offset by scrolled amount.</summary>
        private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))))
                .Log("Filter actualInventory to offset by scrolled amount.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ScrollItems))));
                })
                .Repeat(-1);

            foreach (CodeInstruction patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(ExpandedMenu)}::{nameof(ExpandedMenu.InventoryMenu_draw_transpiler)}");
            }
        }

        /// <summary>Generate additional slots/rows for top inventory menu.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)36),
                    new CodeInstruction(OpCodes.Beq_S))
                .Log("Changing jump condition from Beq 36 to Bge 10.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    CodeInstruction jumpCode = list.Last.Value;
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10));
                    list.AddLast(new CodeInstruction(OpCodes.Bge_S, jumpCode.operand));
                });

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(InventoryMenu), new[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))))
                .Find(
                    new CodeInstruction(OpCodes.Ldc_I4_M1),
                    new CodeInstruction(OpCodes.Ldc_I4_3))
                .Log("Overriding default values for capacity and rows.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuCapacity))));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuRows))));
                });

            foreach (var patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(ExpandedMenu)}::{nameof(ExpandedMenu.ItemGrabMenu_constructor_transpiler)}");
            }
        }

        /// <summary>Move/resize backpack by expanded menu height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Log("Moving backpack icon down by expanded menu extra height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                })
                .Repeat(3);

            foreach (CodeInstruction patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.");
            }
        }

        /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                    new CodeInstruction(OpCodes.Add))
                .Log("Moving bottom dialogue box down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4, 192),
                    new CodeInstruction(OpCodes.Add))
                .Log("Shrinking bottom dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            foreach (CodeInstruction patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.");
            }
        }

        private static int MenuCapacity(MenuWithInventory menu)
        {
            if (ExpandedMenu.Instance._menuCapacity.Value != 0)
            {
                return ExpandedMenu.Instance._menuCapacity.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.Instance._menuCapacity.Value = -1; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenu.Instance._getMaxMenuRows();
            return ExpandedMenu.Instance._menuCapacity.Value = capacity switch
            {
                < 72 => Math.Min(maxMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => maxMenuRows * 12, // Large
            };
        }

        private static int MenuRows(MenuWithInventory menu)
        {
            if (ExpandedMenu.Instance._menuRows.Value != 0)
            {
                return ExpandedMenu.Instance._menuRows.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.Instance._menuCapacity.Value = 3; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenu.Instance._getMaxMenuRows();
            return ExpandedMenu.Instance._menuRows.Value = capacity switch
            {
                < 72 => (int)Math.Min(maxMenuRows, Math.Ceiling(capacity / 12f)),
                _ => maxMenuRows,
            };
        }

        private static int MenuOffset(MenuWithInventory menu)
        {
            if (ExpandedMenu.Instance._menuOffset.Value != -1)
            {
                return ExpandedMenu.Instance._menuOffset.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.Instance._menuOffset.Value = 0; // Vanilla
            }

            int rows = ExpandedMenu.MenuRows(menu);
            return ExpandedMenu.Instance._menuOffset.Value = Game1.tileSize * (rows - 3);
        }

        private static IList<Item> ScrollItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !ReferenceEquals(itemGrabMenu.ItemsToGrabMenu, inventoryMenu) || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return actualInventory;
            }

            IEnumerable<Item> items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).AsEnumerable();
            items = items.Skip(12 * ExpandedMenu.Instance._scrolledAmount.Value);
            items = items.Take(inventoryMenu.capacity);
            return items.ToList();
        }

        private void OnItemGrabMenuConstructor(object sender, CommonFeature.ItemGrabMenuConstructorEventArgs e)
        {
            if (!this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            int offset = ExpandedMenu.MenuOffset(e.ItemGrabMenu);
            e.ItemGrabMenu.height += offset;
            e.ItemGrabMenu.inventory.movePosition(0, offset);
            if (e.ItemGrabMenu.okButton != null)
            {
                e.ItemGrabMenu.okButton.bounds.Y += offset;
            }

            if (e.ItemGrabMenu.trashCan != null)
            {
                e.ItemGrabMenu.trashCan.bounds.Y += offset;
            }

            if (e.ItemGrabMenu.dropItemInvisibleButton != null)
            {
                e.ItemGrabMenu.dropItemInvisibleButton.bounds.Y += offset;
            }

            e.ItemGrabMenu.RepositionSideButtons();
        }

        private void OnItemGrabMenuChanged(object sender, CommonFeature.ItemGrabMenuChangedEventArgs e)
        {
            if (!e.Attached || !this.IsEnabledForItem(e.Chest))
            {
                this._attached.Value = false;
                this._menu.Value = null;
                this._menuCapacity.Value = 0;
                this._menuRows.Value = 0;
                this._menuOffset.Value = -1;
                return;
            }

            this._attached.Value = true;
            this._menu.Value = e.ItemGrabMenu;

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._scrolledAmount.Value = 0;
            }

            this.SyncInventory();
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            int items = this._chest.Value.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            switch (e.Delta)
            {
                case > 0 when this._scrolledAmount.Value > 0:
                    this._scrolledAmount.Value--;
                    break;
                case < 0 when this._scrolledAmount.Value < Math.Max(0, (items.RoundUp(12) / 12) - this._menu.Value.ItemsToGrabMenu.rows):
                    this._scrolledAmount.Value++;
                    break;
                default:
                    return;
            }

            this.SyncInventory();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            KeybindList getScrollUp = this._getScrollUp();
            if (getScrollUp.JustPressed() && this._scrolledAmount.Value > 0)
            {
                this._scrolledAmount.Value--;
                this.SyncInventory();
                this._inputHelper.SuppressActiveKeybinds(getScrollUp);
                return;
            }

            KeybindList getScrollDown = this._getScrollDown();
            if (getScrollDown.JustPressed() && this._scrolledAmount.Value < Math.Max(0, (this._chest.Value.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count.RoundUp(12) / 12) - this._menu.Value.ItemsToGrabMenu.rows))
            {
                this._scrolledAmount.Value++;
                this.SyncInventory();
                this._inputHelper.SuppressActiveKeybinds(getScrollDown);
            }
        }

        private void SyncInventory()
        {
            if (!this._attached.Value)
            {
                return;
            }

            NetObjectList<Item> items = this._chest.Value.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
            IList<Item> filteredInventory = ExpandedMenu.ScrollItems(items, this._menu.Value.ItemsToGrabMenu);
            for (int i = 0; i < this._menu.Value.ItemsToGrabMenu.inventory.Count; i++)
            {
                Item item = filteredInventory.ElementAtOrDefault(i);
                this._menu.Value.ItemsToGrabMenu.inventory[i].name = item is not null
                    ? items.IndexOf(item).ToString()
                    : items.Count.ToString();
            }
        }
    }
}