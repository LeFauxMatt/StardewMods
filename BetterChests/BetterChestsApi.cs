namespace BetterChests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Enums;
    using Common.Integrations.BetterChests;
    using Common.Services;
    using Services;

    public class BetterChestsApi : IBetterChestsApi
    {
        private readonly ModConfigService _modConfigService;
        private readonly ManagedChestService _managedChestService;

        internal BetterChestsApi(ServiceManager serviceManager)
        {
            this._modConfigService = serviceManager.GetByType<ModConfigService>();
            this._managedChestService = serviceManager.GetByType<ManagedChestService>();
        }

        /// <inheritdoc />
        public void RegisterCustomChest(string name, string key, string value)
        {
            this._managedChestService.AddManagedChestType(name, key, value);
        }

        /// <inheritdoc />
        public void SetAccessCarried(string name, bool enabled)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.AccessCarried == FeatureOption.Default)
            {
                chestConfig.AccessCarried = enabled ? FeatureOption.Enabled : FeatureOption.Disabled;
            }
        }

        /// <inheritdoc />
        public void SetCapacity(string name, int capacity)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.Capacity == 0)
            {
                chestConfig.Capacity = capacity;
            }
        }

        /// <inheritdoc />
        public void SetCarryChest(string name, bool enabled)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.CarryChest == FeatureOption.Default)
            {
                chestConfig.CarryChest = enabled ? FeatureOption.Enabled : FeatureOption.Disabled;
            }
        }

        /// <inheritdoc />
        public void SetCollectItems(string name, bool enabled)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.CollectItems == FeatureOption.Default)
            {
                chestConfig.CollectItems = enabled ? FeatureOption.Enabled : FeatureOption.Disabled;
            }
        }

        /// <inheritdoc />
        public void SetCraftingRange(string name, string range)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.CraftingRange == FeatureOptionRange.Default && Enum.TryParse(range, out FeatureOptionRange craftingRange))
            {
                chestConfig.CraftingRange = craftingRange;
            }
        }

        /// <inheritdoc />
        public void SetStashingRange(string name, string range)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (chestConfig.StashingRange == FeatureOptionRange.Default && Enum.TryParse(range, out FeatureOptionRange stashingRange))
            {
                chestConfig.StashingRange = stashingRange;
            }
        }

        /// <inheritdoc />
        public void SetItemFilters(string name, HashSet<string> filters)
        {
            var chestConfig = this._modConfigService.GetChestConfig(name);

            if (!chestConfig.FilterItems.Any())
            {
                chestConfig.FilterItems = filters;
            }
        }
    }
}