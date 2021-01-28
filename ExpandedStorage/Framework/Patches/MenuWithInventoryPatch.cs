using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.HarmonyPatches;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    internal class MenuWithInventoryPatch : HarmonyPatch
    {
        private readonly Type _type = typeof(MenuWithInventory);
        internal MenuWithInventoryPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (Config.AllowModdedCapacity && Config.ExpandInventoryMenu || Config.ShowSearchBar)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(MenuWithInventory.draw), new[] {typeof(SpriteBatch), T.Bool, T.Bool, T.Int, T.Int, T.Int}),
                    transpiler: new HarmonyMethod(GetType(), nameof(DrawPatches)));
            }
        }
        static IEnumerable<CodeInstruction> DrawPatches(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Monitor);

            var patch = patternPatches
                .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                    OC.Add,
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                    OC.Add,
                    OC.Ldc_I4_S,
                    OC.Add)
                .Log("Adding Offset to drawDialogueBox.y.");

            if (Config.AllowModdedCapacity)
                patch.Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset)));
            if (Config.ShowSearchBar)
                patch.Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Padding)));

            patch = patternPatches
                .Find(IL.Ldfld(typeof(IClickableMenu), nameof(IClickableMenu.height)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth)),
                    IL.Ldsfld(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder)),
                    OC.Add,
                    IL.Ldc_I4(192),
                    OC.Add)
                .Log("Subtracting Y-Offset from drawDialogueBox.height");
            
            if (Config.AllowModdedCapacity)
                patch.Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Offset)));
            if (Config.ShowSearchBar)
                patch.Patch(AddOffsetPatch(typeof(ExpandedMenu), nameof(ExpandedMenu.Padding)));

            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {nameof(DrawPatches)}", LogLevel.Warn);
        }

        private enum Operation
        {
            Add,
            Sub
        }
        
        /// <summary>Adds a value to the end of the stack</summary>
        /// <param name="type">Class which the function belongs to</param>
        /// <param name="method">Method name of the draw function</param>
        /// <param name="operation">Whether to add or subtract the value.</param>
        private static Action<LinkedList<CodeInstruction>> AddOffsetPatch(Type type, string method, Operation operation = Operation.Add) =>
            instructions =>
            {
                instructions.AddLast(OC.Ldarg_0);
                instructions.AddLast(IL.Call(type, method, typeof(MenuWithInventory)));
                instructions.AddLast(operation == Operation.Sub ? OC.Sub : OC.Add);
            };
    }
}