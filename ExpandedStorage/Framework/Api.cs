namespace StardewMods.ExpandedStorage.Framework;

using System.Collections.Generic;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Models;

/// <inheritdoc />
public sealed class Api : IExpandedStorageApi
{
    private readonly IModHelper helper;
    private readonly IDictionary<string, LegacyAsset> legacyAssets;
    private readonly IDictionary<string, CachedStorage> storageCache;
    private readonly IDictionary<string, ICustomStorage> storages;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Api" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    /// <param name="storageCache">Cached storage textures and attributes.</param>
    /// <param name="legacyAssets">Textures for legacy Expanded Storage content packs.</param>
    internal Api(
        IModHelper helper,
        IDictionary<string, ICustomStorage> storages,
        IDictionary<string, CachedStorage> storageCache,
        IDictionary<string, LegacyAsset> legacyAssets)
    {
        this.helper = helper;
        this.storages = storages;
        this.storageCache = storageCache;
        this.legacyAssets = legacyAssets;
    }

    /// <inheritdoc />
    public bool LoadContentPack(IManifest manifest, string path)
    {
        var contentPack = this.helper.ContentPacks.CreateTemporary(
            path,
            manifest.UniqueID,
            manifest.Name,
            manifest.Description,
            manifest.Author,
            manifest.Version);

        return this.LoadContentPack(contentPack);
    }

    /// <inheritdoc />
    public bool LoadContentPack(IContentPack contentPack)
    {
        Logger.Info($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
        var storages = contentPack.ReadJsonFile<IDictionary<string, LegacyStorageData>>("expanded-storage.json");
        if (storages is null)
        {
            Logger.Warn($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
            return false;
        }

        var loadedAny = false;
        foreach (var (id, legacyStorage) in storages)
        {
            if (this.storages.ContainsKey(id))
            {
                Logger.Warn($"A storage has already been loaded with the id {id}");
                return false;
            }

            var storage = legacyStorage.AsCustomStorage;

            // Get Texture Path
            var path = contentPack.HasFile(storage.Image)
                ? storage.Image
                : contentPack.HasFile($"assets/{storage.Image}")
                    ? $"assets/{storage.Image}"
                    : string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            this.legacyAssets.Add(id, new(id, contentPack, path));
            storage.DisplayName = contentPack.Translation.Get($"big-craftable.{id}.name");
            storage.Description = contentPack.Translation.Get($"big-craftable.{id}.description");
            storage.Image = $"ExpandedStorage/SpriteSheets/{id}";
            if (!this.RegisterStorage(id, storage))
            {
                continue;
            }

            // Get Shop Entry in DGA Format
            loadedAny = true;
        }

        return loadedAny;
    }

    /// <inheritdoc />
    public bool RegisterStorage(string id, ICustomStorage storage)
    {
        if (this.storages.ContainsKey(id))
        {
            Logger.Warn($"A storage has already been loaded with the id {id}");
            return false;
        }

        this.storages.Add(id, storage);
        this.storageCache.Add(storage.Image, new(storage));
        return true;
    }
}
