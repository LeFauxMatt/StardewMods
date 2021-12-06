namespace BetterChests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums;
    using Common.Services;
    using Models;
    using StardewValley;

    internal class ManagedChestService : BaseService
    {
        private readonly Dictionary<string, KeyValuePair<string, string>> _managedChestTypes = new();
        private Dictionary<string, ChestConfig> ChestConfigs;
        private ChestConfig DefaultConfig;

        private ManagedChestService(ServiceManager serviceManager)
            : base("ManagedChest")
        {
            // Init
            this.AddDependency<ModConfigService>(
                service =>
                {
                    if (service is ModConfigService modConfigService)
                    {
                        this.ChestConfigs = modConfigService.ModConfig.ChestConfigs;
                        this.DefaultConfig = modConfigService.ModConfig.DefaultConfig;
                    }
                });

            this._managedChestTypes.Add("Chest", default);
            this._managedChestTypes.Add("Stone Chest", default);
            this._managedChestTypes.Add("Junimo Chest", default);
            this._managedChestTypes.Add("Mini-Fridge", default);
            this._managedChestTypes.Add("Mini-Shipping Bin", default);
            this._managedChestTypes.Add("Auto-Grabber", default);

            // Events
            
        }

        public void AddManagedChestType(string name, string key, string value)
        {
            this._managedChestTypes.Add(name, new(key, value));
        }

        public IEnumerable<string> GetManagedChestTypes()
        {
            return this._managedChestTypes.Keys;
        }

        public bool TryGetChestConfig(Item item, out ChestConfig config)
        {
            config = null;

            if (!this._managedChestTypes.TryGetValue(item.Name, out var modData))
            {
                return false;
            }

            if ((modData.Key, modData.Value) != default && item.modData.TryGetValue(modData.Key, out var value) && modData.Value != value)
            {
                return false;
            }

            if (!this.ChestConfigs.TryGetValue(item.Name, out var chestConfig))
            {
                return false;
            }

            config = this.GetActualChestConfig(chestConfig);
            return true;
        }

        private ChestConfig GetActualChestConfig(ChestConfig chestConfig)
        {
            return new()
            {
                AccessCarried = chestConfig.AccessCarried == FeatureOption.Default ? this.DefaultConfig.AccessCarried : chestConfig.AccessCarried,
                Capacity = chestConfig.Capacity == 0 ? this.DefaultConfig.Capacity : chestConfig.Capacity,
                CarryChest = chestConfig.CarryChest == FeatureOption.Default ? this.DefaultConfig.CarryChest : chestConfig.CarryChest,
                CollectItems = chestConfig.CollectItems == FeatureOption.Default ? this.DefaultConfig.CollectItems : chestConfig.CollectItems,
                CraftingRange = chestConfig.CraftingRange == FeatureOptionRange.Default ? this.DefaultConfig.CraftingRange : chestConfig.CraftingRange,
                StashingRange = chestConfig.StashingRange == FeatureOptionRange.Default ? this.DefaultConfig.StashingRange : chestConfig.StashingRange,
                FilterItems = chestConfig.FilterItems.Any() ? chestConfig.FilterItems : this.DefaultConfig.FilterItems,
            };
        }
    }
}