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
        
        /// <summary>
        /// Checks if item is a recognized Expanded Storage object.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True/False whether item should be treated as an Expanded Storage.</returns>
        internal static bool ShouldBeExpandedStorage(this Item item) =>
            item is SDVObject obj &&
            (bool)obj.bigCraftable &&
            _expandedStorage.ContainsKey(item.ParentSheetIndex);
        
        /// <summary>
        /// Checks if item is already an Expanded Storage object.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True/False whether object is already a chest with modData.</returns>
        internal static bool IsExpandedStorage(this Item item) =>
            item is Chest chest &&
            chest.modData.ContainsKey("ImJustMatt.ExpandedStorage/actual-capacity");
        
        /// <summary>
        /// Converts a vanilla chest into an Expanded Storage chest by adding the relevant modData.
        /// </summary>
        /// <param name="item">The item to convert into Expanded Storage.</param>
        /// <returns>A chest with modData added used by Expanded Storage.</returns>
        /// <exception cref="InvalidOperationException">Error thrown when attempting to convert an unsupported Item.</exception>
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