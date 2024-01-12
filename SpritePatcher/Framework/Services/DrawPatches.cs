namespace StardewMods.SpritePatcher.Framework.Services;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Manages overlays for items.</summary>
internal sealed class DrawPatches : BaseService
{
#nullable disable
    private static DrawPatches instance;
#nullable enable

    private readonly TextureBuilder textureBuilder;

    /// <summary>Initializes a new instance of the <see cref="DrawPatches" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="textureBuilder">Dependency used for building the texture.</param>
    public DrawPatches(ILog log, IManifest manifest, IPatchManager patchManager, TextureBuilder textureBuilder)
        : base(log, manifest)
    {
        // Init
        DrawPatches.instance = this;
        this.textureBuilder = textureBuilder;

        // Patches
        patchManager.Add(
            this.ModId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(DrawPatches), nameof(DrawPatches.Object_DrawInMenu_Transpiler)),
                PatchType.Transpiler));

        patchManager.Patch(this.ModId);
    }

    private static IEnumerable<CodeInstruction> Object_DrawInMenu_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt
                && instruction.operand is MethodInfo method
                && method.Name == nameof(SpriteBatch.Draw))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(DrawPatches), nameof(DrawPatches.Object_DrawInMenu));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void Object_DrawInMenu(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth,
        SObject obj)
    {
        var sourceRect = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
        if (!DrawPatches.instance.textureBuilder.TryGetTexture(
            obj,
            texture,
            sourceRect,
            DrawMethod.Menu,
            out var newTexture))
        {
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
            return;
        }

        spriteBatch.Draw(
            newTexture,
            position,
            new Rectangle(0, 0, sourceRect.Width, sourceRect.Height),
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth);
    }
}