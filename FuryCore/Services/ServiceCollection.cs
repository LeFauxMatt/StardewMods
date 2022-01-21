namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using FuryCore.Interfaces;

/// <inheritdoc cref="FuryCore.Interfaces.IService" />
public class ServiceCollection : List<IService>, IServiceLocator, IService
{
    private IDictionary<Type, IPendingService> PendingServices { get; } = new Dictionary<Type, IPendingService>();

    /// <inheritdoc />
    public Lazy<TServiceType> Lazy<TServiceType>(Action<TServiceType> action = default)
    {
        var type = typeof(TServiceType);
        if (!this.PendingServices.TryGetValue(type, out var pendingService))
        {
            pendingService = new PendingService<TServiceType>(this.FindService<TServiceType>);
            this.PendingServices.Add(type, pendingService);
        }

        if (pendingService is PendingService<TServiceType> specificPendingService)
        {
            if (action is not null)
            {
                specificPendingService.Actions.Add(action);
            }

            return specificPendingService.LazyInstance;
        }

        return default;
    }

    /// <inheritdoc />
    public void ForceEvaluation()
    {
        // Force evaluation of Lazy Instances
        foreach (var (type, pendingService) in this.PendingServices)
        {
            if (this.FindService(type, new List<IServiceLocator>()) is not null)
            {
                pendingService.ForceEvaluation();
            }
        }
    }

    /// <inheritdoc />
    public TServiceType FindService<TServiceType>()
    {
        return (TServiceType)this.FindService(typeof(TServiceType), new List<IServiceLocator>());
    }

    /// <inheritdoc />
    public object FindService(Type type, IList<IServiceLocator> exclude)
    {
        // Find from local
        object service = this.SingleOrDefault(type.IsInstanceOfType);
        if (service is not null)
        {
            return service;
        }

        // Recursive search in external services
        exclude.Add(this);
        foreach (var serviceCollection in this.OfType<IServiceLocator>().Except(exclude))
        {
            service = serviceCollection.FindService(type, exclude);
            if (service is not null)
            {
                return service;
            }
        }

        // Not found
        return default;
    }
}