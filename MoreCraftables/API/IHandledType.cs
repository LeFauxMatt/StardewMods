using System.Collections.Generic;
using StardewValley;

namespace MoreCraftables.API
{
    public interface IHandledType
    {
        string Type { get; set; }

        IDictionary<string, object> Properties { get; set; }

        bool IsHandledItem(Item item);

        bool CanStackWith(Item item, Item otherItem);
    }
}