using System.Collections.Generic;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.Patches
{
    internal class MenuWithInventoryPatch : HarmonyPatch
    {
        internal MenuWithInventoryPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.ExpandInventoryMenu)
            {
                harmony.Patch(AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), new[] {typeof(SpriteBatch), T.Bool, T.Bool, T.Int, T.Int, T.Int}),
                    transpiler: new HarmonyMethod(GetType(), nameof(MenuWithInventory_draw)));
            }
        }
        static IEnumerable<CodeInstruction> MenuWithInventory_draw(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            patternPatches
                .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                    OC.Add,
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                    OC.Add,
                    OC.Ldc_I4_S,
                    OC.Add)
                .Log("Adding Offset to drawDialogueBox.y.")
                .Patch(AddOffsetPatch);
            
            patternPatches
                .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.height)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                    OC.Add,
                    IL.Ldc_I4(192),
                    OC.Add)
                .Log("Subtracting Y-Offset from drawDialogueBox.height")
                .Patch(AddOffsetPatch);

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(MenuWithInventory_draw)}", LogLevel.Warn);
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