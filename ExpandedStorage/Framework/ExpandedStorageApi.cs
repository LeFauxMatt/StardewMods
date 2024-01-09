namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewMods.ExpandedStorage.Framework.Services;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly BaseEventManager eventManager;
    private readonly IModInfo modInfo;
    private readonly StorageManager storageManager;

    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="storageManager">Dependency for managing expanded storage chests.</param>
    internal ExpandedStorageApi(
        IEventSubscriber eventSubscriber,
        ILog log,
        IModInfo modInfo,
        StorageManager storageManager)
    {
        // Init
        this.modInfo = modInfo;
        this.storageManager = storageManager;
        this.eventManager = new BaseEventManager(log, modInfo.Manifest);

        // Events
        eventSubscriber.Subscribe<ChestCreatedEventArgs>(this.OnChestCreated);
    }

    /// <inheritdoc />
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData) =>
        this.storageManager.TryGetData(item, out storageData);

    /// <inheritdoc />
    public void Subscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public void Unsubscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnChestCreated(ChestCreatedEventArgs e) =>
        this.eventManager.Publish<IChestCreatedEventArgs, ChestCreatedEventArgs>(e);
}