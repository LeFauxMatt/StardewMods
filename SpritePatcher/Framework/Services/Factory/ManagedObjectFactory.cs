namespace StardewMods.SpritePatcher.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models.Events;
using StardewMods.SpritePatcher.Framework.Services.Transient;

/// <summary>Represents a factory that manages the creation and retrieval of ManagedObject instances.</summary>
internal sealed class ManagedObjectFactory : BaseService
{
    private readonly CodeManager codeManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly ITextureManager textureManager;
    private readonly ConditionalWeakTable<object, ManagedObject> managedTextures = new();

    /// <summary>Initializes a new instance of the <see cref="ManagedObjectFactory" /> class.</summary>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="textureManager">Dependency used for managing textures.</param>
    public ManagedObjectFactory(
        CodeManager codeManager,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest,
        ITextureManager textureManager)
        : base(log, manifest)
    {
        this.codeManager = codeManager;
        this.gameContentHelper = gameContentHelper;
        this.textureManager = textureManager;
        eventSubscriber.Subscribe<PatchesChangedEventArgs>(this.OnPatchesChanged);
    }

    /// <summary>Gets the existing ManagedObject associated with the specified entity, or adds a new one if it does not exist.</summary>
    /// <param name="entity">The entity for which to get or add the ManagedObject.</param>
    /// <returns>The existing ManagedObject if found, or a newly created one if not found.</returns>
    public ManagedObject GetOrAdd(object entity)
    {
        if (this.managedTextures.TryGetValue(entity, out var managedObject))
        {
            return managedObject;
        }

        managedObject = new ManagedObject(entity, this.codeManager, this.gameContentHelper, this.textureManager);
        this.managedTextures.Add(entity, managedObject);
        return managedObject;
    }

    private void OnPatchesChanged(PatchesChangedEventArgs e)
    {
        foreach (var (_, cachedObject) in this.managedTextures)
        {
            cachedObject.ClearCache(e.ChangedTargets);
        }
    }
}