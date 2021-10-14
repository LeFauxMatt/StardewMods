namespace MoreChests
{
    using System.Collections.Generic;
    using Common.Integrations.MoreChests;
    using Common.Services;
    using Services;
    using StardewModdingAPI;

    public class MoreChestsAPI : IMoreChestsAPI
    {
        private readonly ServiceManager _serviceManager;

        public MoreChestsAPI(ModEntry mod)
        {
            this._serviceManager = mod.ServiceManager;
        }

        public bool LoadContentPack(IManifest manifest, string path)
        {
            var contentPack = this._serviceManager.Helper.ContentPacks.CreateTemporary(
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
            var contentPackLoader = this._serviceManager.GetByType<ContentPackLoader>();
            return contentPackLoader.LoadContentPack(contentPack);
        }

        public IEnumerable<string> GetAllChests()
        {
            var chestManager = this._serviceManager.GetByType<CustomChestManager>();
            return chestManager.GetAllChests();
        }
    }
}