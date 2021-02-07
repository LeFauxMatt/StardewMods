using Microsoft.Xna.Framework;
using StardewValley;

namespace ExpandedStorage.Framework.Extensions
{
    public static class GameLocationExtensions
    {
        public static bool CarryChest(this GameLocation location, Vector2 pos)
        {
            if (!location.objects.TryGetValue(pos, out var obj))
                return false;
            
            var config = ExpandedStorage.GetConfig(obj);
            if (config == null || !config.CanCarry || !Game1.player.addItemToInventoryBool(obj, true))
                return false;
            
            obj.TileLocation = Vector2.Zero;
            location.objects.Remove(pos);
            return true;
        }
    }
}