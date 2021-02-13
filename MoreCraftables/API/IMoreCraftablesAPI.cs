using StardewModdingAPI;

namespace MoreCraftables.API
{
    public interface IMoreCraftablesAPI
    {
        void AddHandledType(IManifest manifest, IHandledType handledType);
        void AddObjectFactory(IManifest manifest, IObjectFactory objectFactory);
    }
}