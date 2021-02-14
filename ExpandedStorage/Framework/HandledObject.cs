using System.Collections.Generic;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreCraftables.API;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework
{
    public class HandledObject : IHandledObject
    {
        public IHandledObject Base { get; set; }
        public string Type { get; set; } = "Chest";

        public IDictionary<string, object> Properties { get; set; }

        public bool IsHandledItem(Item item)
        {
            var config = ExpandedStorage.GetConfig(item);
            return config?.SourceType == SourceType.MoreCraftables || config?.SourceType == SourceType.Vanilla;
        }

        public bool CanStackWith(Item item, Item otherItem)
        {
            // Cannot stack if storage can be carried or accessed while carrying
            var config = ExpandedStorage.GetConfig(item);
            if (config == null || config.AccessCarried || config.CanCarry)
                return false;

            // Can stack if both are empty chests
            return (item is not Chest chest || chest.items.Count == 0) &&
                   (otherItem is not Chest otherChest || otherChest.items.Count == 0);
        }

        public Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            // Disallow placing non-placeable objects
            var config = ExpandedStorage.GetConfig(obj);
            return config?.IsPlaceable != null && config.IsPlaceable
                ? Base.CreateInstance(obj, location, pos)
                : null;
        }

        public bool Draw(Object obj, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f, IHandledObject.DrawContext drawContext = default, Color color = default)
        {
            return Base.Draw(obj, spriteBatch, pos, origin, alpha, layerDepth, scaleSize);
        }
    }
}