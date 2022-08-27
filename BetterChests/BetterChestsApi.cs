namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public class BetterChestsApi : IBetterChestsApi
{
    private readonly IStorageData _default;
    private readonly Dictionary<Func<object, bool>, IStorageData> _storageTypes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterChestsApi" /> class.
    /// </summary>
    /// <param name="storageTypes">A dictionary of all registered storage types.</param>
    /// <param name="defaultChest">Default data for any storage.</param>
    public BetterChestsApi(Dictionary<Func<object, bool>, IStorageData> storageTypes, IStorageData defaultChest)
    {
        this._storageTypes = storageTypes;
        this._default = defaultChest;
    }

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, IStorageData storage)
    {
        Config.SetupSpecificConfig(manifest, storage);
    }

    /// <inheritdoc />
    public void RegisterChest(Func<object, bool> predicate, IStorageData storage)
    {
        this._storageTypes[predicate] = new StorageNodeData(storage, this._default);
    }
}