namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Extensions;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework.Services;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly IModInfo modInfo;
    private readonly StorageManager storageManager;

    private EventHandler<IChestCreatedEventArgs>? chestCreated;

    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.</summary>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="storageManager">Dependency for managing expanded storage chests.</param>
    internal ExpandedStorageApi(IModInfo modInfo, StorageManager storageManager)
    {
        // Init
        this.modInfo = modInfo;
        this.storageManager = storageManager;

        // Events
        storageManager.ChestCreated += this.OnChestCreated;
    }

    /// <inheritdoc />
    public event EventHandler<IChestCreatedEventArgs> ChestCreated
    {
        add => this.chestCreated += value;
        remove => this.chestCreated -= value;
    }

    /// <inheritdoc />
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData) =>
        this.storageManager.TryGetData(item, out storageData);

    private void OnChestCreated(object? sender, IChestCreatedEventArgs e) => this.chestCreated?.InvokeAll(this, e);
}