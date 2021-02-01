using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Extensions
{
    public static class FarmerExtensions
    {
        private const string ChestsAnywhereOrderKey = "Pathoschild.ChestsAnywhere/Order";
        private static IMonitor _monitor;
        internal static void Init(IMonitor monitor)
        {
            _monitor = monitor;
        }
        
        public static Item AddItemToInventory(this Farmer farmer, Item item, int slots = 12)
        {
            // Find prioritized storage
            var storageConfigs = farmer.Items
                .Take(slots)
                .Where(i => i is Chest)
                .ToDictionary(i => i as Chest, ExpandedStorage.GetConfig)
                .Where(s => s.Value != null);
            
            var storages = storageConfigs
                .Where(s =>
                    s.Value.VacuumItems
                    && s.Value.Filter(item))
                .Select(s => s.Key)
                .OrderByDescending(s => s.modData.TryGetValue(ChestsAnywhereOrderKey, out var order) ? Convert.ToInt32(order) : 0)
                .ToList();

            if (!storages.Any())
                return item;

            _monitor.Log($"Found {storages.Count} For Vacuum\n" + string.Join("\n", storages.Select(s => $"\t{s.DisplayName}")), LogLevel.Debug);

            // Insert item into storage
            foreach (var storage in storages)
            {
                item = storage.addItem(item);
                if (item == null)
                    break;
            }
            return item;
        }
    }
}