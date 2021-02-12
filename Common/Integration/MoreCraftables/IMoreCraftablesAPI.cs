using StardewModdingAPI;

namespace Common.Integration.MoreCraftables
{
    public interface IMoreCraftablesAPI
    {
        public void AddHandledType(IManifest manifest, IHandledType handledType);
        public void AddObjectFactory(IManifest manifest, IObjectFactory objectFactory);
    }
}