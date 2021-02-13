using System.Collections.Generic;
using MoreCraftables.API;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;

namespace MoreCraftables
{
    public class MoreCraftablesAPI : IMoreCraftablesAPI
    {
        private readonly IList<HandledTypeWrapper> _handledTypes;
        private readonly IMonitor _monitor;
        private readonly IList<ObjectFactoryWrapper> _objectFactories;

        public MoreCraftablesAPI(IMonitor monitor, IList<HandledTypeWrapper> handledTypes, IList<ObjectFactoryWrapper> objectFactories)
        {
            _monitor = monitor;
            _handledTypes = handledTypes;
            _objectFactories = objectFactories;
        }

        public void AddHandledType(IManifest manifest, IHandledType handledType)
        {
            _monitor.Log($"Adding HandledType {handledType.Type} from {manifest.UniqueID}");
            _handledTypes.Add(new HandledTypeWrapper(manifest, handledType));
        }

        public void AddObjectFactory(IManifest manifest, IObjectFactory objectFactory)
        {
            _monitor.Log($"Adding ObjectFactory from {manifest.UniqueID}");
            _objectFactories.Add(new ObjectFactoryWrapper(manifest, objectFactory));
        }
    }
}