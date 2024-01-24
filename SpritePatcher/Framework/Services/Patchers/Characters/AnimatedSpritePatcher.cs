namespace StardewMods.SpritePatcher.Framework.Services.Patchers.Characters;

using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;

/// <summary>Patches for <see cref="AnimatedSprite" /> draw methods.</summary>
internal sealed class AnimatedSpritePatcher : BasePatcher
{
    /// <summary>Initializes a new instance of the <see cref="AnimatedSpritePatcher" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="spriteFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public AnimatedSpritePatcher(
        ConfigManager configManager,
        IEventSubscriber eventSubscriber,
        ILog log,
        SpriteFactory spriteFactory,
        IManifest manifest,
        IModRegistry modRegistry,
        IPatchManager patchManager)
        : base(configManager, eventSubscriber, log, spriteFactory, manifest, patchManager)
    {
        this.Patches.Add(
            this.Id,
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(AnimatedSprite),
                    nameof(AnimatedSprite.draw),
                    [typeof(SpriteBatch), typeof(Vector2), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(AnimatedSprite),
                    nameof(AnimatedSprite.draw),
                    [
                        typeof(SpriteBatch),
                        typeof(Vector2),
                        typeof(float),
                        typeof(int),
                        typeof(int),
                        typeof(Color),
                        typeof(bool),
                        typeof(float),
                        typeof(float),
                        typeof(bool),
                    ]),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(AnimatedSprite),
                    nameof(AnimatedSprite.drawShadow),
                    [typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawShadow)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(AnimatedSprite),
                    nameof(AnimatedSprite.drawShadow),
                    [typeof(SpriteBatch), typeof(Vector2), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawShadow)),
                PatchType.Transpiler));

        if (!modRegistry.IsLoaded("spacechase0.SpaceCore"))
        {
            return;
        }

        var mod = modRegistry.Get("spacechase0.SpaceCore");
        var spaceCore =
            (mod?.GetType().GetProperty("Mod", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) as IMod)
            ?.GetType()
            .Assembly;

        var method1 = spaceCore
            ?.GetType("SpaceCore.Patches.AnimatedSpriteDrawExtrasPatch1")
            ?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);

        var method2 = spaceCore
            ?.GetType("SpaceCore.Patches.AnimatedSpriteDrawExtrasPatch2")
            ?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public);

        this.Patches.Add(
            this.Id,
            new SavedPatch(
                method1!,
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                method2!,
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler));
    }

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedAnimatedSprite;
}