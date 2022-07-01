#nullable disable

namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class PendingService<TServiceType> : IPendingService
{
    private readonly Lazy<TServiceType> _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PendingService{TServiceType}" /> class.
    /// </summary>
    /// <param name="valueFactory">The function which returns an instance of the service.</param>
    public PendingService(Func<TServiceType> valueFactory)
    {
        this._service = new(valueFactory);
        this.LazyInstance = new(this.ValueFactory);
    }

    /// <summary>
    ///     Gets the actions to complete after the service is instantiated.
    /// </summary>
    public List<Action<TServiceType>> Actions { get; } = new();

    /// <summary>
    ///     Gets the lazy instance of the service.
    /// </summary>
    public Lazy<TServiceType> LazyInstance { get; }

    /// <inheritdoc />
    public void ForceEvaluation()
    {
        _ = this.LazyInstance.Value;
    }

    private TServiceType ValueFactory()
    {
        var service = this._service.Value;

        foreach (var action in this.Actions)
        {
            action.Invoke(service);
        }

        this.Actions.Clear();

        return service;
    }
}