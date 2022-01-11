namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using FuryCore.Interfaces;

/// <inheritdoc />
internal class PendingService<TServiceType> : IPendingService
{
    private readonly Lazy<TServiceType> _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingService{TServiceType}"/> class.
    /// </summary>
    /// <param name="valueFactory"></param>
    public PendingService(Func<TServiceType> valueFactory)
    {
        this._service = new(valueFactory);
        this.LazyInstance = new(this.ValueFactory);
    }

    public List<Action<TServiceType>> Actions { get; } = new();

    public Lazy<TServiceType> LazyInstance { get; }

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