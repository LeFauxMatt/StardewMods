using Microsoft.Xna.Framework;
using StardewValley;

// ReSharper disable UnusedParameter.Global

// ReSharper disable UnusedMember.Global

namespace Common.Integration.MoreCraftables
{
    public interface IObjectFactory
    {
        public Object CreateInstance(IHandledType handledType, Object obj, GameLocation location, Vector2 pos);

        public bool IsHandledType(IHandledType handledType);
    }
}