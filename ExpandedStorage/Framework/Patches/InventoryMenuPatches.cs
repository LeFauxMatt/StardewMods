using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Common;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.Patches
{
    internal class InventoryMenuPatches
    {
        private static IMonitor _monitor;

        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;

            if (!config.AllowModdedCapacity)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.leftClick),
                    new[]
                    {
                        typeof(int),
                        typeof(int),
                        typeof(Item),
                        typeof(bool)
                    }),
                transpiler: new HarmonyMethod(typeof(InventoryMenuPatches), nameof(InventoryMenu_leftClick)));
            
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.rightClick),
                    new[]
                    {
                        typeof(int),
                        typeof(int),
                        typeof(Item),
                        typeof(bool),
                        typeof(bool)
                    }),
                transpiler: new HarmonyMethod(typeof(InventoryMenuPatches), nameof(InventoryMenu_rightClick)));
            
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.hover),
                    new[]
                    {
                        typeof(int),
                        typeof(int),
                        typeof(Item)
                    }),
                transpiler: new HarmonyMethod(typeof(InventoryMenuPatches), nameof(InventoryMenu_hover)));
            
            harmony.Patch(
                original: AccessTools.Method(
                    typeof(InventoryMenu),
                    nameof(InventoryMenu.draw),
                    new[]
                    {
                        typeof(SpriteBatch),
                        typeof(int),
                        typeof(int),
                        typeof(int)
                    }),
                transpiler: new HarmonyMethod(typeof(InventoryMenuPatches), nameof(InventoryMenu_draw)));
        }

        static IEnumerable<CodeInstruction> InventoryMenu_leftClick(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            patternPatches
                .Find(new[]
                {
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.name))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Convert), nameof(Convert.ToInt32), new[] {typeof(string)}))
                })
                .Log("Offset InventoryMenu.leftClick slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)})),
                    new CodeInstruction(OpCodes.Add)
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_leftClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_rightClick(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            patternPatches
                .Find(new[]
                {
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.name))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Convert), nameof(Convert.ToInt32), new[] {typeof(string)}))
                })
                .Log("Offset InventoryMenu.rightClick slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)})),
                    new CodeInstruction(OpCodes.Add)
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_hover(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            patternPatches
                .Find(new[]
                {
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(ClickableComponent), nameof(ClickableComponent.name))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(Convert), nameof(Convert.ToInt32), new[] {typeof(string)}))
                })
                .Log("Offset InventoryMenu.hover slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)})),
                    new CodeInstruction(OpCodes.Add)
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_hover)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_draw(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            patternPatches
                .Find(new[]
                {
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
                    new CodeInstruction(OpCodes.Callvirt,
                        AccessTools.Property(typeof(ICollection<Item>), nameof(ICollection<Item>.Count)).GetGetMethod())
                })
                .Log("Offset InventoryMenu.draw Count by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)})),
                    new CodeInstruction(OpCodes.Sub)
                });

            patternPatches
                .Find(new[]
                {
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
                    new CodeInstruction(OpCodes.Ldloc_S)
                })
                .Log("Offset InventoryMenu.draw slots by scrolled amount.")
                .Patch(instruction => new[]
                {
                    instruction,
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)})),
                    new CodeInstruction(OpCodes.Add)
                })
                .Repeat(-1);
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(InventoryMenu_draw)}", LogLevel.Warn);
        }
    }
}