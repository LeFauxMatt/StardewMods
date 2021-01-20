using System;
using System.Collections.Generic;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.Patches
{
    internal class InventoryMenuPatch : HarmonyPatch
    {
        private readonly Type _type = typeof(InventoryMenu);
        internal InventoryMenuPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowModdedCapacity || Config.ShowTabs || Config.ShowSearchBar)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(InventoryMenu.draw),
                        new[]
                            {typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)}),
                    transpiler: new HarmonyMethod(GetType(), nameof(FilteredActualInventory)));
            }
        }

        static IEnumerable<CodeInstruction> FilteredActualInventory(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(OC.Ldarg_0,
                    IL.Ldfld(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory)))
                .Log("Replace actualInventory with Filtered Inventory")
                .Patch(FilteredInventoryPatch)
                .Repeat(-1);
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(FilteredActualInventory)}", LogLevel.Warn);
        }

        /// <summary>Adds the value of ExpandedMenu.Skipped to the stack</summary>
        /// <param name="instructions">List of instructions preceding patch</param>
        private static void FilteredInventoryPatch(LinkedList<CodeInstruction> instructions)
        {
            instructions.RemoveLast();
            instructions.AddLast(IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Filtered), typeof(InventoryMenu)));
        }
    }
}