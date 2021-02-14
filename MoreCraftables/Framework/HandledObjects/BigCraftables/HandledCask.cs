using Microsoft.Xna.Framework;
using MoreCraftables.API;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace MoreCraftables.Framework.HandledObjects.BigCraftables
{
    public class HandledCask : HandledBigCraftable
    {
        public HandledCask(IHandledObject handledObject) : base(handledObject)
        {
        }

        public override Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            PlaySoundOrDefault(location, "hammer");
            return new Cask(pos);
        }
    }
}