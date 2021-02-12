using System.Collections.Generic;
using MoreCraftables.Framework.API;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;

namespace MoreCraftables.Framework
{
    public class MoreCraftablesAPI : IMoreCraftablesAPI
    {
        private readonly IList<HandledTypeWrapper> _handledTypes;
        private readonly IList<ObjectFactoryWrapper> _objectFactories;

        public MoreCraftablesAPI(IList<HandledTypeWrapper> handledTypes, IList<ObjectFactoryWrapper> objectFactories)
        {
            _handledTypes = handledTypes;
            _objectFactories = objectFactories;
        }

        public void AddHandledType(IManifest manifest, IHandledType handledType)
        {
            _handledTypes.Add(new HandledTypeWrapper
            {
                ModUniqueId = manifest.UniqueID,
                HandledType = handledType
            });
        }

        public void AddObjectFactory(IManifest manifest, IObjectFactory objectFactory)
        {
            _objectFactories.Add(new ObjectFactoryWrapper
            {
                ModUniqueId = manifest.UniqueID,
                ObjectFactory = objectFactory
            });
        }
    }
}