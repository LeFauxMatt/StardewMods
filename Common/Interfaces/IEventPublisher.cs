namespace StardewMods.Common.Interfaces;

/// <summary>
/// Represents an event publisher service.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event with the given event arguments.
    /// </summary>
    /// <typeparam name="TEventArgs">The event argument implementation.</typeparam>
    /// <param name="eventArgs">The event arguments to publish.</param>
    /// <remarks>
    /// This method is used to raise an event with the provided event arguments. It can be used to notify subscribers of an event.
    /// </remarks>
    public void Publish<TEventArgs>(TEventArgs eventArgs)
        where TEventArgs : EventArgs;

    /// <summary>
    /// Publishes an event with the given event arguments.
    /// </summary>
    /// <typeparam name="TEventType">The type of the event arguments.</typeparam>
    /// <typeparam name="TEventArgs">The event argument implementation.</typeparam>
    /// <param name="eventArgs">The event arguments to publish.</param>
    /// <remarks>
    /// This method is used to raise an event with the provided event arguments. It can be used to notify subscribers of an event.
    /// </remarks>
    public void Publish<TEventType, TEventArgs>(TEventArgs eventArgs)
        where TEventArgs : EventArgs, TEventType;
}