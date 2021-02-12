using Common.Integration.MoreCraftables;
using ExpandedStorage.Framework.Extensions;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace ExpandedStorage.Framework
{
    public class ObjectFactory : IObjectFactory
    {
        public Object CreateInstance(IHandledType handledType, Object obj, GameLocation location, Vector2 pos)
        {
            var config = ExpandedStorage.GetConfig(obj);

            // Do not place unhandled objects
            if (config == null)
                return null;

            // Do not place non-placeable objects
            if (!config.IsPlaceable)
                return null;

            // Do not place in MineShaft or VolcanoDungeon
            if (location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return null;
            }

            // Create Chest instance from object
            var chest = obj.ToChest(config);
            chest.shakeTimer = 50;

            // Initialize ModData preferences
            if (config.ModData != null)
                foreach (var modData in config.ModData)
                    if (!chest.modData.ContainsKey(modData.Key))
                        chest.modData.Add(modData.Key, modData.Value);

            // Return chest
            location.playSound("hammer");
            return chest;
        }

        public bool IsHandledType(IHandledType handledType)
        {
            return handledType.Type == "ExpandedStorage";
        }
    }
}