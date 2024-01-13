namespace StardewMods.SpritePatcher.Framework.Services.Patches.Buildings;

using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Buildings;

/// <summary>Patches for buildings drawn in the game.</summary>
internal sealed class PatchBuildings : BasePatches
{
    /// <summary>Initializes a new instance of the <see cref="PatchBuildings" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="textureBuilder">Dependency used for building the texture.</param>
    public PatchBuildings(ILog log, IManifest manifest, IPatchManager patchManager, TextureBuilder textureBuilder)
        : base(log, manifest, textureBuilder) =>
        patchManager.Add(
            this.ModId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.draw)),
                AccessTools.DeclaredMethod(typeof(PatchBuildings), nameof(PatchBuildings.Draw_Transpiler)),
                PatchType.Transpiler));

    private static IEnumerable<CodeInstruction> Draw_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(
                AccessTools.DeclaredMethod(
                    typeof(SpriteBatch),
                    nameof(SpriteBatch.Draw),
                    [
                        typeof(Texture2D),
                        typeof(Vector2),
                        typeof(Rectangle),
                        typeof(Color),
                        typeof(float),
                        typeof(Vector2),
                        typeof(float),
                        typeof(SpriteEffects),
                        typeof(float),
                    ])))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                yield return CodeInstruction.Call(typeof(BasePatches), nameof(BasePatches.Draw));
            }
            else
            {
                yield return instruction;
            }
        }
    }
}