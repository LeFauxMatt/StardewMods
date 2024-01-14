namespace StardewMods.SpritePatcher.Framework.Services.TextureMangers.Items;

using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;

/// <summary>Patches for <see cref="SObject" /> draw methods.</summary>
internal sealed class ObjectManager : BaseTextureManager
{
    /// <summary>Initializes a new instance of the <see cref="ObjectManager" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="managedObjectFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ObjectManager(
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
                AccessTools.DeclaredMethod(
                    typeof(SObject),
                    nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(SObject),
                    nameof(SObject.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.DrawInMenu)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawWhenHeld)),
                AccessTools.DeclaredMethod(typeof(BaseTextureManager), nameof(BaseTextureManager.DrawWhenHeld)),
                PatchType.Transpiler));

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedObject;
}