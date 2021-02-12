using System.Collections.Generic;
using MoreCraftables.Framework.API;
using StardewValley;

namespace MoreCraftables.Framework
{
    internal class HandledType : IHandledType
    {
        public string Type { get; set; }
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public bool IsHandledItem(Item item) => false;

        public bool CanStackWith(Item item, Item otherItem) => false;
    }
}