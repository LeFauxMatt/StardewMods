using Microsoft.Xna.Framework;
using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.HandledObjects.Objects
{
    public class HandledFence : HandledObject
    {
        private readonly bool _isGate;

        private readonly int _whichType;

        public HandledFence(IHandledObject handledObject) : base(handledObject)
        {
            _whichType = Properties.TryGetValue("whichType", out var whichType) && whichType is int whichTypeValue
                ? whichTypeValue
                : 0;
            _isGate = Properties.TryGetValue("isGate", out var isGate) && (bool) isGate;
        }

        public override Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            PlaySoundOrDefault(location, "axe");
            return new Fence(pos, _whichType, _isGate);
        }
    }
}