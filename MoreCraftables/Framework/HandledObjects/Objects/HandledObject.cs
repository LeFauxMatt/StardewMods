using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.HandledObjects.Objects
{
    public abstract class HandledObject : GenericHandledObject
    {
        protected HandledObject(IHandledObject handledObject) : base(handledObject)
        {
        }

        public override string[] GetData =>
            Game1.objectInformation.TryGetValue(ObjectId, out var objectInformation)
                ? objectInformation.Split('/')
                : new string[] { };

        public override bool IsHandledItem(Item item)
        {
            return item is Object obj && !obj.bigCraftable.Value && base.IsHandledItem(item);
        }
    }
}