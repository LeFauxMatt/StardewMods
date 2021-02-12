using System.Collections.Generic;
using StardewValley;
using Object = StardewValley.Object;

namespace Common.Integration.MoreCraftables
{
    public interface IHandledType
    {
        public string Type { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public bool IsHandledItem(Item item);

        public bool CanStackWith(Item item, Item otherItem);
    }
}