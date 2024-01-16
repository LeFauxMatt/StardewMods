namespace StardewMods.SpritePatcher.Framework.Services.Factory;

using System.Runtime.CompilerServices;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Models.Events;
using StardewMods.SpritePatcher.Framework.Services.Transient;

/// <summary>Represents a factory that manages the creation and retrieval of ManagedObject instances.</summary>
internal sealed class ManagedObjectFactory : BaseService
{
    private readonly CodeManager codeManager;
    private readonly ConditionalWeakTable<IHaveModData, ManagedObject> cachedObjects = new();

    /// <summary>Initializes a new instance of the <see cref="ManagedObjectFactory" /> class.</summary>
    /// <param name="codeManager">Dependency used for managing icons.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ManagedObjectFactory(CodeManager codeManager, IEventSubscriber eventSubscriber, ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.codeManager = codeManager;
        eventSubscriber.Subscribe<PatchesChangedEventArgs>(this.OnPatchesChanged);
    }

    /// <summary>Gets the existing ManagedObject associated with the specified entity, or adds a new one if it does not exist.</summary>
    /// <param name="entity">The entity for which to get or add the ManagedObject.</param>
    /// <returns>The existing ManagedObject if found, or a newly created one if not found.</returns>
    public ManagedObject GetOrAdd(IHaveModData entity)
    {
        if (this.cachedObjects.TryGetValue(entity, out var managedObject))
        {
            return managedObject;
        }

        managedObject = new ManagedObject(entity, this.codeManager);
        this.cachedObjects.Add(entity, managedObject);
        return managedObject;
    }

    private void OnPatchesChanged(PatchesChangedEventArgs e)
    {
        foreach (var (_, cachedObject) in this.cachedObjects)
        {
            cachedObject.ClearCache(e.ChangedTargets);
        }
    }
}