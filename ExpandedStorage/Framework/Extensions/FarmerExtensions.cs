using System;
using System.Linq;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Extensions
{
    public static class FarmerExtensions
    {
        private const string ChestsAnywhereOrderKey = "Pathoschild.ChestsAnywhere/Order";
        public static Item AddItemToInventory(this Farmer farmer, Item item, int slots = 12)
        {
            // Find prioritized storage
            var storages = farmer.Items
                .Take(slots)
                .Where(i => i is Chest)
                .ToDictionary(i => i as Chest, ExpandedStorage.GetConfig)
                .Where(s =>
                    s.Value != null
                    && s.Value.VacuumItems
                    && s.Value.IsAllowed(item)
                    && !s.Value.IsBlocked(item))
                .Select(s => s.Key)
                .OrderByDescending(s => s.modData.TryGetValue(ChestsAnywhereOrderKey, out var order) ? Convert.ToInt32(order) : 0);
            
            // Insert item into storage
            Item returnItem = null;
            foreach (var storage in storages)
            {
                returnItem = storage.addItem(item);
                if (returnItem == null)
                    break;
            }
            return returnItem;
        }
    }
}