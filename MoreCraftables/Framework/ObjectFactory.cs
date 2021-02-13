using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoreCraftables.Framework.API;
using MoreCraftables.Framework.Extensions;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

// ReSharper disable UnusedType.Global

namespace MoreCraftables.Framework
{
    internal class ObjectFactory : IObjectFactory
    {
        private readonly IDictionary<ItemType, ItemGetter> _itemGetters = new Dictionary<ItemType, ItemGetter>();

        public ObjectFactory()
        {
            _itemGetters.Add(ItemType.Cask, CreateCask);
            _itemGetters.Add(ItemType.Chest, CreateChest);
            _itemGetters.Add(ItemType.Fence, CreateFence);
        }

        /// <summary>Create a new Object for placement</summary>
        /// <param name="handledType">Handled type includes properties about the object to place.</param>
        /// <param name="obj">The inventory instance of object before it is placed.</param>
        /// <param name="location">The Game Location to place the object on.</param>
        /// <param name="pos">The xy-coordinates to place the object on.</param>
        /// <returns>New Object</returns>
        public Object CreateInstance(IHandledType handledType, Object obj, GameLocation location, Vector2 pos)
        {
            return Enum.TryParse(handledType.Type, out ItemType itemType) && _itemGetters.TryGetValue(itemType, out var itemGetter)
                ? itemGetter(handledType, obj, location, pos)
                : null;
        }

        public bool IsHandledType(IHandledType handledType)
        {
            return Enum.IsDefined(typeof(ItemType), handledType.Type);
        }

        /// <summary>Create a new Cask for placement</summary>
        private static Cask CreateCask(IHandledType handledType, Object obj, GameLocation location, Vector2 pos)
        {
            location.PlaySoundOrDefault(handledType, "hammer");
            return new Cask(pos);
        }

        /// <summary>Create a new Chest for placement</summary>
        private static Chest CreateChest(IHandledType handledType, Object obj, GameLocation location, Vector2 pos)
        {
            if (location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return null;
            }

            location.PlaySoundOrDefault(handledType, "axe");
            return new Chest(true, pos, obj.ParentSheetIndex)
            {
                name = obj.Name,
                shakeTimer = 50
            };
        }

        /// <summary>Create a new Fence for placement</summary>
        private static Fence CreateFence(IHandledType handledType, Object obj, GameLocation location, Vector2 pos)
        {
            location.PlaySoundOrDefault(handledType, "axe");
            return new Fence(pos,
                handledType.Properties.TryGetValue("whichType", out var whichType) ? (int) whichType : 0,
                handledType.Properties.TryGetValue("isGate", out var isGate) && (bool) isGate);
        }

        private enum ItemType
        {
            Cask,
            Chest,
            Fence
        }

        private delegate Object ItemGetter(IHandledType handledType, Object obj, GameLocation location, Vector2 pos);
    }
}