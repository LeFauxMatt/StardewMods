namespace FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Helpers;
using FuryCore.Attributes;
using FuryCore.Models;
using StardewModdingAPI.Events;

/// <summary>
/// An event whose handlers support sorted priority.
/// </summary>
/// <typeparam name="TEventArgs">The type/class of event arguments.</typeparam>
internal abstract class SortedEventHandler<TEventArgs>
{
    /// <summary>
    /// Gets the total number of registered handlers.
    /// </summary>
    protected int HandlerCount
    {
        get => this.Handlers.Count;
    }

    private SortedList<EventOrderKey, EventHandler<TEventArgs>> Handlers { get; } = new();

    /// <summary>
    /// Adds a new handler for this event.
    /// </summary>
    /// <param name="handler">The handler method top add.</param>
    public void Add(EventHandler<TEventArgs> handler)
    {
        lock (this.Handlers)
        {
            var priority = handler.Method.GetCustomAttribute<SortedEventPriorityAttribute>()?.Priority ?? EventPriority.Normal;
            this.Handlers.Add(new(priority), handler);
        }
    }

    /// <summary>
    /// Removes a handler from this event.
    /// </summary>
    /// <param name="handler">The handler method to remove.</param>
    public void Remove(EventHandler<TEventArgs> handler)
    {
        lock (this.Handlers)
        {
            foreach (var (key, eventHandler) in this.Handlers)
            {
                if (eventHandler == handler)
                {
                    this.Handlers.Remove(key);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Invokes all registered handlers.
    /// </summary>
    /// <param name="eventArgs">The event arguments to send to handlers.</param>
    protected void InvokeAll(TEventArgs eventArgs)
    {
        foreach (var handler in this.Handlers.Values)
        {
            try
            {
                handler.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed in {nameof(SortedEventHandler<TEventArgs>)}. {ex.Message}");
            }
        }
    }
}