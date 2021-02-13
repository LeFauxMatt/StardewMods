using MoreCraftables.API;
using StardewModdingAPI;

namespace MoreCraftables.Framework.Models
{
    public class ObjectFactoryWrapper
    {
        public ObjectFactoryWrapper(IManifest manifest, IObjectFactory objectFactory)
        {
            ModUniqueId = manifest.UniqueID;
            ObjectFactory = objectFactory;
        }

        public string ModUniqueId { get; set; }
        public IObjectFactory ObjectFactory { get; set; }
    }
}