using System.Collections.Generic;
using System.Reflection.Emit;
using Harmony;
using ImJustMatt.Common.PatternPatches;
using ImJustMatt.ExpandedStorage.Framework.Models;
using ImJustMatt.ExpandedStorage.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

// ReSharper disable InvertIf
// ReSharper disable InconsistentNaming

namespace ImJustMatt.ExpandedStorage.Framework.Patches
{
    internal class ItemGrabMenuPatch : MenuPatch
    {
        private static IReflectionHelper _reflection;

        internal ItemGrabMenuPatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection) : base(monitor, config)
        {
            _reflection = reflection;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            var constructor = AccessTools.Constructor(typeof(ItemGrabMenu),
                new[]
                {
                    typeof(IList<Item>),
                    typeof(bool),
                    typeof(bool),
                    typeof(InventoryMenu.highlightThisItem),
                    typeof(ItemGrabMenu.behaviorOnItemSelect),
                    typeof(string),
                    typeof(ItemGrabMenu.behaviorOnItemSelect),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(int),
                    typeof(Item),
                    typeof(int),
                    typeof(object)
                });

            harmony.Patch(
                constructor,
                transpiler: new HarmonyMethod(GetType(), nameof(ConstructorTranspiler))
            );

            harmony.Patch(
                constructor,
                postfix: new HarmonyMethod(GetType(), nameof(ConstructorPostfix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] {typeof(SpriteBatch)}),
                transpiler: new HarmonyMethod(GetType(), nameof(DrawTranspiler))
            );
        }

