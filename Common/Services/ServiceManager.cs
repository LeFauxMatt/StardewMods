namespace Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Service manager to request shared services.
    /// </summary>
    public class ServiceManager
    {
        private static ServiceManager Instance;
        private readonly IDictionary<string, BaseService> _services = new Dictionary<string, BaseService>();

        private ServiceManager()
        {
        }

        /// <summary>Returns and creates if needed an instance of the <see cref="ServiceManager"/> class.</summary>
        /// <returns>An instance of <see cref="ServiceManager"/> class.</returns>
        public static ServiceManager GetSingleton()
        {
            return ServiceManager.Instance ??= new ServiceManager();
        }

        /// <summary>Add to collection of active service instances.</summary>
        /// <typeparam name="TServiceType">Type of service to add.</typeparam>
        public void AddSingleton<TServiceType>()
        {
            var service = (BaseService)typeof(TServiceType).GetMethod("GetSingleton", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { this });
            if (service != null)
            {
                this._services.Add(service.ServiceName, service);
            }
        }

        /// <summary>Add to collection of active service instances.</summary>
        /// <param name="args">Additional parameters used to instantiate service.</param>
        /// <typeparam name="TServiceType">Type of service to add.</typeparam>
        public void AddSingleton<TServiceType>(params object[] args)
        {
            BaseService service;

            if (args.Length > 0)
            {
                var newArgs = new object[args.Length + 1];
                newArgs[0] = this;
                Array.Copy(args, 0, newArgs, 1, args.Length);
                service = (BaseService)typeof(TServiceType).GetMethod("GetSingleton", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, newArgs);
            }
            else
            {
                service = (BaseService)typeof(TServiceType).GetMethod("GetSingleton", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { this });
            }

            if (service != null)
            {
                this._services.Add(service.ServiceName, service);
            }
        }

        /// <summary>
        /// Request a service by name and type.
        /// </summary>
        /// <param name="serviceName">The name of the service to request.</param>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        public TServiceType RequestService<TServiceType>(string serviceName)
        {
            if (this._services.TryGetValue(serviceName, out var genericService) && genericService is TServiceType service)
            {
                return service;
            }

            return default;
        }

        /// <summary>
        /// Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        public TServiceType RequestService<TServiceType>()
        {
            var service = this._services.Values.OfType<TServiceType>().SingleOrDefault();
            return service ?? default;
        }

        /// <summary>
        /// Request a service by type.
        /// </summary>
        /// <typeparam name="TServiceType">The type of service to request.</typeparam>
        /// <returns>Returns a service of the given type.</returns>
        /// <exception cref="ArgumentException">No valid service can be found.</exception>
        public List<TServiceType> RequestServices<TServiceType>()
        {
            return this._services.Values.OfType<TServiceType>().ToList();
        }
    }
}