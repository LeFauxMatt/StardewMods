using Microsoft.Xna.Framework;
using MoreCraftables.API;
using StardewValley;
using StardewValley.Objects;

namespace MoreCraftables.Framework.HandledObjects.BigCraftables
{
    public class HandledCrabPot : HandledBigCraftable
    {
        public HandledCrabPot(IHandledObject handledObject) : base(handledObject)
        {
        }
        
        public override Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            if (!CrabPot.IsValidCrabPotLocationTile(location, (int) pos.X, (int) pos.Y))
                return null;
            var crabPot = new CrabPot(pos)
            {
                ParentSheetIndex = obj.ParentSheetIndex
            };
            PlaySoundOrDefault(location, "waterSlosh");
            DelayedAction.playSoundAfterDelay("slosh", 150);
            crabPot.updateOffset(location);
            crabPot.addOverlayTiles(location);
            return crabPot;
        }
    }
}