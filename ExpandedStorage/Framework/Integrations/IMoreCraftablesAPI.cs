namespace ExpandedStorage.Framework.Integrations
{
    public interface IMoreCraftablesAPI
    {
        void AddHandledType(IHandledType handledType);
        void AddObjectFactory(IObjectFactory objectFactory);
    }
}