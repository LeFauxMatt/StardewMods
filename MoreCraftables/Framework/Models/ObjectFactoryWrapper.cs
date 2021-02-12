using MoreCraftables.Framework.API;

namespace MoreCraftables.Framework.Models
{
    public class ObjectFactoryWrapper
    {
        public string ModUniqueId { get; set; }
        public IObjectFactory ObjectFactory { get; set; }
    }
}