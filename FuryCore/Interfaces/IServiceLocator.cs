namespace FuryCore.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>
///     Request lazy instances of dependency services.
/// </summary>
public interface IServiceLocator
{
    /// <summary>
    ///     Request a lazy instance of an <see cref="IService" />.
    /// </summary>
    /// <param name="action">An action to perform on the service instance.</param>
    /// <typeparam name="TServiceType">The class/type of a service.</typeparam>
    /// <returns>Returns the first <see cref="IService" /> that matches the condition.</returns>
    public Lazy<TServiceType> Lazy<TServiceType>(Action<TServiceType> action = default);

    /// <summary>
    ///     Forces all Lazy service values to be evaluated.
    /// </summary>
    public void ForceEvaluation();

    /// <summary>
    ///     Returns an instantiated service by it's type.
    /// </summary>
    /// <typeparam name="TServiceType">The class/type of a service.</typeparam>
    /// <returns>Returns the first <see cref="IService" /> that matches the condition.</returns>
    public TServiceType FindService<TServiceType>();

    /// <summary>
    /// </summary>
    /// <param name="type">The class/type of a service.</param>
    /// <param name="exclude">
    ///     Used for recursive logic to prevent searching the same <see cref="IServiceLocator" /> more than
    ///     once.
    /// </param>
    /// <returns>Returns the first <see cref="IService" /> that matches the condition.</returns>
    public object FindService(Type type, IList<IServiceLocator> exclude);
}