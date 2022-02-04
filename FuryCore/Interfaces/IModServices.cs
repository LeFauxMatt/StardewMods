namespace StardewMods.FuryCore.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>
///     A collection of services created by this mod.
/// </summary>
public interface IModServices
{
    /// <summary>
    ///     Gets all services in this collection.
    /// </summary>
    public IEnumerable<IModService> All { get; }

    /// <summary>
    ///     Finds a service that is an instance of a type.
    /// </summary>
    /// <typeparam name="TServiceType">The class/type of a service.</typeparam>
    /// <returns>Returns the first <see cref="IModService" /> that matches the condition.</returns>
    public TServiceType FindService<TServiceType>();

    /// <summary>
    ///     Returns an instantiated service by it's type.
    /// </summary>
    /// <typeparam name="TServiceType">The class/type of a service.</typeparam>
    /// <returns>Returns the first <see cref="IModService" /> that matches the condition.</returns>
    public IEnumerable<TServiceType> FindServices<TServiceType>();

    /// <summary>
    ///     Finds services that are an instance of a type.
    /// </summary>
    /// <param name="type">The class/type of a service.</param>
    /// <param name="exclude">
    ///     Used for recursive logic to prevent searching the same <see cref="IModServices" />
    ///     more than once.
    /// </param>
    /// <returns>Returns the first <see cref="IModService" /> that matches the condition.</returns>
    public IEnumerable<IModService> FindServices(Type type, IList<IModServices> exclude);

    /// <summary>
    ///     Request a lazy instance of an <see cref="IModService" />.
    /// </summary>
    /// <param name="action">An action to perform on the service instance.</param>
    /// <typeparam name="TServiceType">The class/type of a service.</typeparam>
    /// <returns>Returns the first <see cref="IModService" /> that matches the condition.</returns>
    public Lazy<TServiceType> Lazy<TServiceType>(Action<TServiceType> action = default);
}