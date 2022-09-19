namespace StardewMods.ExpandedStorage;

using System.Collections.Generic;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ExpandedStorage;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly IDictionary<string, ICustomStorage> _storages;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.
    /// </summary>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    internal ExpandedStorageApi(IDictionary<string, ICustomStorage> storages)
    {
        this._storages = storages;
    }

    /// <inheritdoc />
    public bool RegisterStorage(string id, ICustomStorage storage)
    {
        if (this._storages.ContainsKey(id))
        {
            Log.Warn($"A storage has already been loaded with the id {id}");
            return false;
        }

        this._storages.Add(id, storage);
        return true;
    }
}