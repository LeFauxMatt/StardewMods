namespace Common.Services
{
    internal interface IService
    {
        /// <summary>
        ///     Gets the name of the service.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        ///     Resolves all requested dependencies.
        /// </summary>
        public void ResolveDependencies(ServiceManager serviceManager);
    }
}