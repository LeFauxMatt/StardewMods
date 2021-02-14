using System.Collections.Generic;
using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.HandledObjects.Furniture
{
    public abstract class HandledFurniture : GenericHandledObject
    {
        protected HandledFurniture(IHandledObject handledObject) : base(handledObject)
        {
        }

        public override string[] GetData
        {
            get
            {
                var dataSheet = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
                if (!dataSheet.ContainsKey(ObjectId))
                    dataSheet = Game1.content.LoadBase<Dictionary<int, string>>("Data\\Furniture");
                return dataSheet.TryGetValue(ObjectId, out var objectInformation)
                    ? objectInformation.Split('/')
                    : new string[] { };
            }
        }

        public override bool IsHandledItem(Item item)
        {
            return item is Object obj && obj.bigCraftable.Value && base.IsHandledItem(item);
        }
    }
}