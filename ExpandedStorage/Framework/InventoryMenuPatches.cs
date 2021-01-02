using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework
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

        static IEnumerable<CodeInstruction> InventoryMenu_leftClick(
            MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var matched = 0;
            foreach (var instruction in instructions)
            {
                switch (matched)
                {
                    case 0 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(
                                    typeof(ClickableComponent),
                                    nameof(ClickableComponent.name))):
                        matched = 1;
                        break;
                    case 1 when instruction.opcode == OpCodes.Call &&
                                instruction.operand.Equals(AccessTools.Method(
                                    typeof(Convert),
                                    nameof(Convert.ToInt32),
                                    new []{ typeof(string) })):
                        matched = 2;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(
                                typeof(ChestOverlay),
                                nameof(ChestOverlay.Offset),
                                new []{ typeof(InventoryMenu) }));
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    case 2:
                        break;
                    default:
                        matched = 0;
                        break;
                }
                yield return instruction;
            }
            if (matched == 2)
                _monitor.Log($"Applied patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_rightClick(
            MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var matched = 0;
            foreach (var instruction in instructions)
            {
                switch (matched)
                {
                    case 0 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(
                                    typeof(ClickableComponent),
                                    nameof(ClickableComponent.name))):
                        matched = 1;
                        break;
                    case 1 when instruction.opcode == OpCodes.Call &&
                                instruction.operand.Equals(AccessTools.Method(
                                    typeof(Convert),
                                    nameof(Convert.ToInt32),
                                    new []{ typeof(string) })):
                        matched = 2;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(
                                typeof(ChestOverlay),
                                nameof(ChestOverlay.Offset),
                                new []{ typeof(InventoryMenu) }));
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    case 2:
                        break;
                    default:
                        matched = 0;
                        break;
                }
                yield return instruction;
            }
            if (matched == 2)
                _monitor.Log($"Applied patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(InventoryMenu_rightClick)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_hover(
            MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var matched = 0;
            foreach (var instruction in instructions)
            {
                switch (matched)
                {
                    case 0 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(
                                    typeof(ClickableComponent),
                                    nameof(ClickableComponent.name))):
                        matched = 1;
                        break;
                    case 1 when instruction.opcode == OpCodes.Call &&
                                instruction.operand.Equals(AccessTools.Method(
                                    typeof(Convert),
                                    nameof(Convert.ToInt32),
                                    new []{ typeof(string) })):
                        matched = 2;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(
                                typeof(ChestOverlay),
                                nameof(ChestOverlay.Offset),
                                new []{ typeof(InventoryMenu) }));
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    case 2:
                        break;
                    default:
                        matched = 0;
                        break;
                }
                yield return instruction;
            }
            if (matched == 2)
                _monitor.Log($"Applied patches in {nameof(InventoryMenu_hover)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(InventoryMenu_hover)}", LogLevel.Warn);
        }
        
        static IEnumerable<CodeInstruction> InventoryMenu_draw(
            MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var matched = 0;
            var reset = 0;
            var patches = 0;
            foreach (var instruction in instructions)
            {
                switch (matched)
                {
                    case 0 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(
                                    typeof(InventoryMenu),
                                    nameof(InventoryMenu.actualInventory))):
                        matched = 1;
                        break;
                    case 1 when instruction.opcode == OpCodes.Callvirt &&
                                instruction.operand.Equals(AccessTools.Property(
                                    typeof(ICollection<Item>), nameof(ICollection<Item>.Count)).GetGetMethod()):
                        reset = matched = 2;
                        patches++;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(
                            typeof(ChestOverlay),
                            nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)}));
                        yield return new CodeInstruction(OpCodes.Sub);
                        continue;
                    case 2 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(
                                    typeof(InventoryMenu),
                                    nameof(InventoryMenu.actualInventory))):
                        matched = 3;
                        break;
                    case 3 when instruction.opcode == OpCodes.Ldloc_S:
                        matched = reset;
                        patches++;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(
                            typeof(ChestOverlay),
                            nameof(ChestOverlay.Offset),
                            new[] {typeof(InventoryMenu)}));
                        yield return new CodeInstruction(OpCodes.Add);
                        continue;
                    default:
                        matched = reset;
                        break;
                }
                yield return instruction;
            }
            if (patches >= 8)
                _monitor.Log($"Applied patches in {nameof(InventoryMenu_draw)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(InventoryMenu_draw)}", LogLevel.Warn);
        }
    }
}