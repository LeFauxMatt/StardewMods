using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    internal class ItemGrabMenuPatch : HarmonyPatch
    {
        private readonly Type _type = typeof(ItemGrabMenu);
        private static IReflectionHelper _reflection;

        internal ItemGrabMenuPatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection)
            : base(monitor, config)
        {
            _reflection = reflection;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowModdedCapacity)
            {
                harmony.Patch(AccessTools.Constructor(_type, new[] {typeof(IList<Item>), T.Bool, T.Bool, typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), T.String, typeof(ItemGrabMenu.behaviorOnItemSelect), T.Bool, T.Bool, T.Bool, T.Bool, T.Bool, T.Int, typeof(Item), T.Int, T.Object }),
                    transpiler: new HarmonyMethod(GetType(), nameof(CapacityPatches)));
            }
            
            if (Config.AllowModdedCapacity && Config.ExpandInventoryMenu || Config.ShowSearchBar)
            {
                harmony.Patch(AccessTools.Constructor(_type, new[] {typeof(IList<Item>), T.Bool, T.Bool, typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), T.String, typeof(ItemGrabMenu.behaviorOnItemSelect), T.Bool, T.Bool, T.Bool, T.Bool, T.Bool, T.Int, typeof(Item), T.Int, T.Object }),
                    postfix: new HarmonyMethod(GetType(), nameof(ConstructorPostfix)));
            }
            
            if (Config.ExpandInventoryMenu || Config.ShowOverlayArrows || Config.ShowTabs || Config.ShowSearchBar)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(ItemGrabMenu.draw), new []{typeof(SpriteBatch)}),
                    transpiler: new HarmonyMethod(GetType(), nameof(DrawPatches)));
            }
        }

        /// <summary>Loads default chest InventoryMenu when storage has modded capacity.</summary>
        static IEnumerable<CodeInstruction> CapacityPatches(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);
            
            patternPatches
                .Find(IL.Isinst(typeof(Chest)), IL.Callvirt(typeof(Chest), nameof(Chest.GetActualCapacity)), OC.Ldc_I4_S, OC.Beq)
                .Log("Changing jump condition to Bge 12.")
                .Patch(JumpCapacityPatch);

            patternPatches
                .Find(IL.Newobj(typeof(InventoryMenu), T.Int, T.Int, T.Bool, typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), T.Int, T.Int, T.Int, T.Int, T.Bool),
                    IL.Stfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)))
                .Find(OC.Ldc_I4_M1)
                .Log("Overriding default values for capacity and rows.")
                .Patch(CapacityRowsPatch)
                .Skip(1);

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(CapacityPatches)}", LogLevel.Warn);
        }

        static void ConstructorPostfix(ItemGrabMenu __instance)
        {
            var sourceItemReflected = _reflection.GetField<Item>(__instance, "sourceItem");
            var sourceItem = sourceItemReflected.GetValue();
            if (__instance.context is not Chest chest)
                return;
            
            var config = ExpandedStorage.GetConfig(chest);
            if (config == null)
                return;

            if (chest.SpecialChestType != Chest.SpecialChestTypes.None)
            {
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
                {
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
                }

                __instance.discreteColorPickerCC = discreteColorPickerCC;

                __instance.populateClickableComponentList();
            }

            if (Config.ShowSearchBar && config.ShowSearchBar)
            {
                var padding = ExpandedMenu.Padding(__instance);
                __instance.yPositionOnScreen -= padding;
                __instance.height += padding;
                if (__instance.chestColorPicker != null)
                    __instance.chestColorPicker.yPositionOnScreen -= padding;
            }

            if (Config.AllowModdedCapacity && Config.ExpandInventoryMenu)
            {
                var offset = ExpandedMenu.Offset(__instance);
                __instance.height += offset;
                __instance.inventory.movePosition(0, offset);
                __instance.okButton.bounds.Y += offset;
                __instance.trashCan.bounds.Y += offset;
                __instance.dropItemInvisibleButton.bounds.Y += offset;
            }
            
            if (Game1.options.SnappyMenus)
                __instance.snapToDefaultClickableComponent();
            __instance.SetupBorderNeighbors();
        }

        /// <summary>Patch UI elements for ItemGrabMenu.</summary>
        static IEnumerable<CodeInstruction> DrawPatches(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            if (Config.ShowTabs)
            {
                patternPatches
                    .Find(IL.Callvirt(typeof(SpriteBatch), nameof(SpriteBatch.Draw)))
                    .Log("Adding Overlay DrawUnder method to ItemGrabMenu.")
                    .Patch(OverlayPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.DrawUnder)));
            }

            // Offset backpack icon
            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu)))
                    .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)))
                    .Log("Adding Offset to yPositionOnScreen for Backpack sprite.")
                    .Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset)))
                    .Repeat(3);
            }

            // Add top padding
            if (Config.ShowSearchBar)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)),
                        IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)),
                        IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                        OC.Sub,
                        IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                        OC.Sub)
                    .Log("Adding top padding offset to drawDialogueBox.y.")
                    .Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Padding), Operation.Sub));
                
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)),
                        IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.height)),
                        IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                        OC.Add,
                        IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                        OC.Ldc_I4_2,
                        OC.Mul,
                        OC.Add)
                    .Log("Adding top padding offset to drawDialogueBox.height.")
                    .Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Padding)));
            }
            
            // Draw arrows under hover text
            if (Config.ShowOverlayArrows || Config.ShowSearchBar || Config.ShowTabs)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeButton)),
                        OC.Ldarg_1,
                        IL.Callvirt(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), typeof(SpriteBatch)))
                    .Log("Adding Overlay Draw method to ItemGrabMenu.")
                    .Patch(OverlayPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Draw)));
            }
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(DrawPatches)}", LogLevel.Warn);
        }

        /// <summary>Replaces jump condition for Inventory Menu to >= 12</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void JumpCapacityPatch(LinkedList<CodeInstruction> instructions)
        {
            var instruction = instructions.Last.Value;
            instructions.RemoveLast();
            instructions.RemoveLast();
            instructions.AddLast(IL.Ldc_I4_S((byte) 12));
            instructions.AddLast(IL.Bge(instruction.operand));
        }
        
        /// <summary>Replaced capacity and rows to ExpandedMenu.Capacity and ExpandedMenu.Rows</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void CapacityRowsPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.RemoveLast();
            instructions.AddLast(IL.Ldarg_S((byte) 16));
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Capacity), typeof(object)));
            instructions.AddLast(IL.Ldarg_S((byte) 16));
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Rows), typeof(object)));
        }
        
        /// <summary>Adds a call to a draw function accepting SpriteBatch</summary>
        /// <param name="type">Class which the function belongs to</param>
        /// <param name="method">Method name of the draw function</param>
        private static Action<LinkedList<CodeInstruction>> OverlayPatch(Type type, string method) =>
            instructions =>
            {
                instructions.AddLast(OC.Ldarg_1);
                instructions.AddLast(IL.Call(type, method, typeof(SpriteBatch)));
            };

        private enum Operation
        {
            Add,
            Sub
        }
        
        /// <summary>Adds a value to the end of the stack</summary>
        /// <param name="type">Class which the function belongs to</param>
        /// <param name="method">Method name of the draw function</param>
        /// <param name="operation">Whether to add or subtract the value.</param>
        private static Action<LinkedList<CodeInstruction>> AddOffsetPatch(Type type, string method, Operation operation = Operation.Add) =>
            instructions =>
            {
                instructions.AddLast(OC.Ldarg_0);
                instructions.AddLast(IL.Call(type, method, typeof(MenuWithInventory)));
                instructions.AddLast(operation == Operation.Sub ? OC.Sub : OC.Add);
            };
    }
}