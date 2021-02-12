using StardewModdingAPI;

// ReSharper disable UnusedMember.Global

namespace MoreCraftables.Framework.API
{
    public interface IMoreCraftablesAPI
    {
        public void AddHandledType(IManifest manifest, IHandledType handledType);
        public void AddObjectFactory(IManifest manifest, IObjectFactory objectFactory);
    }
}