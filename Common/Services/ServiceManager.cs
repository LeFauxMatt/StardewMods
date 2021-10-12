namespace Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using StardewModdingAPI;

    /// <summary>
    ///     Service manager to request shared services.
    /// </summary>
    internal class ServiceManager
    {
        private readonly IList<PendingService> _pendingServices = new List<PendingService>();
        private readonly IDictionary<string, IService> _services = new Dictionary<string, IService>();

        public ServiceManager(IModHelper helper, IManifest manifest)
        {
            ServiceManager.Instance ??= this;
            this.Helper = helper;
            this.ModManifest = manifest;
        }

        private static ServiceManager Instance { get; set; }
        public IModHelper Helper { get; }
        public IManifest ModManifest { get; }

        /// <summary>
        ///     Request a service by name and type.
        /// </summary>
        /// <param name="serviceName">The name of the service to request.</param>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        public TServiceType GetByName<TServiceType>(string serviceName) where TServiceType : IService
        {
            return (TServiceType)this._services[serviceName];
        }

        /// <summary>
        ///     Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="NullReferenceException">No valid service can be found.</exception>
        public TServiceType GetByType<TServiceType>() where TServiceType : IService
        {
            return this._services.Values.OfType<TServiceType>().Single();
        }

        public PendingService Create<TServiceType>() where TServiceType : IService
        {
            var pendingService = new PendingService(typeof(TServiceType));
            this._pendingServices.Add(pendingService);
            return pendingService;
        }

        /// <summary>
        ///     Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        public List<TServiceType> GetAll<TServiceType>() where TServiceType : IService
        {
            return this._services.Values.OfType<TServiceType>().ToList();
        }

        public void ResolveDependencies()
        {
            for (var i = this._pendingServices.Count - 1; i >= 0; i--)
            {
                var service = this._pendingServices[i].Create(this);
                Log.Trace($"Registering service {service.ServiceName}.", true);
                this._services.Add(service.ServiceName, service);
            }

            foreach (var service in this._services.Values)
            {
                service.ResolveDependencies(this);
            }
        }
    }
}