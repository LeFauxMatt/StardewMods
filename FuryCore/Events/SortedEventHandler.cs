namespace FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Helpers;
using FuryCore.Attributes;
using FuryCore.Models;
using StardewModdingAPI.Events;

internal abstract class SortedEventHandler<TEventArgs>
{
    protected int HandlerCount
    {
        get => this.Handlers.Count;
    }

    private SortedList<EventOrderKey, EventHandler<TEventArgs>> Handlers { get; } = new();

    public void Add(EventHandler<TEventArgs> handler)
    {
        lock (this.Handlers)
        {
            var priority = handler.Method.GetCustomAttribute<SortedEventPriorityAttribute>()?.Priority ?? EventPriority.Normal;
            this.Handlers.Add(new(priority), handler);
        }
    }

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