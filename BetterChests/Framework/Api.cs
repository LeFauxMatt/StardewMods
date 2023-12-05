namespace StardewMods.BetterChests.Framework;

using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public sealed class Api : IBetterChestsApi
{
    /// <inheritdoc />
    public event EventHandler<IStorageTypeRequestedEventArgs>? StorageTypeRequested
    {
        add => Storages.StorageTypeRequested += value;
        remove => Storages.StorageTypeRequested -= value;
    }

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, IStorageData storage) =>
        Config.SetupSpecificConfig(manifest, storage);
}
