namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using FuryCore.Interfaces;

/// <inheritdoc cref="IModService" />
public class ModServices : IModServices, IModService
{
    /// <inheritdoc/>
    public IEnumerable<IModService> All
    {
        get => this.Services;
    }

    private IDictionary<Type, IPendingService> PendingServices { get; } = new Dictionary<Type, IPendingService>();

    private List<IModService> Services { get; } = new();

    /// <summary>
    ///     Adds service(s) to this collection.
    /// </summary>
    /// <param name="services">The service(s) to add.</param>
    public void Add(params IModService[] services)
    {
        this.Services.AddRange(services);

        // Force evaluation of Lazy Instances
        foreach (var (type, pendingService) in this.PendingServices)
        {
            if (this.FindServices(type, new List<IModServices>()).Any())
            {
                pendingService.ForceEvaluation();
            }
        }
    }

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

    /// <inheritdoc/>
    public TServiceType FindService<TServiceType>()
    {
        return this.FindServices<TServiceType>().SingleOrDefault();
    }

    /// <inheritdoc />
    public IEnumerable<TServiceType> FindServices<TServiceType>()
    {
        return this.FindServices(typeof(TServiceType), new List<IModServices>()).Cast<TServiceType>();
    }

    /// <inheritdoc/>
    public IEnumerable<IModService> FindServices(Type type, IList<IModServices> exclude)
    {
        exclude.Add(this);
        var services = this.Services.Where(type.IsInstanceOfType).Concat(
            from serviceLocator in this.Services.OfType<IModServices>().Except(exclude)
            from subService in serviceLocator.FindServices(type, exclude)
            select subService).ToList();
        return services;
    }
}