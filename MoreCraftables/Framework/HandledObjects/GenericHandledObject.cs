using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.HandledObjects
{
    public class GenericHandledObject : IHandledObject
    {
        private readonly string _playSound;

        /// <summary>ParentSheetIndex related to this item.</summary>
        internal int ObjectId;

        protected GenericHandledObject()
        {
            Properties = new Dictionary<string, object>();
        }

        protected GenericHandledObject(IHandledObject handledObject)
        {
            Type = handledObject.Type;
            Properties = handledObject.Properties;
            _playSound = Properties.TryGetValue("playSound", out var playSound) && playSound is string value
                ? value
                : null;
        }

        public virtual string[] GetData => new string[] { };
        public IHandledObject Base { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> Properties { get; set; }

        public virtual bool IsHandledItem(Item item)
        {
            return item.ParentSheetIndex == ObjectId;
        }

        public virtual bool CanStackWith(Item item, Item otherItem)
        {
            return true;
        }

        public virtual Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            return null;
        }

        public virtual bool Draw(Object obj, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f, IHandledObject.DrawContext drawContext = default, Color color = default)
        {
            return false;
        }

        internal void PlaySoundOrDefault(GameLocation location, string defaultSound)
        {
            location.playSound(_playSound ?? defaultSound);
        }

        internal enum ItemType
        {
            Cask,
            Chest,
            CrabPot,
            Fence
        }
    }
}