namespace FuryCore.Attributes;

using System;
using StardewModdingAPI.Events;

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Method)]
public class SortedEventPriorityAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SortedEventPriorityAttribute" /> class.
    /// </summary>
    /// <param name="priority"></param>
    public SortedEventPriorityAttribute(EventPriority priority)
    {
        this.Priority = priority;
    }

    internal EventPriority Priority { get; }
}