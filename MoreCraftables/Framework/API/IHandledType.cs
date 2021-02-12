using System.Collections.Generic;
using StardewValley;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace MoreCraftables.Framework.API
{
    public interface IHandledType
    {
        public string Type { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public bool IsHandledItem(Item item);

        public bool CanStackWith(Item item, Item otherItem);
    }
}