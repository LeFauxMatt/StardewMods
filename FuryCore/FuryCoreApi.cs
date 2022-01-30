namespace StardewMods.FuryCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc cref="FuryCore.Interfaces.IModServices" />
public class FuryCoreApi : IModServices, IModService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreApi" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public FuryCoreApi(IModServices services)
    {
        this.Services = services;
    }

    /// <inheritdoc/>
    public IEnumerable<IModService> All
    {
        get => this.Services.All.Where(service => service.GetType().GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true);
    }

    private IModServices Services { get; }

    /// <inheritdoc/>
    public Lazy<TServiceType> Lazy<TServiceType>(Action<TServiceType> action = default)
    {
        return typeof(TServiceType).GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true
            ? this.Services.Lazy(action)
            : default;
    }

    /// <inheritdoc/>
    public TServiceType FindService<TServiceType>()
    {
        return typeof(TServiceType).GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true
            ? this.Services.FindService<TServiceType>()
            : default;
    }

    /// <inheritdoc/>
    public IEnumerable<TServiceType> FindServices<TServiceType>()
    {
        return this.Services.FindServices<TServiceType>().Where(service => service.GetType().GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true);
    }

    /// <inheritdoc/>
    public IEnumerable<IModService> FindServices(Type type, IList<IModServices> exclude)
    {
        return this.Services.FindServices(type, exclude).Where(service => service.GetType().GetCustomAttribute<FuryCoreServiceAttribute>()?.Exportable == true);
    }
}