namespace StardewMods.SpritePatcher.Framework.Services.Patchers.Items;

using HarmonyLib;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;
using StardewValley.Objects;

/// <summary>Patches for <see cref="Wallpaper" /> draw methods.</summary>
internal sealed class WallpaperPatcher : BasePatcher
{
    /// <summary>Initializes a new instance of the <see cref="WallpaperPatcher" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="spriteFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public WallpaperPatcher(
        ConfigManager configManager,
        IEventSubscriber eventSubscriber,
        ILog log,
        SpriteFactory spriteFactory,
        IManifest manifest,
        IPatchManager patchManager)
        : base(configManager, eventSubscriber, log, spriteFactory, manifest, patchManager) =>
        this.Patches.Add(
            this.Id,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Wallpaper), nameof(Wallpaper.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawInMenu)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Wallpaper), nameof(Wallpaper.drawWhenHeld)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawWhenHeld)),
                PatchType.Transpiler));

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedWallpaper;
}