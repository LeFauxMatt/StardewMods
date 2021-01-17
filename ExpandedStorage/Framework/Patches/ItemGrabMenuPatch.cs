using System;
using System.Collections.Generic;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    internal class ItemGrabMenuPatch : HarmonyPatch
    {
        private readonly Type _itemGrabMenuType = typeof(ItemGrabMenu);
        internal ItemGrabMenuPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowModdedCapacity || Config.ExpandInventoryMenu)
            {
                harmony.Patch(AccessTools.Constructor(_itemGrabMenuType, new[] {typeof(IList<Item>), T.Bool, T.Bool, typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), T.String, typeof(ItemGrabMenu.behaviorOnItemSelect), T.Bool, T.Bool, T.Bool, T.Bool, T.Bool, T.Int, typeof(Item), T.Int, T.Object }),
                    transpiler: new HarmonyMethod(GetType(), nameof(ItemGrabMenu_ctor)));
            }

            if (Config.ShowOverlayArrows || Config.ShowTabs)
            {
                harmony.Patch(AccessTools.Method(_itemGrabMenuType, nameof(ItemGrabMenu.draw), new []{typeof(SpriteBatch)}),
                    transpiler: new HarmonyMethod(GetType(), nameof(ItemGrabMenu_draw)));
            }
        }

        /// <summary>Loads default chest InventoryMenu when storage has modded capacity.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_ctor(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Ldarg_S((byte) 4), OC.Ldc_I4_1, OC.Ldc_I4_1, OC.Ldc_I4_0, OC.Ldc_I4_0)
                    .Log("Setting yOffset of base MenuWithInventory")
                    .Patch(BaseYOffsetPatch);
            }

            if (Config.AllowModdedCapacity)
            {
                patternPatches
                    .Find(IL.Isinst(typeof(Chest)), IL.Callvirt(typeof(Chest), nameof(Chest.GetActualCapacity)), OC.Ldc_I4_S, OC.Beq)
                    .Log("Changing jump condition to Bge 12.")
                    .Patch(JumpCapacityPatch);
            }
            
            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Newobj(typeof(InventoryMenu), T.Int, T.Int, T.Bool, typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), T.Int, T.Int, T.Int, T.Int, T.Bool),
                        IL.Stfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)))
                    .Find(OC.Ldc_I4_M1)
                    .Log("Overriding default values for capacity and rows.")
                    .Patch(CapacityRowsPatch)
                    .Skip(1);
            }

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(ItemGrabMenu_ctor)}", LogLevel.Warn);
        }
        
        /// <summary>Patch UI elements for ItemGrabMenu.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_draw(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            if (Config.ShowTabs)
            {
                patternPatches
                    .Find(IL.Callvirt(typeof(SpriteBatch), nameof(SpriteBatch.Draw)))
                    .Log("Adding Overlay DrawUnder method to ItemGrabMenu.")
                    .Patch(UnderlayPatch);
            }

            // Offset backpack icon
            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu)))
                    .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)))
                    .Log("Adding Offset to yPositionOnScreen.")
                    .Patch(AddOffsetPatch)
                    .Repeat(3);
            }
            
            // Draw arrows under hover text
            if (Config.ShowOverlayArrows)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeButton)),
                        OC.Ldarg_1,
                        IL.Callvirt(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), typeof(SpriteBatch)))
                    .Log("Adding Overlay Draw method to ItemGrabMenu.")
                    .Patch(OverlayPatch);
            }
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(ItemGrabMenu_draw)}", LogLevel.Warn);
        }
        
        /// <summary>Replaces 0 yOffset with ExpandedMenu.Offset</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void BaseYOffsetPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.RemoveLast();
            instructions.AddLast(IL.Ldarg_S((byte) 16));
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset), typeof(object)));
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
        
        /// <summary>Adds a call to ExpandedMenu.DrawUnder for Overlay</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void UnderlayPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.AddLast(OC.Ldarg_1);
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.DrawUnder), typeof(SpriteBatch)));
        }
        
        /// <summary>Adds a call to ExpandedMenu.Draw for Overlay</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void OverlayPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.AddLast(OC.Ldarg_1);
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Draw), typeof(SpriteBatch)));
        }
        
        /// <summary>Adds the value of ExpandedMenu.Offset to the stack</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void AddOffsetPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.AddLast(OC.Ldarg_0);
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset), typeof(MenuWithInventory)));
            instructions.AddLast(OC.Add);
        }
    }
}