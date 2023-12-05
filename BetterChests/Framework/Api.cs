namespace StardewMods.BetterChests.Framework;

using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public sealed class Api : IBetterChestsApi
{
    /// <inheritdoc />
    public event EventHandler<IStorageTypeRequestedEventArgs>? StorageTypeRequested
    {
        add => StorageService.StorageTypeRequested += value;
        remove => StorageService.StorageTypeRequested -= value;
    }

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, IStorageData storage) =>
        ConfigService.SetupSpecificConfig(manifest, storage);
}
