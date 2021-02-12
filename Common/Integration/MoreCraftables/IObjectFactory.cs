using Microsoft.Xna.Framework;
using StardewValley;

namespace Common.Integration.MoreCraftables
{
    public interface IObjectFactory
    {
        public Object CreateInstance(IHandledType handledType, Object obj, GameLocation location, Vector2 pos);

        public bool IsHandledType(IHandledType handledType);
    }
}