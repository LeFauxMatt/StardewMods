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
            if (this.FindServices(type, new List<IServiceLocator>()).Any())
            {
                pendingService.ForceEvaluation();
            }
        }
    }

    /// <inheritdoc/>
    public TServiceType FindService<TServiceType>()
    {
        return this.FindServices<TServiceType>().SingleOrDefault();
    }

    /// <inheritdoc />
    public IEnumerable<TServiceType> FindServices<TServiceType>()
    {
        return this.FindServices(typeof(TServiceType), new List<IServiceLocator>()).Cast<TServiceType>();
    }

    /// <inheritdoc />
    public IEnumerable<IService> FindServices(Type type, IList<IServiceLocator> exclude)
    {
        // Find from local
        var services = this.Where(type.IsInstanceOfType);

        // Recursive search in external services
        exclude.Add(this);
        services = services.Concat(
            from serviceLocator in this.OfType<IServiceLocator>().Except(exclude)
            from service in serviceLocator.FindServices(type, exclude)
            select service);

        return services;
    }
}