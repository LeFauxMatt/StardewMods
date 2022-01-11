namespace FuryCore.Models;

using System;
using StardewModdingAPI.Events;

/// <inheritdoc />
internal readonly struct EventOrderKey : IComparable<EventOrderKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventOrderKey"/> struct.
    /// </summary>
    /// <param name="eventPriority"></param>
    public EventOrderKey(EventPriority eventPriority)
    {
        this.EventPriority = (int)eventPriority;
        this.RegistrationOrder = EventOrderKey.TotalRegistrations++;
    }

    public int EventPriority { get; }

    public int RegistrationOrder { get; }

    private static int TotalRegistrations { get; set; }

    public void Deconstruct(out int eventPriority, out int registrationOrder)
    {
        eventPriority = this.EventPriority;
        registrationOrder = this.RegistrationOrder;
    }

    /// <inheritdoc/>
    public int CompareTo(EventOrderKey other)
    {
        var (eventPriority, registrationOrder) = other;
        var priorityCompare = -this.EventPriority.CompareTo(eventPriority);
        return priorityCompare != 0
            ? priorityCompare
            : this.RegistrationOrder.CompareTo(registrationOrder);
    }
}