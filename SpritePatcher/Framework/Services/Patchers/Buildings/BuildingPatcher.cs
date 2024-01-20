namespace StardewMods.SpritePatcher.Framework.Services.Patchers.Buildings;

using HarmonyLib;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;
using StardewValley.Buildings;

/// <summary>Patches for <see cref="Building" /> draw methods.</summary>
internal sealed class BuildingPatcher : BasePatcher
{
    /// <summary>Initializes a new instance of the <see cref="BuildingPatcher" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="managedObjectFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public BuildingPatcher(
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
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.draw)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.drawBackground)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawBackground)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.drawInConstruction)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawInConstruction)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.drawInMenu)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawInMenu)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Building), nameof(Building.drawShadow)),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.DrawShadow)),
                PatchType.Transpiler));

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedBuilding;
}