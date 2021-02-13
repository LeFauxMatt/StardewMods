using System.Collections.Generic;
using MoreCraftables.API;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework
{
    public class HandledType : IHandledType
    {
        public string Type { get; set; } = "ExpandedStorage";

        public IDictionary<string, object> Properties { get; set; }

        public bool IsHandledItem(Item item)
        {
            return ExpandedStorage.HasConfig(item);
        }

        public bool CanStackWith(Item item, Item otherItem)
        {
            // Cannot stack if storage can be carried or accessed while carrying
            var config = ExpandedStorage.GetConfig(item);
            if (config == null || config.AccessCarried || config.CanCarry)
                return false;

            // Can stack if both are empty chests
            return (item is not Chest chest || chest.items.Count == 0) &&
                   (otherItem is not Chest otherChest || otherChest.items.Count == 0);
        }
    }
}