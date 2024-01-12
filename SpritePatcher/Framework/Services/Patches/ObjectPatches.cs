namespace StardewMods.SpritePatcher.Framework.Services.Patches;

using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Manages overlays for items.</summary>
internal sealed class ObjectPatches : BaseService<ObjectPatches>
{
#nullable disable
    private static ObjectPatches instance;
#nullable enable

    private readonly IPatchManager patchManager;
    private readonly TextureBuilder textureBuilder;

    /// <summary>Initializes a new instance of the <see cref="ObjectPatches" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="textureBuilder">Dependency used for building the texture.</param>
    public ObjectPatches(
        IEventSubscriber eventSubscriber,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager,
        TextureBuilder textureBuilder)
        : base(log, manifest)
    {
        // Init
        ObjectPatches.instance = this;
        this.patchManager = patchManager;
        this.textureBuilder = textureBuilder;

        // Patches
        patchManager.Add(
            this.Id,
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(SObject),
                    nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(ObjectPatches), nameof(ObjectPatches.Draw_Transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(SObject),
                    nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(ObjectPatches), nameof(ObjectPatches.Draw_Transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(ObjectPatches), nameof(ObjectPatches.DrawInMenu_Transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawWhenHeld)),
                AccessTools.DeclaredMethod(typeof(ObjectPatches), nameof(ObjectPatches.DrawWhenHeld_Transpiler)),
                PatchType.Transpiler));

        eventSubscriber.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

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
                yield return CodeInstruction.Call(typeof(ObjectPatches), nameof(ObjectPatches.Draw));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static IEnumerable<CodeInstruction> DrawInMenu_Transpiler(IEnumerable<CodeInstruction> instructions)
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
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                yield return CodeInstruction.Call(typeof(ObjectPatches), nameof(ObjectPatches.Draw));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static IEnumerable<CodeInstruction> DrawWhenHeld_Transpiler(IEnumerable<CodeInstruction> instructions)
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
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return CodeInstruction.Call(typeof(ObjectPatches), nameof(ObjectPatches.Draw));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void Draw(
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
        SObject obj,
        DrawMethod drawMethod)
    {
        var sourceRect = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
        if (!ObjectPatches.instance.textureBuilder.TryGetTexture(
            obj,
            texture,
            sourceRect,
            drawMethod,
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

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs obj) => this.patchManager.Patch(this.Id);
}