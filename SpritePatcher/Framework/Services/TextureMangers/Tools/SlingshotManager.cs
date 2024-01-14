namespace StardewMods.SpritePatcher.Framework.Services.TextureMangers.Tools;

using HarmonyLib;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;
using StardewValley.Tools;

/// <summary>Patches for <see cref="Slingshot" /> draw methods.</summary>
internal sealed class SlingshotManager : BaseTextureManager
{
    /// <summary>Initializes a new instance of the <see cref="SlingshotManager" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="managedObjectFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public SlingshotManager(
        ConfigManager configManager,
        IEventSubscriber eventSubscriber,
        ILog log,
        ManagedObjectFactory managedObjectFactory,
        IManifest manifest,
        IPatchManager patchManager)
        : base(configManager, eventSubscriber, log, managedObjectFactory, manifest, patchManager) =>
        this.Patches.Add(
            this.Id,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Slingshot), nameof(Slingshot.draw)),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Slingshot), nameof(Slingshot.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.DrawInMenu)),
                PatchType.Transpiler));

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedSlingshot;
}