using Microsoft.Xna.Framework;
using StardewValley;

namespace MoreCraftables.API
{
    public interface IObjectFactory
    {
        Object CreateInstance(IHandledType handledType, Object obj, GameLocation location, Vector2 pos);

        bool IsHandledType(IHandledType handledType);
    }
}