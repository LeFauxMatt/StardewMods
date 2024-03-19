namespace StardewMods.SpritePatcher.Framework.Services.Patchers.Characters;

using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Services.Factory;
using StardewValley.Characters;

/// <summary>Patches for <see cref="Child" /> draw methods.</summary>
internal sealed class ChildPatcher : BasePatcher
{
    /// <summary>Initializes a new instance of the <see cref="ChildPatcher" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="spriteFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    public ChildPatcher(
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
                AccessTools.DeclaredMethod(typeof(Child), nameof(Child.draw), [typeof(SpriteBatch)]),
                AccessTools.DeclaredMethod(typeof(BasePatcher), nameof(BasePatcher.Draw)),
                PatchType.Transpiler));

    /// <inheritdoc />
    public override AllPatches Type => AllPatches.PatchedChild;
}