using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    internal class ItemGrabMenuPatches
    {
        private static IMonitor _monitor;
        
        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;

            if (config.AllowModdedCapacity)
            {
                harmony.Patch(
                    original: AccessTools.Constructor(typeof(ItemGrabMenu),
                        new[] {
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
                        }),
                    transpiler: new HarmonyMethod(typeof(ItemGrabMenuPatches), nameof(ItemGrabMenu_ctor)));
            }

            if (config.ShowOverlayArrows)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new []{typeof(SpriteBatch)}),
                    transpiler: new HarmonyMethod(typeof(ItemGrabMenuPatches), nameof(ItemGrabMenu_draw)));
            }
        }

        /// <summary>Loads default chest InventoryMenu when storage has modded capacity.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_ctor(
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
                    case 0 when instruction.opcode == OpCodes.Ldarg_S:
                        ++matched;
                        break;
                    case 1 when instruction.opcode == OpCodes.Ldc_I4_1:
                        ++matched;
                        break;
                    case 2 when instruction.opcode == OpCodes.Ldc_I4_1:
                        ++matched;
                        break;
                    case 3 when instruction.opcode == OpCodes.Ldc_I4_0:
                        ++matched;
                        break;
                    case 4 when instruction.opcode == OpCodes.Ldc_I4_0:
                        reset = ++matched;
                        patches++;
                        _monitor.Log("Setting inventory y-offset to 192.", LogLevel.Debug);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, 192);
                        continue;
                    case 5 when instruction.opcode == OpCodes.Isinst && instruction.operand.Equals(typeof(Chest)):
                        ++matched;
                        break;
                    case 6 when instruction.opcode == OpCodes.Callvirt &&
                                instruction.operand.Equals(AccessTools.Method(typeof(Chest),
                                    nameof(Chest.GetActualCapacity))):
                        ++matched;
                        break;
                    case 7 when instruction.opcode == OpCodes.Ldc_I4_S:
                        ++matched;
                        break;
                    case 8 when instruction.opcode == OpCodes.Beq:
                        reset = ++matched;
                        ++patches;
                        yield return new CodeInstruction(OpCodes.Bge, (Label)instruction.operand);
                        continue;
                    case 9 when instruction.opcode == OpCodes.Newobj &&
                                instruction.operand.Equals(AccessTools.Constructor(typeof(InventoryMenu), new []
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
                                })):
                        ++matched;
                        _monitor.Log("Found constructor of InventoryMenu.", LogLevel.Debug);
                        break;
                    case 10 when instruction.opcode == OpCodes.Stfld &&
                                instruction.operand.Equals(AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))):
                        reset = ++matched;
                        _monitor.Log("Found first instantiation of ItemsToGrabMenu.", LogLevel.Debug);
                        break;
                    case 11 when instruction.opcode == OpCodes.Ldc_I4_M1:
                        ++matched;
                        ++patches;
                        _monitor.Log("Change capacity from -1 to 72 and rows from 3 to 6.", LogLevel.Debug);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, 72);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_6);
                        continue;
                    case 12:
                        ++matched;
                        // Skip duplicate row arg
                        continue;
                    case 13:
                        break;
                    default:
                        matched = reset;
                        break;
                        
                }
                yield return instruction;
            }
            if (patches == 3)
                _monitor.Log($"Applied patches in {nameof(ItemGrabMenu_ctor)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(ItemGrabMenu_ctor)}", LogLevel.Warn);
        }
        
        /// <summary>Draw arrows under hover text.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_draw(
            MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var matched = 0;
            foreach (var instruction in instructions)
            {
                switch (matched)
                {
                    case 0 when instruction.opcode == OpCodes.Ldfld &&
                                instruction.operand.Equals(AccessTools.Field(typeof(ItemGrabMenu),
                                    nameof(ItemGrabMenu.organizeButton))):
                        matched = 1;
                        break;
                    case 1 when instruction.opcode == OpCodes.Ldarg_1:
                        matched = 2;
                        break;
                    case 2 when instruction.opcode == OpCodes.Callvirt &&
                                instruction.operand.Equals(AccessTools.Method(typeof(ClickableTextureComponent),
                                nameof(ClickableTextureComponent.draw),new []{ typeof(SpriteBatch) })):
                        matched = 3;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.DrawArrows),
                                new[] {typeof(SpriteBatch)}));
                        continue;
                    case 3:
                        break;
                    default:
                        matched = 0;
                        break;
                }
                yield return instruction;
            }
            if (matched == 3)
                _monitor.Log($"Applied patches in {nameof(ItemGrabMenu_draw)}", LogLevel.Debug);
            else
                _monitor.Log($"Failed to apply patches in {nameof(ItemGrabMenu_draw)}", LogLevel.Warn);
        }
    }
}