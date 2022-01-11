namespace MoreChests;

using System.Collections.Generic;
using Common.Integrations.MoreChests;
using Services;
using StardewModdingAPI;

public class MoreChestsApi : IMoreChestsApi
{
    private readonly ServiceLocator _serviceLocator;

    public MoreChestsApi(ModEntry mod)
    {
        this._serviceLocator = mod.ServiceLocator;
    }

    public bool LoadContentPack(IManifest manifest, string path)
    {
        var contentPack = this._serviceLocator.Helper.ContentPacks.CreateTemporary(
            path,
            manifest.UniqueID,
            manifest.Name,
            manifest.Description,
            manifest.Author,
            manifest.Version);

        return this.LoadContentPack(contentPack);
    }

    public bool LoadContentPack(IContentPack contentPack)
    {
        var contentPackLoader = this._serviceLocator.GetInstance<ContentPackLoader>();
        return contentPackLoader.LoadContentPack(contentPack);
    }

    public IEnumerable<string> GetAllChests()
    {
        var chestManager = this._serviceLocator.GetInstance<CustomChestManager>();
        return chestManager.GetAllChests();
    }
}