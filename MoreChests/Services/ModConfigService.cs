namespace MoreChests.Services
{
    using System.Collections.Generic;
    using Common.Integrations.BetterChests;
    using Common.Integrations.GenericModConfigMenu;
    using Common.Services;
    using Models;
    using StardewModdingAPI;

    internal class ModConfigService : BaseService
    {
        private readonly BetterChestsIntegration _betterChests;
        private readonly IDictionary<string, ChestConfig> _configs = new Dictionary<string, ChestConfig>();
        private readonly GenericModConfigMenuIntegration _modConfig;
        private CustomChestManager _customChestManager;

        private ModConfigService(ServiceManager serviceManager)
            : base("ModConfig")
        {
            // Init
            this._modConfig = new(serviceManager.Helper.ModRegistry);
            this._betterChests = new(serviceManager.Helper.ModRegistry);

            // Dependencies
            this.AddDependency<CustomChestManager>(service => this._customChestManager = service as CustomChestManager);
        }

        public bool RegisterNew(IContentPack contentPack)
        {
            if (!this._modConfig.IsLoaded)
            {
                return false;
            }

            this._modConfig.API.RegisterModConfig(
                contentPack.Manifest,
                this.RevertToDefault,
                () => contentPack.WriteJsonFile("config.json", this._configs));

            this._modConfig.API.SetDefaultIngameOptinValue(
                contentPack.Manifest,
                true);

            return true;
        }

        public void AddChests(IContentPack contentPack, IEnumerable<KeyValuePair<string, ChestData>> chestData)
        {
            if (!this._modConfig.IsLoaded)
            {
                return;
            }

            var config = contentPack.ReadJsonFile<Dictionary<string, ChestConfig>>("config.json");
            foreach (var data in chestData)
            {
                if (!config.TryGetValue(data.Key, out var chestConfig))
                {
                    chestConfig = new(data.Value.Capacity, data.Value.EnabledFeatures);
                }

                this._configs.Add(data.Key, chestConfig);
                this._modConfig.API.RegisterPageLabel(
                    contentPack.Manifest,
                    data.Key,
                    string.Empty,
                    data.Key);
            }
        }

        private void RevertToDefault()
        {
            foreach (var config in this._configs)
            {
                if (this._customChestManager.TryGetChestData(config.Key, out var chestData))
                {
                    config.Value.Capacity = chestData.Capacity;
                    config.Value.EnabledFeatures = chestData.EnabledFeatures;
                }
            }
        }
    }
}