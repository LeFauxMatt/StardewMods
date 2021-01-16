using System;
using System.Collections.Generic;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.Patches
{
    internal class InventoryMenuPatch : HarmonyPatch
    {
        private readonly Type _inventoryMenuType = typeof(InventoryMenu);
        internal InventoryMenuPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (!Config.AllowModdedCapacity)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(_inventoryMenuType, nameof(InventoryMenu.leftClick),
                    new[] {typeof(int), typeof(int), typeof(Item), typeof(bool)}),
                transpiler: new HarmonyMethod(GetType(), nameof(InventoryMenu_leftClick)));
            
            harmony.Patch(
                original: AccessTools.Method(_inventoryMenuType, nameof(InventoryMenu.rightClick),
                    new[] {typeof(int), typeof(int), typeof(Item), typeof(bool), typeof(bool)}),
                transpiler: new HarmonyMethod(GetType(), nameof(InventoryMenu_rightClick)));
            
            harmony.Patch(
                original: AccessTools.Method(_inventoryMenuType, nameof(InventoryMenu.hover),
                    new[] {typeof(int), typeof(int), typeof(Item)}),
                transpiler: new HarmonyMethod(GetType(), nameof(InventoryMenu_hover)));
            
            harmony.Patch(
                original: AccessTools.Method(_inventoryMenuType, nameof(InventoryMenu.draw),
                    new[]
                    {typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)}),
                transpiler: new HarmonyMethod(GetType(), nameof(InventoryMenu_draw)));
        }

        static IEnumerable<CodeInstruction> InventoryMenu_leftClick(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(IL.Ldfld(typeof(ClickableComponent), nameof(ClickableComponent.name)),
                    IL.Call(typeof(Convert), nameof(Convert.ToInt32), T.String))
                .Log("Offset InventoryMenu.leftClick slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Skipped), typeof(InventoryMenu)),
                    OC.Add
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_leftClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_rightClick(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(IL.Ldfld(typeof(ClickableComponent), nameof(ClickableComponent.name)),
                    IL.Call(typeof(Convert), nameof(Convert.ToInt32), T.String))
                .Log("Offset InventoryMenu.rightClick slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Skipped), typeof(InventoryMenu)),
                    OC.Add
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_hover(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(IL.Ldfld(typeof(ClickableComponent), nameof(ClickableComponent.name)),
                    IL.Call(typeof(Convert), nameof(Convert.ToInt32), T.String))
                .Log("Offset InventoryMenu.hover slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Skipped), typeof(InventoryMenu)),
                    OC.Add
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_hover)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_draw(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(IL.Ldfld(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)),
                    IL.Callvirt_Get(typeof(ICollection<Item>), nameof(ICollection<Item>.Count)))
                .Log("Offset InventoryMenu.draw Count by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Skipped), typeof(InventoryMenu)),
                    OC.Sub
                });

            patternPatches
                .Find(IL.Ldfld(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)),
                    OC.Ldloc_S)
                .Log("Offset InventoryMenu.draw slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Skipped), typeof(InventoryMenu)),
                    OC.Add
                })
                .Repeat(-1);
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_draw)}", LogLevel.Warn);
        }
    }
}