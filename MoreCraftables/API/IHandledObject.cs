using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MoreCraftables.API
{
    public interface IHandledObject
    {
        string Type { get; set; }

        IDictionary<string, object> Properties { get; set; }
        IHandledObject Base { get; set; }

        bool IsHandledItem(Item item);

        bool CanStackWith(Item item, Item otherItem);

        Object CreateInstance(Object obj, GameLocation location, Vector2 pos);
    }
}