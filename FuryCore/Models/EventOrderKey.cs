namespace StardewMods.FuryCore.Models;

using System;
using StardewModdingAPI.Events;

/// <inheritdoc />
internal readonly struct EventOrderKey : IComparable<EventOrderKey>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventOrderKey" /> struct.
    /// </summary>
    /// <param name="eventPriority">The event priority for this method handler.</param>
    public EventOrderKey(EventPriority eventPriority)
    {
        this.EventPriority = (int)eventPriority;
        this.RegistrationOrder = EventOrderKey.TotalRegistrations++;
    }

    private static int TotalRegistrations { get; set; }

    private int EventPriority { get; }

    private int RegistrationOrder { get; }

    /// <inheritdoc />
    public int CompareTo(EventOrderKey other)
    {
        var priorityCompare = -this.EventPriority.CompareTo(other.EventPriority);
        return priorityCompare != 0
            ? priorityCompare
            : this.RegistrationOrder.CompareTo(other.RegistrationOrder);
    }
}