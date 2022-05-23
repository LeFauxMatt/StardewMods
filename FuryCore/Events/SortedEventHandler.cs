#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Common.Helpers;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Models;

/// <summary>
///     An event whose handlers support sorted priority.
/// </summary>
/// <typeparam name="TEventArgs">The type/class of event arguments.</typeparam>
public abstract class SortedEventHandler<TEventArgs>
{
    /// <summary>
    ///     Gets the total number of registered handlers.
    /// </summary>
    protected int HandlerCount
    {
        get => this.Handlers.Count;
    }

    private SortedList<EventOrderKey, EventHandler<TEventArgs>> Handlers { get; } = new();

    private int InvocationDepth { get; set; }

    /// <summary>
    ///     Adds a new handler for this event.
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
    ///     Removes a handler from this event.
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
    ///     Invokes all registered handlers.
    /// </summary>
    /// <param name="eventArgs">The event arguments to send to handlers.</param>
    protected void InvokeAll(TEventArgs eventArgs)
    {
        var depth = ++this.InvocationDepth;
        foreach (var handler in this.Handlers.Values)
        {
            try
            {
                handler.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append($"This mod failed in {handler.Method.Name}");
                if (handler.Method.DeclaringType?.Name is not null)
                {
                    sb.Append($" of {handler.Method.DeclaringType.Name}. Technical details:\n");
                }

                sb.Append(ex.Message);
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                if (frame?.GetFileName() is { } fileName)
                {
                    var line = frame.GetFileLineNumber().ToString();
                    sb.Append($" at {fileName}:line {line}");
                }

                Log.Error(sb.ToString());
            }

            if (depth != this.InvocationDepth)
            {
                break;
            }
        }
    }
}