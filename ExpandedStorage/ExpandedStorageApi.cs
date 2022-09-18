namespace StardewMods.ExpandedStorage;

using System.Collections.Generic;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Models;

/// <inheritdoc />
public sealed class ExpandedStorageApi : IExpandedStorageApi
{
    private readonly IContentPackHelper _contentPackHelper;
    private readonly IDictionary<string, IManagedStorage> _storages;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpandedStorageApi" /> class.
    /// </summary>
    /// <param name="contentPackHelper">API for managing content packs.</param>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    internal ExpandedStorageApi(IContentPackHelper contentPackHelper, IDictionary<string, IManagedStorage> storages)
    {
        this._contentPackHelper = contentPackHelper;
        this._storages = storages;
    }

    /// <inheritdoc />
    public bool LoadContentPack(IManifest manifest, string path)
    {
        return this.LoadContentPack(
            this._contentPackHelper.CreateTemporary(
                path,
                manifest.UniqueID,
                manifest.Name,
                manifest.Description,
                manifest.Author,
                manifest.Version));
    }

    /// <inheritdoc />
    public bool LoadContentPack(IContentPack contentPack)
    {
        Log.Info($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}");

        var storages = contentPack.ReadJsonFile<IDictionary<string, ICustomStorage>>("expanded-storage.json");
        if (storages is null)
        {
            Log.Warn($"Nothing to load from {contentPack.Manifest.Name}");
            return false;
        }

        var loadedAny = false;
        foreach (var (name, storage) in storages)
        {
            var managedStorage = new ManagedStorage(name, contentPack, storage);
            if (this.RegisterStorage(name, managedStorage))
            {
                loadedAny = true;
            }
        }

        if (!loadedAny)
        {
            Log.Warn($"Nothing to load from {contentPack.Manifest.Name}");
        }

        return loadedAny;
    }

    /// <inheritdoc />
    public bool RegisterStorage(string name, IManagedStorage storage)
    {
        if (this._storages.ContainsKey(name))
        {
            Log.Warn($"A storage has already been loaded with the name {name}");
            return false;
        }

        this._storages.Add(name, storage);
        return true;
    }
}