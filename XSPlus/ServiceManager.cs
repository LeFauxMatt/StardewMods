namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using StardewModdingAPI;

    /// <summary>
    /// Service manager to request shared services.
    /// </summary>
    internal class ServiceManager
    {
        private readonly IDictionary<string, BaseService> _services = new Dictionary<string, BaseService>();

        /// <summary>Add to collection of active service instances.</summary>
        /// <param name="args">Additional parameters used to instantiate service.</param>
        /// <typeparam name="TServiceType">Type of service to add.</typeparam>
        public void AddSingleton<TServiceType>(params object[] args)
        {
            BaseService service;

            if (args.Length > 0)
            {
                object[] newArgs = new object[args.Length + 1];
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

            throw new ArgumentException();
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
            if (service is not null)
            {
                return service;
            }

            throw new ArgumentException();
        }
    }
}