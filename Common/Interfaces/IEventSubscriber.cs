namespace StardewMods.Common.Interfaces;

/// <summary>
/// Represents an event subscriber service.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to an event handler.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="handler">The event handler to subscribe.</param>
    void Subscribe<TEventArgs>(Action<TEventArgs> handler);

    /// <summary>
    /// Unsubscribes an event handler from an event.
    /// </summary>
    /// <param name="handler">The event handler to unsubscribe.</param>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    void Unsubscribe<TEventArgs>(Action<TEventArgs> handler);
}