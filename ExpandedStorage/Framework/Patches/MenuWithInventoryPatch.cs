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
                .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)))
                .Log("Adding Offset to yPositionOnScreen.")
                .Patch(instruction => new[]
                {
                    instruction,
                    OC.Ldarg_0,
                    IL.Call(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset), typeof(MenuWithInventory)),
                    OC.Add
                })
                .Repeat(-1);
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(MenuWithInventory_draw)}", LogLevel.Warn);
        }
    }
}