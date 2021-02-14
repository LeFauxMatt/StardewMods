using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace MoreCraftables.API
{
    public interface IHandledObject
    {
        enum DrawContext
        {
            Placed,
            Menu,
            Held
        }

        string Type { get; set; }

        IDictionary<string, object> Properties { get; set; }
        IHandledObject Base { get; set; }

        bool IsHandledItem(Item item);

        bool CanStackWith(Item item, Item otherItem);

        Object CreateInstance(Object obj, GameLocation location, Vector2 pos);

        bool Draw(Object obj, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f, DrawContext drawContext = default, Color color = default);
    }
}