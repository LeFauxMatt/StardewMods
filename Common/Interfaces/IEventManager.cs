namespace StardewMods.Common.Interfaces;

/// <summary>
/// Represents a service for publishing and subscribing to events.
/// </summary>
public interface IEventManager : IEventSubscriber, IEventPublisher
{
}