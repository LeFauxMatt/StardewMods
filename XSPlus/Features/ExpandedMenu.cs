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
        private static readonly PerScreen<int> MenuCapacity = new();
        private static readonly PerScreen<int> MenuRows = new();
        private static readonly PerScreen<int> MenuOffset = new() { Value = -1 };
        private static ExpandedMenu Instance;
        private readonly IInputHelper InputHelper;
        private readonly Func<KeybindList> GetScrollUp;
        private readonly Func<KeybindList> GetScrollDown;
        private readonly Func<int> GetMaxMenuRows;
        private readonly PerScreen<IClickableMenu> Menu = new();
        private readonly PerScreen<Chest> Chest = new();
        private readonly PerScreen<bool> Attached = new();
        private readonly PerScreen<int> ScrolledAmount = new();

        /// <summary>Initializes a new instance of the <see cref="ExpandedMenu"/> class.</summary>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="getScrollUp">Get method for configured scroll up button.</param>
        /// <param name="getScrollDown">Get method for configured scroll down button.</param>
        /// <param name="getMaxMenuRows">Get method for configured default menu rows.</param>
        public ExpandedMenu(IInputHelper inputHelper, Func<KeybindList> getScrollUp, Func<KeybindList> getScrollDown, Func<int> getMaxMenuRows)
            : base("ExpandedMenu")
        {
            ExpandedMenu.Instance = this;
            this.InputHelper = inputHelper;
            this.GetScrollUp = getScrollUp;
            this.GetScrollDown = getScrollDown;
            this.GetMaxMenuRows = getMaxMenuRows;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Display.MenuChanged += this.OnMenuChanged;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            harmony.Patch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                transpiler: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler)));
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenu.ItemGrabMenuConstructorParams),
                postfix: new HarmonyMethod(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_postfix)));
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
            modEvents.Display.MenuChanged -= this.OnMenuChanged;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            harmony.Unpatch(
                AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.InventoryMenu_draw_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenu.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.ItemGrabMenu_constructor_postfix)));
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

        /// <summary>Resize top inventory menu for expanded rows.</summary>
        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return;
            }

            int offset = ExpandedMenu.GetMenuOffset(__instance);
            __instance.height += offset;
            __instance.inventory.movePosition(0, offset);
            if (__instance.okButton != null)
            {
                __instance.okButton.bounds.Y += offset;
            }

            if (__instance.trashCan != null)
            {
                __instance.trashCan.bounds.Y += offset;
            }

            if (__instance.dropItemInvisibleButton != null)
            {
                __instance.dropItemInvisibleButton.bounds.Y += offset;
            }

            __instance.RepositionSideButtons();
        }

        /// <summary>Generate additional slots/rows for top inventory menu.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(StardewValley.Objects.Chest.GetActualCapacity))),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)36),
                    new CodeInstruction(OpCodes.Beq_S))
                .Log("Changing jump condition from Beq 36 to Bge 10.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    var jumpCode = list.Last.Value;
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
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.GetMenuCapacity))));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.GetMenuRows))));
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
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.GetMenuOffset))));
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
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.GetMenuOffset))));
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
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenu), nameof(ExpandedMenu.GetMenuOffset))));
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

        private static int GetMenuCapacity(MenuWithInventory menu)
        {
            if (ExpandedMenu.MenuCapacity.Value != 0)
            {
                return ExpandedMenu.MenuCapacity.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.MenuCapacity.Value = -1; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenu.Instance.GetMaxMenuRows();
            return ExpandedMenu.MenuCapacity.Value = capacity switch
            {
                < 72 => Math.Min(maxMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => maxMenuRows * 12, // Large
            };
        }

        private static int GetMenuRows(MenuWithInventory menu)
        {
            if (ExpandedMenu.MenuRows.Value != 0)
            {
                return ExpandedMenu.MenuRows.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.MenuCapacity.Value = 3; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenu.Instance.GetMaxMenuRows();
            return ExpandedMenu.MenuRows.Value = capacity switch
            {
                < 72 => (int)Math.Min(maxMenuRows, Math.Ceiling(capacity / 12f)),
                _ => maxMenuRows,
            };
        }

        private static int GetMenuOffset(MenuWithInventory menu)
        {
            if (ExpandedMenu.MenuOffset.Value != -1)
            {
                return ExpandedMenu.MenuOffset.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenu.MenuOffset.Value = 0; // Vanilla
            }

            int rows = ExpandedMenu.GetMenuRows(menu);
            return ExpandedMenu.MenuOffset.Value = Game1.tileSize * (rows - 3);
        }

        private static IList<Item> ScrollItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !ReferenceEquals(itemGrabMenu.ItemsToGrabMenu, inventoryMenu) || !ExpandedMenu.Instance.IsEnabledForItem(chest))
            {
                return actualInventory;
            }

            var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).AsEnumerable();
            items = items.Skip(12 * ExpandedMenu.Instance.ScrolledAmount.Value);
            items = items.Take(inventoryMenu.capacity);
            return items.ToList();
        }

        private static void SyncInventory()
        {
            if (Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu)
            {
                return;
            }

            var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
            var filteredInventory = ExpandedMenu.ScrollItems(items, itemGrabMenu.ItemsToGrabMenu);
            for (int i = 0; i < itemGrabMenu.ItemsToGrabMenu.inventory.Count; i++)
            {
                Item item = filteredInventory.ElementAtOrDefault(i);
                itemGrabMenu.ItemsToGrabMenu.inventory[i].name = item is not null
                    ? items.IndexOf(item).ToString()
                    : items.Count.ToString();
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this.Menu.Value))
            {
                return;
            }

            this.Menu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } || !this.IsEnabledForItem(chest))
            {
                this.Attached.Value = false;
                ExpandedMenu.MenuCapacity.Value = 0;
                ExpandedMenu.MenuRows.Value = 0;
                ExpandedMenu.MenuOffset.Value = -1;
                return;
            }

            if (!ReferenceEquals(this.Chest.Value, chest))
            {
                this.Chest.Value = chest;
                this.ScrolledAmount.Value = 0;
            }

            if (!this.Attached.Value)
            {
                this.Attached.Value = true;
            }

            ExpandedMenu.SyncInventory();
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!this.Attached.Value || Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu)
            {
                return;
            }

            int items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            switch (e.Delta)
            {
                case > 0 when this.ScrolledAmount.Value > 0:
                    this.ScrolledAmount.Value--;
                    break;
                case < 0 when this.ScrolledAmount.Value < Math.Max(0, (items.RoundUp(12) / 12) - itemGrabMenu.ItemsToGrabMenu.rows):
                    this.ScrolledAmount.Value++;
                    break;
                default:
                    return;
            }

            ExpandedMenu.SyncInventory();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this.Attached.Value || Game1.activeClickableMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu)
            {
                return;
            }

            KeybindList getScrollUp = this.GetScrollUp();
            if (getScrollUp.JustPressed() && this.ScrolledAmount.Value > 0)
            {
                this.ScrolledAmount.Value--;
                ExpandedMenu.SyncInventory();
                this.InputHelper.SuppressActiveKeybinds(getScrollUp);
                return;
            }

            KeybindList getScrollDown = this.GetScrollDown();
            if (getScrollDown.JustPressed() && this.ScrolledAmount.Value < Math.Max(0, (chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count.RoundUp(12) / 12) - itemGrabMenu.ItemsToGrabMenu.rows))
            {
                this.ScrolledAmount.Value++;
                ExpandedMenu.SyncInventory();
                this.InputHelper.SuppressActiveKeybinds(getScrollDown);
            }
        }
    }
}