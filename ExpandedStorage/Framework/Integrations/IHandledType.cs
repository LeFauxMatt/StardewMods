using System.Collections.Generic;
using StardewValley;

namespace ExpandedStorage.Framework.Integrations
{
    public interface IHandledType
    {
        string Type { get; set; }

        IDictionary<string, object> Properties { get; set; }

        bool IsHandledItem(Item item);

        bool CanStackWith(Item item, Item otherItem);
    }
}