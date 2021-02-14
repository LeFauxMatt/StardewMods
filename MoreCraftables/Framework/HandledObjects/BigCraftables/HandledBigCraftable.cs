using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.HandledObjects.BigCraftables
{
    public abstract class HandledBigCraftable : GenericHandledObject
    {
        protected HandledBigCraftable(IHandledObject handledObject) : base(handledObject)
        {
        }

        public override string[] GetData =>
            Game1.bigCraftablesInformation.TryGetValue(ObjectId, out var objectInformation)
                ? objectInformation.Split('/')
                : new string[] { };

        public override bool IsHandledItem(Item item)
        {
            return item is Object obj && obj.bigCraftable.Value && base.IsHandledItem(item);
        }
    }
}