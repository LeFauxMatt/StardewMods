namespace StardewMods.SpritePatcher.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models.Events;
using StardewMods.SpritePatcher.Framework.Services.Transient;

/// <summary>Represents a factory that manages the creation and retrieval of ManagedObject instances.</summary>
internal sealed class SpriteFactory : BaseService
{
    private readonly CodeManager codeManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly ISpriteSheetManager spriteSheetManager;
    private readonly ConditionalWeakTable<object, Sprite> trackedSprites = new();

    /// <summary>Initializes a new instance of the <see cref="SpriteFactory" /> class.</summary>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="spriteSheetManager">Dependency used for managing textures.</param>
    public SpriteFactory(
        CodeManager codeManager,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        ISpriteSheetManager spriteSheetManager)
        : base(log, manifest)
    {
        this.codeManager = codeManager;
        this.gameContentHelper = gameContentHelper;
        this.spriteSheetManager = spriteSheetManager;
        eventSubscriber.Subscribe<PatchesChangedEventArgs>(this.OnPatchesChanged);
    }

    /// <summary>Gets the existing ManagedObject associated with the specified entity, or adds a new one if it does not exist.</summary>
    /// <param name="entity">The entity for which to get or add the ManagedObject.</param>
    /// <returns>The existing ManagedObject if found, or a newly created one if not found.</returns>
    public Sprite GetOrAdd(object entity)
    {
        if (this.trackedSprites.TryGetValue(entity, out var sprite))
        {
            return sprite;
        }

        sprite = new Sprite(entity, this.codeManager, this.gameContentHelper, this.Log, this.spriteSheetManager);
        this.trackedSprites.Add(entity, sprite);
        return sprite;
    }

    private void OnPatchesChanged(PatchesChangedEventArgs e)
    {
        foreach (var (_, cachedObject) in this.trackedSprites)
        {
            cachedObject.ClearCache(e.ChangedTargets);
        }
    }
}