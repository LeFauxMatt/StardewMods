namespace StardewMods.Common.Integrations.BetterChests;

/// <summary>API for Better Chests.</summary>
public interface IBetterChestsApi
{
    /// <summary>Raised when storage data is requested for a storage type.</summary>
    public event EventHandler<IStorageTypeRequestedEventArgs> StorageTypeRequested;

    /// <summary>Adds all applicable config options to an existing GMCM for this storage data.</summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="storage">The storage to configure for.</param>
    public void AddConfigOptions(IManifest manifest, IStorageData storage);
}
