namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
internal sealed class Api : IBetterChestsApi
{
    private readonly ConfigMenu configMenu;
    private readonly StorageHandler storages;

    /// <summary>
    /// Initializes a new instance of the <see cref="Api"/> class.
    /// </summary>
    /// <param name="configMenu">Dependency used for handling configs.</param>
    /// <param name="storages">Dependency for handling storages.</param>
    public Api(ConfigMenu configMenu, StorageHandler storages)
    {
        this.configMenu = configMenu;
        this.storages = storages;
    }

    /// <inheritdoc />
    public event EventHandler<IStorageTypeRequestedEventArgs>? StorageTypeRequested
    {
        add => this.storages.StorageTypeRequested += value;
        remove => this.storages.StorageTypeRequested -= value;
    }

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, IStorageData storage) =>
        this.configMenu.SetupSpecificConfig(manifest, storage);
}
