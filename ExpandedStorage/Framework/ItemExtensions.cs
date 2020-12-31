using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework.Models;
using StardewValley;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage.Framework
{
    internal static class ItemExtensions
    {
        private static IDictionary<int, ExpandedStorageData> _expandedStorage;
        internal static void Init(IEnumerable<ExpandedStorageData> expandedStorage)
        {
            _expandedStorage = expandedStorage.ToDictionary(s => s.ParentSheetIndex, s => s);
        }
        internal static bool ShouldBeExpandedStorage(this Item item) =>
            item is SDVObject obj &&
            (bool)obj.bigCraftable &&
            _expandedStorage.ContainsKey(item.ParentSheetIndex);
        internal static bool IsExpandedStorage(this Item item) =>
            item is Chest chest &&
            chest.modData.ContainsKey("ImJustMatt.ExpandedStorage/actual-capacity");
        internal static Chest ToExpandedStorage(this Item item)
        {
            if (!(item is SDVObject obj))
                throw new InvalidOperationException($"Cannot convert {item.Name} to Chest");
            
            if (!(obj is Chest chest))
            {
                chest = new Chest(true, obj.TileLocation, obj.ParentSheetIndex)
                {
                    name = obj.name
                };
            }
            
            // Use existing capacity
            if (chest.modData.ContainsKey("ImJustMatt.ExpandedStorage/actual-capacity"))
                return chest;
            
            // Assign modded capacity into Chest
            if (!obj.modData.TryGetValue("ImJustMatt.ExpandedStorage/actual-capacity", out var actualCapacity))
                actualCapacity = _expandedStorage[obj.ParentSheetIndex].Capacity.ToString();
            
            chest.modData["ImJustMatt.ExpandedStorage/actual-capacity"] = actualCapacity switch
            {
                "-1" => int.MaxValue.ToString(),
                "0" => "36",
                _ => actualCapacity
            };
            return chest;
        }
    }
}