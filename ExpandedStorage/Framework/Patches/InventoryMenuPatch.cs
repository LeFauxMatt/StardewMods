using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Type _type = typeof(InventoryMenu);
        internal InventoryMenuPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowModdedCapacity || Config.ShowTabs)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(InventoryMenu.leftClick),
                        new[] {typeof(int), typeof(int), typeof(Item), typeof(bool)}),
                    new HarmonyMethod(GetType(), nameof(leftClick_Prefix)));
            
                harmony.Patch(AccessTools.Method(_type, nameof(InventoryMenu.rightClick),
                        new[] {typeof(int), typeof(int), typeof(Item), typeof(bool), typeof(bool)}),
                    new HarmonyMethod(GetType(), nameof(rightClick_Prefix)));
            
                harmony.Patch(AccessTools.Method(_type, nameof(InventoryMenu.hover),
                        new[] {typeof(int), typeof(int), typeof(Item)}),
                    new HarmonyMethod(GetType(), nameof(hover_Prefix)));
                
                harmony.Patch(AccessTools.Method(_type, nameof(InventoryMenu.draw),
                        new[]
                            {typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)}),
                    transpiler: new HarmonyMethod(GetType(), nameof(FilteredActualInventory)));
            }
        }

        public static void leftClick_Prefix(InventoryMenu __instance, int x, int y, Item toPlace, bool playSound, ref Item __result)
        {
            if (!ExpandedMenu.ContextMatches(__instance))
                return;
            SlotNamesPatch(__instance);
        }
        
        public static void rightClick_Prefix(InventoryMenu __instance, int x, int y, Item toAddTo, bool playSound, bool onlyCheckToolAttachments, ref Item __result)
        {
            if (!ExpandedMenu.ContextMatches(__instance))
                return;
            SlotNamesPatch(__instance);
        }
        
        public static void hover_Prefix(InventoryMenu __instance, int x, int y, Item heldItem, ref Item __result)
        {
            if (!ExpandedMenu.ContextMatches(__instance))
                return;
            SlotNamesPatch(__instance);
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

        /// <summary>Assigns slot name taking into account filtered/skipped slots</summary>
        /// <param name="instance">The inventory menu to modify</param>
        private static void SlotNamesPatch(InventoryMenu instance)
        {
            var items = ExpandedMenu.Filtered(instance);
            for (var i = 0; i < instance.inventory.Count; i++)
            {
                var item = items.ElementAtOrDefault(i);
                instance.inventory[i].name = item != null
                    ? instance.actualInventory.IndexOf(item).ToString()
                    : instance.actualInventory.Count.ToString();
            }
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