        /// <summary>Loads default chest InventoryMenu when storage has modded capacity.</summary>
        private static IEnumerable<CodeInstruction> ConstructorTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
                    new CodeInstruction(OpCodes.Ldc_I4_S),
                    new CodeInstruction(OpCodes.Beq)
                )
                .Log("Changing jump condition from Beq 36 to Bge 10.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    var jumpCode = list.Last.Value;
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (byte) 10));
                    list.AddLast(new CodeInstruction(OpCodes.Bge, jumpCode.operand));
                });

            var inventoryMenuConstructor = AccessTools.Constructor(typeof(InventoryMenu), new[]
            {
                typeof(int),
                typeof(int),
                typeof(bool),
                typeof(IList<Item>),
                typeof(InventoryMenu.highlightThisItem),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(bool)
            });

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Newobj, inventoryMenuConstructor),
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
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_S, (byte) 16));
                    list.AddLast(new CodeInstruction(OpCodes.Call, MenuCapacity));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_S, (byte) 16));
                    list.AddLast(new CodeInstruction(OpCodes.Call, MenuRows));
                });

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(ConstructorTranspiler)}", LogLevel.Warn);
        }

        private static void ConstructorPostfix(ItemGrabMenu __instance)
        {
            var config = ExpandedStorage.GetConfig(__instance.context);
            if (config == null || __instance.context is ShippingBin)
                return;

            __instance.ItemsToGrabMenu.rows = MenuModel.GetRows(__instance.context);
            __instance.ItemsToGrabMenu.capacity = MenuModel.GetMenuCapacity(__instance.context);

            if (__instance.context is not Chest chest)
                chest = null;

            if (ExpandedStorage.HeldChest.Value != null
                && chest != null
                && !ReferenceEquals(ExpandedStorage.HeldChest.Value, chest))
            {
                var reflectedBehaviorFunction = _reflection.GetField<ItemGrabMenu.behaviorOnItemSelect>(__instance, "behaviorFunction");
                reflectedBehaviorFunction.SetValue(delegate(Item item, Farmer who)
                {
                    var tmp = chest.addItem(item);
                    if (tmp == null)
                    {
                        ExpandedStorage.HeldChest.Value.GetItemsForPlayer(who.UniqueMultiplayerID).Remove(item);
                        ExpandedStorage.HeldChest.Value.clearNulls();
                        MenuViewModel.RefreshItems();
                    }

                    chest.ShowMenu();
                    if (Game1.activeClickableMenu is ItemGrabMenu menu)
                        menu.heldItem = tmp;
                });

                __instance.behaviorOnItemGrab = delegate(Item item, Farmer who)
                {
                    __instance.heldItem = ExpandedStorage.HeldChest.Value.addItem(item);
                    if (__instance.heldItem == null)
                    {
                        chest.GetItemsForPlayer(who.UniqueMultiplayerID).Remove(item);
                        chest.clearNulls();
                        MenuViewModel.RefreshItems();
                    }
                };

                __instance.inventory = new InventoryMenu(
                    __instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2,
                    __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16,
                    false,
                    ExpandedStorage.HeldChest.Value.GetItemsForPlayer(Game1.player.UniqueMultiplayerID),
                    config.HighlightMethod);
            }
            else
            {
                __instance.inventory.highlightMethod = config.HighlightMethod;
            }

            if (config.SourceType == SourceType.JsonAssets && chest != null && __instance.chestColorPicker == null)
            {
                var sourceItemReflected = _reflection.GetField<Item>(__instance, "sourceItem");
                var sourceItem = sourceItemReflected.GetValue();

                // Add color picker back to special Expanded Storage Chests
                var colorPickerChest = new Chest(true, sourceItem.ParentSheetIndex);
                var chestColorPicker = new DiscreteColorPicker(
                    __instance.xPositionOnScreen,
                    __instance.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2,
                    0,
                    colorPickerChest);

                colorPickerChest.playerChoiceColor.Value = chest.playerChoiceColor.Value;
                chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor(chest.playerChoiceColor.Value);
                __instance.chestColorPicker = chestColorPicker;

                __instance.colorPickerToggleButton = new ClickableTextureComponent(
                    new Rectangle(__instance.xPositionOnScreen + __instance.width,
                        __instance.yPositionOnScreen + __instance.height / 3 - 64 + -160, 64, 64),
                    Game1.mouseCursors,
                    new Rectangle(119, 469, 16, 16),
                    4f)
                {
                    hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
                    myID = 27346,
                    downNeighborID = -99998,
                    leftNeighborID = 53921,
                    region = 15923
                };

                var discreteColorPickerCC = new List<ClickableComponent>();
                for (var i = 0; i < chestColorPicker.totalColors; i++)
                    discreteColorPickerCC.Add(
                        new ClickableComponent(
                            new Rectangle(
                                chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4,
                                chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36, 28), "")
                        {
                            myID = i + 4343,
                            rightNeighborID = i < chestColorPicker.totalColors - 1 ? i + 4343 + 1 : -1,
                            leftNeighborID = i > 0 ? i + 4343 - 1 : -1,
                            downNeighborID = __instance.ItemsToGrabMenu.inventory.Count > 0 ? 53910 : 0
                        });

                __instance.discreteColorPickerCC = discreteColorPickerCC;
                __instance.populateClickableComponentList();
            }

            if (config.ShowSearchBar)
            {
                var padding = MenuModel.GetPadding(__instance);
                __instance.yPositionOnScreen -= padding;
                __instance.height += padding;
                if (__instance.chestColorPicker != null)
                    __instance.chestColorPicker.yPositionOnScreen -= padding;
            }

            if (Config.ExpandInventoryMenu)
            {
                var offset = MenuModel.GetOffset(__instance);
                __instance.height += offset;
                __instance.inventory.movePosition(0, offset);
                __instance.okButton.bounds.Y += offset;
                __instance.trashCan.bounds.Y += offset;
                __instance.dropItemInvisibleButton.bounds.Y += offset;

                if (offset < 0)
                {
                    if (__instance.colorPickerToggleButton != null)
                        __instance.colorPickerToggleButton.bounds.Y += offset;
                    __instance.fillStacksButton.bounds.Y += offset;
                    __instance.organizeButton.bounds.Y += offset;
                }
            }

            __instance.SetupBorderNeighbors();
        }

        /// <summary>Patch UI elements for ItemGrabMenu.</summary>
        private static IEnumerable<CodeInstruction> DrawTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Method(typeof(SpriteBatch), nameof(SpriteBatch.Draw)))
                )
                .Log("Adding DrawUnderlay method to ItemGrabMenu.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_1));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MenuView), nameof(MenuView.DrawUnderlay))));
                });

            // Offset backpack icon
            if (Config.ExpandInventoryMenu)
                patternPatches
                    .Find(
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu)))
                    )
                    .Find(
                        new CodeInstruction(OpCodes.Ldfld, IClickableMenuYPositionOnScreen)
                    )
                    .Log("Adding Offset to yPositionOnScreen for Backpack sprite.")
                    .Patch(OffsetPatch(MenuOffset, OpCodes.Add))
                    .Repeat(3);

            // Add top padding
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, IClickableMenuYPositionOnScreen),
                    new CodeInstruction(OpCodes.Ldsfld, IClickableMenuBorderWidth),
                    new CodeInstruction(OpCodes.Sub),
                    new CodeInstruction(OpCodes.Ldsfld, IClickableMenuSpaceToClearTopBorder),
                    new CodeInstruction(OpCodes.Sub)
                )
                .Log("Adding top padding offset to drawDialogueBox.y.")
                .Patch(OffsetPatch(MenuPadding, OpCodes.Sub));

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, IClickableMenuHeight),
                    new CodeInstruction(OpCodes.Ldsfld, IClickableMenuSpaceToClearTopBorder),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, IClickableMenuBorderWidth),
                    new CodeInstruction(OpCodes.Ldc_I4_2),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Adding top padding offset to drawDialogueBox.height.")
                .Patch(OffsetPatch(MenuPadding, OpCodes.Add));

            // Draw arrows under hover text
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.junimoNoteIcon))),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), new[] {typeof(SpriteBatch)})),
                    new CodeInstruction(OpCodes.Ldarg_0)
                )
                .Log("Adding DrawOverlay method to ItemGrabMenu.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_1));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MenuView), nameof(MenuView.DrawOverlay))));
                });

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(DrawTranspiler)}", LogLevel.Warn);
        }
    }
}