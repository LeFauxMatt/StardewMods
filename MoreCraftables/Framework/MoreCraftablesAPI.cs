using System.Collections.Generic;
using MoreCraftables.Framework.API;
using StardewModdingAPI;

namespace MoreCraftables.Framework
{
    public class MoreCraftablesAPI : IMoreCraftablesAPI
    {
        private readonly IList<IHandledType> _handledTypes;
        private readonly IMonitor _monitor;
        private readonly IList<IObjectFactory> _objectFactories;

        public MoreCraftablesAPI(IMonitor monitor, IList<IHandledType> handledTypes, IList<IObjectFactory> objectFactories)
        {
            _monitor = monitor;
            _handledTypes = handledTypes;
            _objectFactories = objectFactories;
        }

        public void AddHandledType(IHandledType handledType)
        {
            _monitor.Log($"Adding HandledType {handledType.Type}");
            _handledTypes.Add(handledType);
        }

        public void AddObjectFactory(IObjectFactory objectFactory)
        {
            _monitor.Log($"Adding ObjectFactory");
            _objectFactories.Add(objectFactory);
        }
    }
}