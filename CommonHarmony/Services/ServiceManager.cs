namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common.Helpers;
    using StardewModdingAPI;

    /// <summary>
    ///     Service manager to request shared services.
    /// </summary>
    public class ServiceManager
    {
        private static ServiceManager Instance;
        private readonly IDictionary<string, BaseService> _services = new Dictionary<string, BaseService>();

        private ServiceManager(IModHelper helper, IManifest manifest)
        {
            this.Helper = helper;
            this.ModManifest = manifest;
        }

        public IModHelper Helper { get; }
        public IManifest ModManifest { get; }

        /// <summary>Returns and creates if needed an instance of the <see cref="ServiceManager" /> class.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// <param name="manifest">A manifest which describes a mod for SMAPI.</param>
        /// <returns>An instance of <see cref="ServiceManager" /> class.</returns>
        public static ServiceManager Create(IModHelper helper, IManifest manifest)
        {
            return ServiceManager.Instance ??= new(helper, manifest);
        }

        /// <summary>
        ///     Request a service by name and type.
        /// </summary>
        /// <param name="serviceName">The name of the service to request.</param>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        internal async Task<TServiceType> Get<TServiceType>(string serviceName) where TServiceType : BaseService
        {
            if (this._services.TryGetValue(serviceName, out var genericService) && genericService is TServiceType service)
            {
                return service;
            }

            return await this.Create<TServiceType>();
        }

        /// <summary>
        ///     Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="NullReferenceException">No valid service can be found.</exception>
        internal async Task<TServiceType> Get<TServiceType>() where TServiceType : BaseService
        {
            return this._services.Values.OfType<TServiceType>().SingleOrDefault() ?? await this.Create<TServiceType>();
        }

        internal async Task<TServiceType> Create<TServiceType>() where TServiceType : BaseService
        {
            var service = this._services.Values.OfType<TServiceType>().SingleOrDefault();
            if (service is not null)
            {
                return service;
            }

            var method = typeof(TServiceType).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            var task = (Task<TServiceType>)method?.Invoke(
                null,
                new object[]
                {
                    this,
                });

            service = await (task ?? throw new NullReferenceException(nameof(TServiceType)));
            Log.Verbose($"Registering service {service.ServiceName}.");
            this._services.Add(service.ServiceName, service);
            return service;
        }

        /// <summary>
        ///     Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        internal List<TServiceType> GetAll<TServiceType>()
        {
            return this._services.Values.OfType<TServiceType>().ToList();
        }
    }
}