using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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

            if (Config.ShowOverlayArrows)
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
                    .Patch(instruction => new[]
                    {
                        IL.Ldarg_S((byte) 16),
                        IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset), typeof(object))
                    });
            }

            if (Config.AllowModdedCapacity)
            {
                patternPatches
                    .Find(IL.Isinst(typeof(Chest)), IL.Callvirt(typeof(Chest), nameof(Chest.GetActualCapacity)), OC.Ldc_I4_S, OC.Beq)
                    .Log("Changing jump condition to Bge.")
                    .Patch(instruction => new[] { IL.Bge((Label) instruction.operand) });
            }

            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Newobj(typeof(InventoryMenu), T.Int, T.Int, T.Bool, typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), T.Int, T.Int, T.Int, T.Int, T.Bool),
                        IL.Stfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)))
                    .Find(OC.Ldc_I4_M1)
                    .Log("Overriding default values for capacity and rows.")
                    .Patch(instruction => new[]
                    {
                        IL.Ldarg_S((byte) 16),
                        IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Capacity), typeof(object)),
                        IL.Ldarg_S((byte) 16),
                        IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Rows), typeof(object))
                    })
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

            // Offset backpack icon
            if (Config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu)))
                    .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)))
                    .Log("Adding Offset to yPositionOnScreen.")
                    .Patch(instruction => new[]
                    {
                        instruction,
                        OC.Ldarg_0,
                        IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset), typeof(MenuWithInventory)),
                        OC.Add
                    })
                    .Repeat(3);
            }
            
            // Draw arrows under hover text
            if (Config.ShowOverlayArrows)
            {
                patternPatches
                    .Find(IL.Ldfld(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeButton)),
                        OC.Ldarg_1,
                        IL.Callvirt(typeof(ClickableTextureComponent), nameof(ClickableTextureComponent.draw), typeof(SpriteBatch)))
                    .Log("Adding DrawArrows method to ItemGrabMenu.")
                    .Patch(instruction => new[]
                    {
                        instruction,
                        OC.Ldarg_1,
                        IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Draw), typeof(SpriteBatch))
                    });
            }
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(ItemGrabMenu_draw)}", LogLevel.Warn);
        }
    }
}