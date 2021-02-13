using MoreCraftables.API;
using StardewModdingAPI;

namespace MoreCraftables.Framework.Models
{
    public class HandledTypeWrapper
    {
        public HandledTypeWrapper(IManifest manifest, IHandledType handledType)
        {
            ModUniqueId = manifest.UniqueID;
            HandledType = handledType;
        }

        public string ModUniqueId { get; set; }
        public IHandledType HandledType { get; set; }
    }
}