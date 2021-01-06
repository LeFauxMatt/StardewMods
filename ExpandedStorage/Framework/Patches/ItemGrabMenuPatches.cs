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
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    internal class ItemGrabMenuPatches
    {
        private static IMonitor _monitor;
        private static ModConfig _config;
        internal static void PatchAll(ModConfig config, IMonitor monitor, HarmonyInstance harmony)
        {
            _monitor = monitor;
            _config = config;

            if (_config.AllowModdedCapacity || _config.ExpandInventoryMenu)
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

            if (_config.ShowOverlayArrows)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new []{typeof(SpriteBatch)}),
                    transpiler: new HarmonyMethod(typeof(ItemGrabMenuPatches), nameof(ItemGrabMenu_draw)));
            }
        }

        /// <summary>Loads default chest InventoryMenu when storage has modded capacity.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_ctor(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            if (_config.AllowModdedCapacity)
            {
                patternPatches
                    .Find(new[]
                    {
                        new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
                        new CodeInstruction(OpCodes.Ldc_I4_S),
                        new CodeInstruction(OpCodes.Beq)
                    })
                    .Log("Changing jump condition to Bge.")
                    .Patch(instruction => new[] {new CodeInstruction(OpCodes.Bge, (Label) instruction.operand)});
            }

            if (_config.ExpandInventoryMenu)
            {
                patternPatches
                    .Find(new[]
                    {
                        new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(InventoryMenu), new []
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
                        })),
                        new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu)))
                    });

                patternPatches
                    .Find(new[]
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_M1)
                    })
                    .Log("Change capacity from -1 to 72 and rows from 3 to 6.")
                    .Patch(instruction => new[]
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_S, 72),
                        new CodeInstruction(OpCodes.Ldc_I4_6)
                    })
                    .Skip(1);
            }

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;

            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(ItemGrabMenu_ctor)}", LogLevel.Warn);
        }
        
        /// <summary>Draw arrows under hover text.</summary>
        static IEnumerable<CodeInstruction> ItemGrabMenu_draw(IEnumerable<CodeInstruction> instructions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var patternPatches = new PatternPatches(instructions, _monitor);

            if (_config.ShowOverlayArrows)
            {
                patternPatches
                    .Find(new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld,
                            AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeButton))),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Callvirt,
                            AccessTools.Method(typeof(ClickableTextureComponent),
                                nameof(ClickableTextureComponent.draw), new[] {typeof(SpriteBatch)}))
                    })
                    .Log("Adding DrawArrows method to ItemGrabMenu.")
                    .Patch(instruction => new[]
                    {
                        instruction,
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,
                            AccessTools.Method(typeof(ChestOverlay), nameof(ChestOverlay.DrawArrows),
                                new[] {typeof(SpriteBatch)}))
                    });
            }
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _monitor.Log($"Failed to apply all patches in {nameof(ItemGrabMenu_draw)}", LogLevel.Warn);
        }
    }
}