namespace StardewMods.BetterChests;

using System.Collections.Generic;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public class BetterChestsApi : IBetterChestsApi
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterChestsApi" /> class.
    /// </summary>
    /// <param name="storageTypes">A dictionary of all registered storage types.</param>
    public BetterChestsApi(Dictionary<KeyValuePair<string, string>, IStorageData> storageTypes)
    {
        this.StorageTypes = storageTypes;
    }

    private Dictionary<KeyValuePair<string, string>, IStorageData> StorageTypes { get; }

    /// <inheritdoc />
    public void RegisterChest(string key, string value, IStorageData storage)
    {
        this.StorageTypes[new(key, value)] = storage;
    }
}