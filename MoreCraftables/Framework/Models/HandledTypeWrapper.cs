using MoreCraftables.Framework.API;

namespace MoreCraftables.Framework.Models
{
    public class HandledTypeWrapper
    {
        public string ModUniqueId { get; set; }
        public IHandledType HandledType { get; set; }
    }
}