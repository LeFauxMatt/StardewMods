namespace Common.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Encapsulates services that support the features of this mod.
    /// </summary>
    internal abstract class BaseService : IService
    {
        private readonly Dictionary<Type, IList<Action<IService>>> _pendingDependencies = new();
        //private readonly HashSet<Type> _pendingDependencies = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseService" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        private protected BaseService(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        private protected IList<IService> Dependencies { get; } = new List<IService>();

        /// <inheritdoc />
        public string ServiceName { get; }

        /// <inheritdoc />
        public void ResolveDependencies(ServiceManager serviceManager)
        {
            foreach (var dependency in this._pendingDependencies)
            {
                var service = (IService)typeof(ServiceManager).GetMethod(nameof(ServiceManager.GetByType))?.MakeGenericMethod(dependency.Key).Invoke(
                    serviceManager,
                    new object[]
                    {
                    });

                this.Dependencies.Add(service);

                foreach (var handler in dependency.Value)
                {
                    handler(service);
                }
            }
        }

        private protected void AddDependency<TServiceType>(Action<IService> handler) where TServiceType : IService
        {
            var type = typeof(TServiceType);
            if (!this._pendingDependencies.TryGetValue(type, out var handlers))
            {
                handlers = new List<Action<IService>>();
                this._pendingDependencies.Add(type, handlers);
            }

            handlers.Add(handler);
        }
    }
}